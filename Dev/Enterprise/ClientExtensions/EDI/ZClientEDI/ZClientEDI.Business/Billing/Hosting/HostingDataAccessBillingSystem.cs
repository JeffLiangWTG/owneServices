using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingDataAccessBillingSystem : BillingSystemWithDatabase
	{
		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.HostingDataAccess; }
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<HostingUsage> result = new List<HostingUsage>();
			foreach (var usageByPeriod in chargeableUsages.GroupBy(x => x.U1_PeriodStart))
			{
				foreach (var usageByProductionDb in usageByPeriod.GroupBy(x => ProductionDatabasePk(x)))
				{
					var firstUsage = usageByProductionDb.FirstOrDefault(x => x.Database.LD_LicenceType == DatabaseTypes.Codes.Production) ?? usageByProductionDb.First();

					var usingParty = new UsingParty(firstUsage);
					int sizeMB = usageByProductionDb.Sum(x => x.U1_UnitCountAsInt);

					var usage = new HostingDataAccessUsage(Context.Factory, usingParty, firstUsage.U1_PeriodStart, firstUsage.U1_Code, firstUsage.U1_SubCode, sizeMB);

					if (Context.IsPreviewOnly)
					{
						usage.SetPreviewOnly(Context.PreviewPriceHeader);
					}
					if (usage.UnitCount != 0)
					{
						usage.ChargeableUsagePKs.AddRange(usageByProductionDb.Select(x => x.PK));
						result.Add(usage);
					}
				}
			}

			return result.ToArray();
		}

		ZGuid ProductionDatabasePk(ClientChargeableUsage usage)
		{
			var db = usage.Database;
			return db.LD_LicenceType == DatabaseTypes.Codes.Production || db.LD_LD_ParentDatabase.IsEmpty
				? db.PK
				: db.LD_LD_ParentDatabase;
		}

		protected override SystemBill CreateSystemBill()
		{
			return new HostingDataAccessBill(Context.Factory);
		}

		#region Load Raw Usage

		const string RawUsageSummaryDescription = "WiseCloud ReadOnly Access - Data Usage";

		static void SetRawUsageColumnNames(SummaryLine header)
		{
			header.Column1 = "MB";
			header.Column2 = "Source IP";
			header.Column3 = "Destination IP";
			header.Column4 = "Start Time (UTC)";
			header.Column5 = "End Time (UTC)";
		}

		void PopulateRawUsage(System.Data.IDataReader reader, SummaryLineCollection lines)
		{
			while (reader.Read())
			{
				int col = 0;
				var units = reader.GetInt32(col++);
				var ip1 = reader.GetString(col++);
				var ip2 = reader.GetString(col++);
				var maxTimeStampString = reader.GetString(col++);
				ZDateTime time = reader.GetDateTime(col++);

				SummaryLine line = lines.AddNew();
				line.Column1 = units.ToString(CultureInfo.InvariantCulture);
				line.Column2 = ip1;
				line.Column3 = ip2;
				line.Column4 = time.ToLongTimeString();
				line.Column5 = maxTimeStampString;
			}
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = RawUsageSummaryDescription;
			SetRawUsageColumnNames(rawUsage.Summary.Header);
			PopulateRawUsage(reader, rawUsage.Summary.Lines);
			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = RawUsageSummaryDescription;
			SetRawUsageColumnNames(rawUsage.Summary.Header);
			PopulateRawUsage(reader, rawUsage.Summary.Lines);
			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var headerColumns = new string[] { "MB", "Source IP", "Destination IP", "Start Time (UTC)", "End Time (UTC)" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					int col = 0;
					var units = reader.GetInt32(col++);
					var ip1 = reader.GetString(col++);
					var ip2 = reader.GetString(col++);
					var maxTimeStampString = reader.GetString(col++);
					ZDateTime time = reader.GetDateTime(col++);

					string[] dataValues = new string[] { units.ToString(CultureInfo.InvariantCulture), ip1, ip2, time.ToLongTimeString(), maxTimeStampString };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					int col = 0;
					var units = reader.GetInt32(col++);
					var ip1 = reader.GetString(col++);
					var ip2 = reader.GetString(col++);
					var maxTimeStampString = reader.GetString(col++);
					ZDateTime time = reader.GetDateTime(col++);
					var branchCode = reader.GetString(col++);
					var staff = reader.GetString(col++);

					writer.WriteCsvUsageReport(time, "", branchCode, staff, string.Concat(ip1, ' ', ip2, ' ', maxTimeStampString).Trim(), context.PriceItemCode, context.PriceItemDescription, units);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"SELECT 
	TX_BillableCount,
	TX_Reference1 = ISNULL(TX_Reference1, ''),
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
WHERE
	TX_Category = 'HOS'
	AND TX_PriceItemCode = '#HG'
	AND TX_SystemId = @DatabaseId
	AND TX_Period = @Period
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion

		#region Accumulate Unbilled Months

		protected override ZDateTime EarliestUsageToAccumulate
		{
			get { return Context.PeriodStart.AddMonths(-1); }
		}

		#endregion
	}
}

