using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ForwardAirBillingSystem : TransactionBillingSystem
	{
		public ForwardAirBillingSystem()
		{
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ForwardAir; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionSystemBillEx(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new ForwardAirUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Consol";
			rawUsage.Summary.Header.Column3 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string consol = (string)reader["TX_Reference2"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
				line.Column2 = consol;
				line.Column3 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Consol";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string consol = (string)reader["TX_Reference2"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = consol;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Consol", "Message Time (UTC)" };
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

					string consol = (string)reader["TX_Reference2"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, consol, messageTime.ToLongTimeString() };
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

					string consol = (string)reader["TX_Reference2"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, consol, context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"SELECT 
	TX_ClientID,
	CompanyCode = ISNULL(LCC_Code, ''),
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'FWA'
	AND TX_PriceItemCode = 'FWA'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion
	}
}

