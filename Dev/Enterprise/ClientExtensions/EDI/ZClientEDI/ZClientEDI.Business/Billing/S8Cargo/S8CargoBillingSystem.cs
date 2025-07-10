using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class S8CargoBillingSystem : TransactionBillingSystem
	{
		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.S8Cargo; }
		}

		#endregion

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionalSystemBill(SystemCode, Context.Factory);
		}

		#endregion

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Message Type";
			rawUsage.Summary.Header.Column3 = "Reference";
			rawUsage.Summary.Header.Column4 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string messageType = (string)reader["TX_Reference2"];
				string reference = (string)reader["TX_Reference3"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
				line.Column2 = messageType;
				line.Column3 = reference;
				line.Column4 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Message Type";
			rawUsage.Summary.Header.Column3 = "Reference";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string messageType = (string)reader["TX_Reference2"];
				string reference = (string)reader["TX_Reference3"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = messageType;
				line.Column3 = reference;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Message Type", "Reference", "Message Time (UTC)" };
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

					string messageType = (string)reader["TX_Reference2"];
					string reference = (string)reader["TX_Reference3"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, messageType, reference, messageTime.ToLongTimeString() };
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

					string messageType = (string)reader["TX_Reference2"];
					string reference = (string)reader["TX_Reference3"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(messageType, ' ', reference).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return query_Raw_Usage; }
		}

		const string query_Raw_Usage =
@"SELECT 
	TX_ClientID,
	CompanyCode = ISNULL(LCC_Code, ''),
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'S8C'
	AND TX_PriceItemCode = 'S8C'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
	AND TX_Reference1 = 'Enterprise'
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion
	}
}

