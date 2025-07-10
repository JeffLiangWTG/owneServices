using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ERouter;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.ExDocs
{
	public class ExDocsBillingSystem : TransactionBillingSystem
	{
		public ExDocsBillingSystem()
			: base()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ExDocs; }
		}

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionalSystemBill(SystemCode, Context.Factory);
		}

		#endregion

		#region Raw Usages

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			var exDocsRaw = new SystemCodeRawUsage(context, SystemCode);
			exDocsRaw.SummaryHeaderDescription = "Export Documents Transactions";

			exDocsRaw.Summary.Header.Column1 = "Message Type";
			exDocsRaw.Summary.Header.Column2 = "Transmitted";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string messageType = (string)reader["U2_Type"];
				string rfpMessageType = GetRfpMessageType(messageType);

				SummaryLine line = exDocsRaw.Summary.Lines.AddNew();
				line.Column1 = rfpMessageType;
				line.Column2 = transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return exDocsRaw;
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var exDocsRaw = new StlRawUsage(context);

			exDocsRaw.Summary.Header.Column1 = "Company Code";
			exDocsRaw.Summary.Header.Column2 = "Message Type";
			exDocsRaw.Summary.Header.Column9 = "Transmitted";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string messageType = (string)reader["U2_Type"];
				string companyCode = (string)reader["CompanyCode"];

				SummaryLine line = exDocsRaw.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = GetRfpMessageType(messageType);
				line.Column9 = transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return exDocsRaw;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string[] headerColumns;
			if (isStlBilling)
			{
				headerColumns = new string[] { "Company Code", "Message Type", "Transmitted" };
			}
			else
			{
				headerColumns = new string[] { "Message Type", "Transmitted" };
			}

			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string messageType = (string)reader["U2_Type"];

				string[] dataValues;
				if (isStlBilling)
				{
					string companyCode = (string)reader["CompanyCode"];
					dataValues = new string[] { companyCode, GetRfpMessageType(messageType), transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}
				else
				{
					dataValues = new string[] { GetRfpMessageType(messageType), transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}

				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				ZDateTime transmitDate = (DateTime)reader["U2_Date"];
				string messageType = (string)reader["U2_Type"];
				string companyCode = string.Empty;

				if (isStlBilling)
				{
					companyCode = (string)reader["CompanyCode"];
				}

				writer.WriteCsvUsageReport(transmitDate, companyCode, "", "", GetRfpMessageType(messageType), context.PriceItemCode, context.PriceItemDescription, 1);
			});
		}

		string GetRfpMessageType(string messageType)
		{
			// Note: these codes come from 
			// C:\Dev\Enterprise\Product\Operations\Customs\AU\Declaration.Business\Business\EXDOCLists\EXDOCMessageTypeCodes.cs
			switch (messageType)
			{
				case "LDG": return "Lodge RFP";
				case "TRF": return "Transfer RFP";
				case "ATR": return "Accept RFP";
				default: return "RFP Message";
			}
		}

		protected override string Query_Raw_Usage => "SELECT 1";

		public const string ApplicationCode = "EXD";

		protected virtual ExternalChargeableUsageProvider GetUsageProvider() => new EdiERouterChargeableUsageProvider(ApplicationCode);

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

