using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class GlobalContainerTrackingBillingSystem : TransactionBillingSystem
	{
		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.GlobalContainerTracking; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new GlobalContainerTrackingBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new GlobalContainerTrackingUsage(factory, user, periodStart);
		}

		#region Create System Usages

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var hasPriceItems = HasPriceItems;
			var result = new List<GlobalContainerTrackingUsage>(chargeableUsages.Length);

			foreach (var dateGroup in chargeableUsages.Where(x => x.U1_UnitCount != 0).GroupBy(x => x.U1_PeriodStart))
			{
				var periodStart = dateGroup.Key;

				foreach (var databaseGroup in dateGroup.Where(s => s.Database != null).GroupBy(s => s.Database))
				{
					var dbUsageCount = databaseGroup.Sum(u => u.U1_UnitCountAsInt);

					if (hasPriceItems)
					{
						foreach (var clientCompanyGroup in databaseGroup.GroupBy(u => u.ClientCompany))
						{
							var usage = CreateBreakDownUsage(clientCompanyGroup, periodStart, dbUsageCount);
							foreach (var chargeableUsage in clientCompanyGroup)
							{
								usage.ChargeableUsagePKs.Add(chargeableUsage.PK);
							}
							result.Add(usage);
						}
					}
					else
					{
						//Usages without matched price items should still be shown
						foreach (var chargeableUsage in databaseGroup)
						{
							var usage = new GlobalContainerTrackingUsage(Context.Factory, new UsingParty(chargeableUsage), periodStart);
							usage.ChargeableUsagePKs.Add(chargeableUsage.PK);
							usage.TransactionCount = chargeableUsage.U1_UnitCountAsInt;
							result.Add(usage);
						}
					}
				}
			}

			return result.ToArray();
		}

		bool HasPriceItems => LicenceCompany.StandardPricesCompany?
				.PriceHeaderForDate(new DateTime(Context.PeriodStart.Year, Context.PeriodStart.Month, 15), SystemCode)?
				.LocalOrStandardItems?.Where(x => x.L7_Code == SystemCode).Any() ?? false;

		GlobalContainerTrackingUsage CreateBreakDownUsage(IGrouping<ClientCompany, ClientChargeableUsage> clientCompanyGroup, ZDateTime periodStart, int dbUsageCount)
		{
			GlobalContainerTrackingUsage result = null;

			var user = new UsingParty(clientCompanyGroup.Key);
			var clientCompanyUsageCount = clientCompanyGroup.Sum(u => u.U1_UnitCountAsInt);

			result = new GlobalContainerTrackingUsage(Context.Factory, user, periodStart);
			result.TransactionCount = clientCompanyUsageCount;
			result.CalculatePriceItemForUnitBreak(dbUsageCount);
			SetUsageTransactionDescription(result, result.PriceItem);
			result.TransactionPrice = result.PriceItem.L7_Price;
			result.CompanyUsageCount = clientCompanyUsageCount;
			result.DatabaseUsageCount = dbUsageCount;

			return result;
		}

		static void SetUsageTransactionDescription(GlobalContainerTrackingUsage usage, ClientLicencePriceItem priceItem)
		{
			var lastPriceItem = priceItem.Parent.Items.FindAllByCode(priceItem.L7_Code)
				.Where(x => x.L7_UnitBreak > priceItem.L7_UnitBreak).OrderBy(x => x.L7_UnitBreak).FirstOrDefault();

			if (lastPriceItem == null)
			{
				usage.TransactionDescription = "Above " + priceItem.L7_UnitBreak + " unique container tracked";
			}
			else if (priceItem.L7_UnitBreak == 0)
			{
				usage.TransactionDescription = "Below " + lastPriceItem.L7_UnitBreak + " unique container tracked";
			}
			else
			{
				usage.TransactionDescription = (priceItem.L7_UnitBreak + 1).ToString(CultureInfo.InvariantCulture)
					+ " - " + ((int)lastPriceItem.L7_UnitBreak).ToString(CultureInfo.InvariantCulture) + " unique container tracked";
			}
		}

		#endregion

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Container Number";
			rawUsage.Summary.Header.Column3 = "Job Number";
			rawUsage.Summary.Header.Column4 = "CBR/MBN";
			rawUsage.Summary.Header.Column5 = "Carrier Code";
			rawUsage.Summary.Header.Column6 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string containerNumber = (string)reader["TX_Reference1"];
				string jobNumber = (string)reader["TX_Reference2"];
				string refNumber = (string)reader["TX_Reference3"];
				string carrierCode = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
				line.Column2 = containerNumber;
				line.Column3 = jobNumber;
				line.Column4 = refNumber;
				line.Column5 = carrierCode;
				line.Column6 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption
			return null;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption

			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Container Number", "Job Number", "CBR/MBN", "Carrier Code", "Message Time (UTC)" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string client;
					if (isStlBilling)
					{
						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}
					string containerNumber = (string)reader["TX_Reference1"];
					string jobNumber = (string)reader["TX_Reference2"];
					string refNumber = (string)reader["TX_Reference3"];
					string carrierCode = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, containerNumber, jobNumber, refNumber, carrierCode, messageTime.ToLongTimeString() };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			var priceCode = context.PriceItemCode;
			var priceDescription = context.PriceItemDescription;

			if (priceCode.IsEmpty)
			{
				priceCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			}

			if (priceDescription.IsEmpty)
			{
				priceDescription = BillingConstants.GetBillingSystemList().GetDescriptionFromCode(BillingConstants.BillingSystem.GlobalContainerTracking);
			}

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string client;
					if (isStlBilling)
					{
						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}
					string containerNumber = (string)reader["TX_Reference1"];
					string jobNumber = (string)reader["TX_Reference2"];
					string refNumber = (string)reader["TX_Reference3"];
					string carrierCode = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, (containerNumber + ' ' + jobNumber + ' ' + refNumber + ' ' + carrierCode).Trim(), priceCode, priceDescription, billableCount);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"
if @Period >= 201908
begin
	SELECT
		TX_ClientId,
		CompanyCode = ISNULL(LCC_Code, ''),
		TX_Reference1, 
		TX_Reference2 = ISNULL(TX_Reference2, ''), 
		TX_Reference3 = ISNULL(TX_Reference3, ''), 
		TX_Reference4 = ISNULL(TX_Reference4, ''),
		TX_ServiceOccuredUTC,
		ISNULL(TX_Branch, '') TX_Branch,
		TX_BillableCount,
		TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
	FROM
		dbo.BillingViewChargeable
		LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
	WHERE
		TX_Category = 'CTR'
		AND TX_PriceItemCode = 'CTO'
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_Period = @Period
	ORDER BY
		TX_ClientID, TX_ServiceOccuredUTC
end
else if @Period >= 201607
begin
	SELECT
		TX_ClientId,
		CompanyCode = ISNULL(LCC_Code, ''),
		TX_Reference1, 
		TX_Reference2 = ISNULL(TX_Reference2, ''), 
		TX_Reference3 = ISNULL(TX_Reference3, ''), 
		TX_Reference4 = ISNULL(TX_Reference4, ''),
		TX_ServiceOccuredUTC,
		ISNULL(TX_Branch, '') TX_Branch,
		TX_BillableCount,
		TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
	FROM
		dbo.BillingViewChargeable
		LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
	WHERE
		TX_Category = 'CTR'
		AND TX_PriceItemCode in ('CTO', 'CTV', 'CTI')
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_Period = @Period
		AND (TX_Reference5 is null or TX_Reference5 in ('', 'Full'))
	ORDER BY
		TX_ClientID, TX_ServiceOccuredUTC
end
else
begin
	SELECT 
		TX_ClientID,
		CompanyCode = ISNULL(LCC_Code, ''),
		TX_Reference1, 
		TX_Reference2 = ISNULL(TX_Reference2, ''),
		TX_Reference3 = ISNULL(TX_Reference3, ''), 
		TX_Reference4 = ISNULL(TX_Reference4, ''),
		TX_ServiceOccuredUTC,
		ISNULL(TX_Branch, '') TX_Branch,
		TX_BillableCount,
		TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
	FROM
		dbo.BillingViewChargeable
		LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
	WHERE
		TX_Category = 'CTR'
		AND TX_PriceItemCode = 'CTR'
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_Period = @Period
	ORDER BY
		TX_ClientID, TX_ServiceOccuredUTC
end
";

		#endregion
	}
}

