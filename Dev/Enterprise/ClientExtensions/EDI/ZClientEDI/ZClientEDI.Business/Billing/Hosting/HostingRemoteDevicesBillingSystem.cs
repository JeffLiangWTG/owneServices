using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingRemoteDevicesBillingSystem : BillingSystemWithDatabase
	{
		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.HostingRemoteDevices; }
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<HostingUsage> result = new List<HostingUsage>();
			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				var usingParty = new UsingParty(chargeableUsage);
				var usageOwner = usingParty.UsageOwnerLicence;
				var siteLive = SiteLive(usageOwner.Database);
				if (siteLive.IsEmpty || siteLive < chargeableUsage.U1_PeriodStart.AddDays(15))
				{
					var usage = new HostingUsage(Context.Factory, chargeableUsage);
					result.Add(usage);
				}
			}

			return result.ToArray();
		}

		static ZDateTime SiteLive(LicenceDatabase db)
		{
			return db.LicHeadersForAllCompanies.Cast<LicenceHeader>()
					.Select(x => x.LA_AgreedLiveDate)
					.Aggregate((a, b) => { return a.IsEmpty ? b : (b.IsEmpty ? a : (a < b ? a : b)); });
		}

		protected override SystemBill CreateSystemBill()
		{
			return new HostingBill(Context.Factory);
		}

		#region Load Raw Usage

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "WiseCloud Premium - Remote Devices";

			rawUsage.Summary.Header.Column1 = "Printer Name";
			rawUsage.Summary.Header.Column2 = "Print Server Name";

			LoadRawUsage(context, (reader) =>
			{
				var printerName = (string)reader["PrinterName"];
				var printServerName = (string)reader["PrintServerName"];

				SummaryLine line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = printerName;
				line.Column2 = printServerName;
			});

			return rawUsage;
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "WiseCloud Premium - Remote Devices";

			rawUsage.Summary.Header.Column1 = "Printer Name";
			rawUsage.Summary.Header.Column2 = "Print Server Name";

			LoadRawUsage(context, (reader) =>
			{
				var printerName = (string)reader["PrinterName"];
				var printServerName = (string)reader["PrintServerName"];

				SummaryLine line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = printerName;
				line.Column2 = printServerName;
			});

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var headerColumns = new string[] { "Printer Name", "Print Server Name" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			LoadRawUsage(context, (reader) =>
			{
				var printerName = (string)reader["PrinterName"];
				var printServerName = (string)reader["PrintServerName"];

				string[] dataValues = new string[] { printerName, printServerName };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			LoadRawUsage(context, (reader) =>
			{
				var printerName = (string)reader["PrinterName"];
				var printServerName = (string)reader["PrintServerName"];
				ZDateTime dateCaptured = (DateTime)reader["DateCaptured"];
				string reference = (printerName + ' ' + printServerName).Trim();
				writer.WriteCsvUsageReport(dateCaptured, "", "", "", reference, context.PriceItemCode, context.PriceItemDescription, 1);
			});
		}

		void LoadRawUsage(BillingLoadRawUsageContext context, Action<IDataReader> readerAction)
		{
			if (context.PeriodAsInt < 202206)
			{
				GetUsageProvider().LoadRawUsage(context, readerAction);
				return;
			}
			else
			{
				using (var command = GetRawUsageQuery(context))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						readerAction(reader);
					}
				}
			}
		}

		#region SQL

		protected override string Query_Raw_Usage => @"
if @Period < 202206
begin
	THROW 51000, 'NotImplementedException', 1;
end
else
begin
	SELECT
		PrinterName = IIF(@PriceItemCode = '#HS', '', TX_Reference1),
		PrintServerName = IIF(@PriceItemCode = '#HR', '', TX_Reference1),
		DateCaptured = TX_ServiceOccuredUTC
	FROM
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'STL'
		AND TX_Period = @Period
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_PriceItemCode = 
			CASE @PriceItemCode 
				WHEN '#HS' THEN 'PRS'
				WHEN '#HR' THEN 'PRT'
				ELSE ''
			END
	ORDER By TX_ServiceOccuredUTC
end
";

		#endregion

		#endregion

		protected virtual ExternalChargeableUsageProvider GetUsageProvider() => new EdiRemoteDevicesChargeableUsageProvider();

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context) => throw new NotImplementedException();

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context) => throw new NotImplementedException();
	}
}

