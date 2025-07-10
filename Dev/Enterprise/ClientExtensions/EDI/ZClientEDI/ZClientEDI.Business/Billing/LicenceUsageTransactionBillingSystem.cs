using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Billing system for transactional usage that is reported in the licence usage report
	/// and so stored in the ediProd..LicenceUsage table.
	/// </summary>
	public class LicenceUsageTransactionBillingSystem : TransactionBillingSystem
	{
		public const string ClientChargeableUsageCode = "CPT";

		public LicenceUsageTransactionBillingSystem(string code, string systemDescription = null)
		{
			this.systemCode = code;
			this.systemDescription = systemDescription ?? BillingConstants.BillingSystemList.GetDescriptionFromCode(code) ?? code;

			// Will cause the PriceItemUsage to have a SubCode that matches the UsageSubCode,
			// which in turn will cause EdiBilledUsage.BU9_UsageSubCode to be set.
			IncludeSubCodeInSystemUsage = true;
		}

		readonly string systemCode;
		readonly string systemDescription;

		public override string SystemCode
		{
			get { return systemCode; }
		}

		protected override void AddCodeFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, ClientChargeableUsageCode);
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, SystemCode);
		}

		#region System Bill

		protected override SystemBill CreateSystemBill()
		{
			var bill = new TransactionalSystemBill(SystemCode, Context.Factory);
			bill.UsageCodeForBilledUsage = ClientChargeableUsageCode;
			return bill;
		}

		#endregion

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);
			LoadRawUsageCommon(reader, rawUsage.Summary, false, context);
			return rawUsage;
		}

		void LoadRawUsageCommon(IDataReader reader, SummarySection summary, bool isStl, BillingLoadRawUsageContext context)
		{
			summary.Header.Column1 = "Company Code";
			summary.Header.Column2 = "User";
			if (isStl)
			{
				summary.Header.Column3 = "Usage Description";
				summary.Header.Column4 = "Price Code";
				summary.Header.Column5 = "Price Item Description";
				summary.Header.Column9 = "Date";
			}
			else
			{
				summary.Header.Column3 = "Price Code";
				summary.Header.Column4 = "Price Item Description";
				summary.Header.Column5 = "Date";
			}

			while (reader.Read())
			{
				DateTime date = (DateTime)reader[ClientLicenceUsageSchema.Constants.LX_UsageTime];
				string userName = (string)reader[ClientStaffSchema.Constants.LS_FullName];
				string companyCode = (string)reader[ClientCompanySchema.Constants.LCC_Code];

				SummaryLine line = summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = userName;
				var dateText = date.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				if (isStl)
				{
					line.Column3 = systemDescription;
					line.Column4 = context.PriceItemCode;
					line.Column5 = context.PriceItemDescription;
					line.Column9 = dateText;
				}
				else
				{
					line.Column3 = context.PriceItemCode;
					line.Column4 = context.PriceItemDescription;
					line.Column5 = dateText;
				}
			}
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			LoadRawUsageCommon(reader, rawUsage.Summary, true, context);
			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var headerColumns = new string[] { "Company Code", "User", "Usage Description", "Price Code", "Price Item Description", "Date" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					DateTime date = (DateTime)reader[ClientLicenceUsageSchema.Constants.LX_UsageTime];
					string userName = (string)reader[ClientStaffSchema.Constants.LS_FullName];

					var companyCode = (string)reader[ClientCompanySchema.Constants.LCC_Code];
					var dataValues = new string[] { companyCode, userName, systemDescription, context.PriceItemCode, context.PriceItemDescription, date.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) };

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
					ZDateTime usageTime = (DateTime)reader[ClientLicenceUsageSchema.Constants.LX_UsageTime];
					string staff = (string)reader[ClientStaffSchema.Constants.LS_Code];
					string companyCode = (string)reader[ClientCompanySchema.Constants.LCC_Code];

					writer.WriteCsvUsageReport(usageTime, companyCode, "", staff, systemDescription, context.PriceItemCode, context.PriceItemDescription, 1);
				}
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return string.Format(CultureInfo.InvariantCulture, sql_Raw_Usage, SystemCode, "{0}"); }
		}

		readonly string sql_Raw_Usage = @"
SELECT 
	" + ClientLicenceUsageSchema.Constants.LX_UsageTime + @",
	" + ClientStaffSchema.Constants.LS_FullName + @",
	" + ClientCompanySchema.Constants.LCC_Code + @",
	" + ClientStaffSchema.Constants.LS_Code + @"
FROM 
	dbo.ClientLicenceUsage
	join dbo.ClientCompany ON LX_LCC = LCC_PK
	join dbo.ClientStaff ON LX_LS = LS_PK AND LS_LD = LCC_LD
WHERE
	LX_ModuleCode = '{0}' 
	AND LX_UsageTime >= @DateFrom AND LX_UsageTime < @DateTo
	AND (LCC_PK = @ClientCompanyPk OR LCC_LD = @DatabasePk)
ORDER By
	" + ClientLicenceUsageSchema.Constants.LX_UsageTime + @"
";

		#endregion
	}
}

