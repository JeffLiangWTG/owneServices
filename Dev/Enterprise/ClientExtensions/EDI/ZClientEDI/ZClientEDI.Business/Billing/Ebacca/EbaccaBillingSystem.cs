using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ERouter;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Ebacca
{
	/// <summary>
	/// eBACCa (electronic Biosecurity Authorization/Clearance Certificate Application) usage
	/// </summary>
	/// <seealso cref="http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20090403.pdf"/>
	public class EbaccaBillingSystem : TransactionBillingSystem
	{
		public EbaccaBillingSystem()
			: base()
		{
		}

		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.eBACCA; }
		}

		#endregion

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionalSystemBill(SystemCode, Context.Factory);
		}

		#endregion

		#region Load Raw Usage

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			SystemCodeRawUsage ebaccaRaw = new SystemCodeRawUsage(context, SystemCode);
			ebaccaRaw.SummaryHeaderDescription = "eBACCa Transactions";

			ebaccaRaw.Summary.Header.Column1 = "Customs Code";
			ebaccaRaw.Summary.Header.Column2 = "Job";
			ebaccaRaw.Summary.Header.Column3 = "Transmitted";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string sender = (string)reader["U2_Sender"];
				string jobnumber = (string)reader["U2_Reference"];

				SummaryLine line = ebaccaRaw.Summary.Lines.AddNew();
				line.Column1 = sender;
				line.Column2 = jobnumber;
				line.Column3 = transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return ebaccaRaw;
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Customs Code";
			rawUsage.Summary.Header.Column3 = "Job";
			rawUsage.Summary.Header.Column9 = "Transmitted";

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string sender = (string)reader["U2_Sender"];
				string jobnumber = (string)reader["U2_Reference"];
				string companyCode = (string)reader["CompanyCode"];

				SummaryLine line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = sender;
				line.Column3 = jobnumber;
				line.Column9 = transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
			});

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string[] headerColumns;
			if (isStlBilling)
			{
				headerColumns = new string[] { "Company Code", "Customs Code", "Job", "Transmitted" };
			}
			else
			{
				headerColumns = new string[] { "Customs Code", "Job", "Transmitted" };
			}

			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				DateTime transmitDate = (DateTime)reader["U2_Date"];
				string sender = (string)reader["U2_Sender"];
				string jobnumber = (string)reader["U2_Reference"];

				string[] dataValues;
				if (isStlBilling)
				{
					string companyCode = (string)reader["CompanyCode"];
					dataValues = new string[] { companyCode, sender, jobnumber, transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}
				else
				{
					dataValues = new string[] { sender, jobnumber, transmitDate.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) };
				}

				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			GetUsageProvider().LoadRawUsage(context, (reader) =>
			{
				string companyCode = string.Empty;
				ZDateTime transmitDate = (DateTime)reader["U2_Date"];
				string sender = (string)reader["U2_Sender"];
				string jobnumber = (string)reader["U2_Reference"];

				if (isStlBilling)
				{
					companyCode = (string)reader["CompanyCode"];
				}

				writer.WriteCsvUsageReport(transmitDate, companyCode, "", "", string.Concat(sender, ' ', jobnumber).Trim(), context.PriceItemCode, context.PriceItemDescription, 1);
			});
		}

		#endregion

		#region SQL

		/// <summary>
		/// Note: EC_Sender is the NZ customs code and can map to multiple entries in eRouterAddressMap.sEdiAddress
		///       The multiple entries can differ only in forwarding email address. 
		///       This is probably due to an address for the old Deliverance system and an address
		///       for the new Enterprise system.
		/// Note: The company code in EC_Company code may not be the billing company
		///       since eBACCa eRouter messages are sent from a service task which may be logged in
		///       as another company
		/// Note: The way to distinguish test and production systems is via the eRouterAddressMap.
		///		E.g., Customs code 40189737C maps to: Toll Networks (NZ) Ltd T/A Toll Global Forwarding (NZ) 
		///		             while 51111115D maps to: Toll Networks (NZ) T/A Toll Global Forwarding TEST SYSTEM
		/// </summary>

		protected override string Query_Raw_Usage => "SELECT 1";

		public const string ApplicationCode = "NZM";

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

