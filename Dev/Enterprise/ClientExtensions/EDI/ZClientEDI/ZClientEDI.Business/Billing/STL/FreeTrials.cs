using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class FreeTrials
	{
		public FreeTrials()
		{
		}

		public void CreateFreeTrialsFromNewUsage(DateTime newUsageMonthAsDate, ILogger logger)
		{
			var trialConfig = EDIDataRegistry.Instance.StlFreeTrialDiscounts.Value;
			foreach (var discountGroup in trialConfig.Cast<ICodeDescription>().GroupBy(x => x.Code))
			{
				var table = new System.Data.DataTable();
				table.Locale = CultureInfo.InvariantCulture;

				using (var cmd = BuildFreeTrialCommand(
					newUsageMonthAsDate,
					discountGroup.Key,
					discountGroup.Select(x => ExtractCategoryAndPriceCode(x.Description))))
				{
					cmd.NewDataAdapter().Fill(table);
				}

				if (table.Rows.Count > 0)
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					for (int i = 0; i < table.Rows.Count; ++i)
					{
						var databasePk = (Guid)table.Rows[i]["pk"];
						var minServiceOccuredUtc = (DateTime)table.Rows[i]["MinServiceOccuredUTC"];
						var freeTrialDiscount = factory.New<DiscountLicenceSetting>();
						freeTrialDiscount.LS9_Name = discountGroup.Key;
						freeTrialDiscount.LS9_LD = databasePk;
						freeTrialDiscount.LS9_ValidFrom = newUsageMonthAsDate;
						int addMonths = minServiceOccuredUtc.Day <= 15 ? 1 : 2;
						freeTrialDiscount.LS9_ValidTo = newUsageMonthAsDate.AddMonths(addMonths).AddDays(-1);
						freeTrialDiscount.LS9_Comment = (NoResString)"System Created " + ZDateTime.UtcNow.ToString((NoResString)"u", CultureInfo.InvariantCulture);

						if (logger != null)
						{
							logger.Information((NoResString)"Free trial discount " + discountGroup.Key
								+ (NoResString)" created for server " + freeTrialDiscount.Database.EnterpriseCode + (NoResString)"-" + freeTrialDiscount.Database.LD_ServerCode
								+ (NoResString)" " + freeTrialDiscount.LS9_ValidFrom.ToISO8601ShortDateString() + (NoResString)" to " + freeTrialDiscount.LS9_ValidTo.ToISO8601ShortDateString());
						}
					}
					factory.Save();
				}
			}
		}

		static Tuple<string, string> ExtractCategoryAndPriceCode(string commaSeparatedText)
		{
			var codeList = commaSeparatedText.Split(',');
			return Tuple.Create(
				codeList.Length > 0 ? codeList[0].Trim() : "",
				codeList.Length > 1 ? codeList[1].Trim() : "");
		}

		#region SQL

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		DbCommand BuildFreeTrialCommand(DateTime periodStart, string discountName, IEnumerable<Tuple<string, string>> categoryPriceCodePairList)
		{
			var cmd = Db.Connection.Command("");
			var s = new StringBuilder();
			s.Append(SQL1);
			int index = 1;
			foreach (var pair in categoryPriceCodePairList)
			{
				var indexText = index.ToString(CultureInfo.InvariantCulture);
				var categoryParamName = (NoResString)"@category" + indexText;
				var priceCodeParamName = "@priceCode" + indexText;
				cmd.AddParameter(categoryParamName, System.Data.SqlDbType.VarChar, pair.Item1);
				cmd.AddParameter(priceCodeParamName, System.Data.SqlDbType.VarChar, pair.Item2);
				if (index > 1)
				{
					s.Append(", ");
				}
				s.Append("(" + categoryParamName + ", " + priceCodeParamName + ")");
				++index;
			}

			s.Append(SQL2);
			cmd.CommandText = s.ToString();
			cmd.AddParameter("@periodStartDate", System.Data.SqlDbType.DateTime, periodStart);
			cmd.AddParameter("@discountName", System.Data.SqlDbType.VarChar, discountName);
			return cmd;
		}

		const string SQL1 =
@"
declare @codes table(category char(3) not null, priceCode char(3) not null, primary key (category, priceCode))
insert @codes(category, priceCode)
values ";

		const string SQL2 =
@"
declare @db table (pk uniqueidentifier not null primary key, DatabaseNumber int not null)

insert @db(pk, DatabaseNumber)
select U1_LD, LD_DatabaseNumber
from 
(
	-- used this month
	select U1_LD
	from dbo.ClientChargeableUsage 
	join @codes on U1_Code = category and U1_SubCode = priceCode and U1_PeriodStart = @periodStartDate
	group by U1_LD
) u
cross apply
(
	select LiveDate = MIN(LA_AgreedLiveDate) from dbo.LicenceHeader where LA_LD = U1_LD
) live
outer apply
(
	select top 1 HasEarlierLiveUsage = 1
	from dbo.ClientChargeableUsage
	join @codes on U1_Code = category and U1_SubCode = priceCode
	where u.U1_LD = ClientChargeableUsage.U1_LD
		and (ClientChargeableUsage.U1_PeriodStart < @periodStartDate)
		and (ClientChargeableUsage.U1_PeriodStart >= LiveDate or LiveDate is null)
) earlier
outer apply
(
	select top 1 HasTrialDiscount = 1
	from dbo.EdiLicenceSetting
	where LS9_LD = U1_LD
		and LS9_Type = 'DIS'
		and LS9_Name = @discountName
) discount
cross apply (select LD_DatabaseNumber from dbo.LicenceDatabase where LD_PK = U1_LD) db
where (@periodStartDate >= LiveDate)
	and HasEarlierLiveUsage is null
	and HasTrialDiscount is null
	option(recompile)

select pk, MinServiceOccuredUTC = min(TX_ServiceOccuredUTC) 
from BillingViewChargeable
join @codes on TX_Category = category and TX_PriceItemCode = priceCode
join @db on TX_DatabaseNumber = DatabaseNumber
where TX_Period = YEAR(@periodStartDate) * 100 + MONTH(@periodStartDate)
group by pk
";

		#endregion
	}
}
