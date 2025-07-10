using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Client.EDI.Billing.Business
{
	class StlReportingBusinessObjectBYO : StlReportingBusinessObjectBase
	{
		public const string PriceItemCode = "BYO";
		readonly BillingLoadRawUsageContext Context;

		public StlReportingBusinessObjectBYO(BillingLoadRawUsageContext context)
		{
			Context = context;
		}

		public override void GetCsvUsageReport(Action<string> action)
		{
			var headerColumns = new string[] { "Device ID", "ID Hash", "Branch", "Count" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			var report = GetUsageReport(usageLine =>
			{
				var dataValues = new string[] { usageLine.DeviceID, usageLine.IDHash, usageLine.Branch, usageLine.Count.ToString(CultureInfo.InvariantCulture) };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});

			foreach (var summaryLine in report.SummaryLines)
			{
				var dataValues = new string[] { summaryLine.Caption, "", "", summaryLine.Count.ToString(CultureInfo.InvariantCulture) };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			}
		}

		public override void GetCsvUsageReport(ICsvUsageReportWriter writer)
		{
			var report = GetUsageReport(usageLine =>
			{
				var reference = $"{usageLine.DeviceID} {usageLine.IDHash}";
				writer.WriteCsvUsageReport(usageLine.UsageTime, "", usageLine.Branch, "", reference, Context.PriceItemCode, Context.PriceItemDescription, usageLine.Count);
			});

			foreach (var summaryLine in report.SummaryLines)
			{
				var reference = summaryLine.Caption;
				writer.WriteCsvUsageReport(Context.PeriodEndTimeUtc, "", "", "", reference, Context.PriceItemCode, summaryLine.Description, summaryLine.Count);
			}
		}

		public override StlRawUsage LoadStlRawUsage()
		{
			var rawUsage = new StlRawUsage(Context);
			rawUsage.Summary.Header.Column1 = "Device ID";
			rawUsage.Summary.Header.Column2 = "ID Hash";
			rawUsage.Summary.Header.Column3 = "Branch";
			rawUsage.Summary.Header.Column4 = "Count";

			var report = GetUsageReport(usageLine =>
			{
				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = usageLine.DeviceID;
				line.Column2 = usageLine.IDHash;
				line.Column3 = usageLine.Branch;
				line.Column4 = usageLine.Count.ToString(CultureInfo.InvariantCulture);
			});

			var lineSymbol = rawUsage.Summary.Lines.AddNew();
			lineSymbol.Column1 = new string('─', 17);
			lineSymbol.Column2 = new string('─', 17);
			lineSymbol.Column3 = new string('─', 5);
			lineSymbol.Column4 = new string('─', 5);

			foreach (var summaryLine in report.SummaryLines)
			{
				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = summaryLine.Caption;
				line.Column2 = " ";
				line.Column3 = " ";
				line.Column4 = summaryLine.Count.ToString(CultureInfo.InvariantCulture);
			}

			return rawUsage;
		}

		BYOReport GetUsageReport(Action<BYOReport.UsageLine> usageLineAction)
		{
			var report = new BYOReport();
			var connection = GetConnectionForReader();
			using (var cmd = connection.Command(RF2UsageQuery))
			{
				cmd.AddParameter("@DatabaseId", System.Data.SqlDbType.VarChar, (string)Context.DatabaseId);
				cmd.AddParameter("@Period", System.Data.SqlDbType.Int, Context.PeriodAsInt);
				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var usageLine = new BYOReport.UsageLine()
						{
							DeviceID = reader.GetString(0),
							Count = reader.GetInt32(1),
							UsageTime = reader.GetDateTime(2),
							Branch = reader.GetString(3),
							IDHash = reader.GetString(4),
						};
						report.UsageLines.Add(usageLine);
						usageLineAction(usageLine);
					}
				}
			}

			var byoCount = Db.Connection.ExecuteScalar<int>(BYOUsageQuery, (cmd) =>
			{
				cmd.AddParameter("@DatabasePK", System.Data.SqlDbType.UniqueIdentifier, Context.DatabasePK.ToGuid());
				cmd.AddParameter("@PeriodStart", System.Data.SqlDbType.SmallDateTime, Context.Period.ToDateTime());
			});

			var totalRF2 = report.UsageLines.Sum(x => x.Count);
			report.SummaryLines.Add(new BYOReport.SummaryLine()
			{
				Caption = "Total Device Count",
				Description = "Total count of all active devices",
				Count = totalRF2
			});

			report.SummaryLines.Add(new BYOReport.SummaryLine()
			{
				Caption = "Registered Devices",
				Description = "Total count of all registered devices",
				Count = -(totalRF2 - byoCount),
			});

			report.SummaryLines.Add(new BYOReport.SummaryLine()
			{
				Caption = "Handheld Device Licenses",
				Description = "Numbers of active handheld device licenses",
				Count = byoCount,
			});

			return report;
		}

		class BYOReport
		{
			public class UsageLine
			{
				public string DeviceID { get; set; }
				public string IDHash { get; set; }
				public string Branch { get; set; }
				public int Count { get; set; }
				public DateTime UsageTime { get; set; }
			}

			public class SummaryLine
			{
				public string Caption { get; set; }
				public string Description { get; set; }
				public int Count { get; set; }
			}

			public List<UsageLine> UsageLines { get; } = new List<UsageLine>();
			public List<SummaryLine> SummaryLines { get; } = new List<SummaryLine>();
		}

		readonly string RF2UsageQuery =
@"SELECT DeviceID = ISNULL(TX_Reference2, ''), TX_BillableCount, TX_ServiceOccuredUTC,
			Branches = ISNULL(TX_Reference3, ''), IDHash = ISNULL(TX_Reference4, '')
FROM dbo.BillingViewChargeable
WHERE TX_Period = @Period
	AND TX_Category = 'STL'
	AND TX_PriceItemCode = 'RF2'
	AND TX_SystemId = @DatabaseId
	AND TX_BillableCount > 0
ORDER BY TX_ServiceOccuredUTC, TX_Reference2;";

		readonly string BYOUsageQuery =
@"
SELECT BYO_UnitCount = ISNULL(CAST(SUM(U1_UnitCount) AS INT), 0)
FROM dbo.ClientChargeableUsage
WHERE U1_Code = 'STL'
	AND U1_SubCode = 'BYO'
	AND U1_LD = @DatabasePK
	AND U1_PeriodStart = @PeriodStart;
";
	}
}
