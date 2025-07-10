using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Billing.Fax
{
	public class FaxBillingSystem : TransactionBillingSystem
	{
		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.Fax; }
		}

		#endregion

		#region Create System Bill & Usage

		protected override SystemBill CreateSystemBill()
		{
			return new FaxSystemBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new FaxUsage(factory, user, periodStart);
		}

		#endregion

		#region Load Raw Usage

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			var faxRawUsage = new SystemCodeRawUsage(context, SystemCode);
			faxRawUsage.Summary.Header.Column1 = "Fax";
			faxRawUsage.Summary.Header.Column2 = "Number Of Pages";
			faxRawUsage.Summary.Header.Column3 = "Sent";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime receivedDate = (DateTime)reader["ReceivedDateTime"];
				string faxNumber = (string)reader["FaxNumber"];
				int numberOfPages = (int)reader["NumberOfPages"];

				SummaryLine line = faxRawUsage.Summary.Lines.AddNew();
				line.Column1 = faxNumber;
				line.Column2 = numberOfPages.ToString(CultureInfo.InvariantCulture);
				line.Column3 = receivedDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return faxRawUsage;
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var faxRawUsage = new StlRawUsage(context);
			faxRawUsage.Summary.Header.Column1 = "Company Code";
			faxRawUsage.Summary.Header.Column2 = "Fax";
			faxRawUsage.Summary.Header.Column3 = "Number Of Pages";
			faxRawUsage.Summary.Header.Column9 = "Sent";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				string companyCode = reader["CompanyCode"].ToString();
				string faxNumber = reader["FaxNumber"].ToString();
				int numberOfPages = (int)reader["NumberOfPages"];
				DateTime receivedDate = (DateTime)reader["ReceivedDateTime"];

				SummaryLine line = faxRawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = faxNumber;
				line.Column3 = numberOfPages.ToString(CultureInfo.InvariantCulture);
				line.Column9 = receivedDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return faxRawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string[] headerColumns;
			if (isStlBilling)
			{
				headerColumns = new string[] { "Company Code", "Fax", "Number Of Pages", "Sent" };
			}
			else
			{
				headerColumns = new string[] { "Fax", "Number Of Pages", "Sent" };
			}

			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				string faxNumber = (string)reader["FaxNumber"];
				int numberOfPages = (int)reader["NumberOfPages"];
				DateTime receivedDate = (DateTime)reader["ReceivedDateTime"];

				string[] dataValues;
				if (isStlBilling)
				{
					string companyCode = (string)reader["CompanyCode"];
					dataValues = new string[] { companyCode, faxNumber, numberOfPages.ToString(CultureInfo.InvariantCulture), receivedDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}
				else
				{
					dataValues = new string[] { faxNumber, numberOfPages.ToString(CultureInfo.InvariantCulture), receivedDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}

				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				string faxNumber = (string)reader["FaxNumber"];
				int numberOfPages = (int)reader["NumberOfPages"];
				ZDateTime receivedDate = (DateTime)reader["ReceivedDateTime"];
				string companyCode = "";

				if (isStlBilling)
				{
					companyCode = (string)reader["CompanyCode"];
				}

				writer.WriteCsvUsageReport(receivedDate, companyCode, "", "", faxNumber, context.PriceItemCode, context.PriceItemDescription, numberOfPages);
			});
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage => "SELECT 1";

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "raw usages in remote database")]
		public override void OnChargeableUsageUpdate(BillingPeriod billingPeriod, ILogger logger)
			=> GetUsageProvider().BulkCopyUsages(billingPeriod, logger);

		protected virtual EdiFaxChargeableUsageProvider GetUsageProvider()
			=> new EdiFaxChargeableUsageProvider();

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}
	}
}

