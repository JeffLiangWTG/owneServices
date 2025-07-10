using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class OceanTracingBillingSystem : TransactionBillingSystem
	{
		public OceanTracingBillingSystem()
		{
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.OceanTracing; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new OceanTracingBill(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new OceanTracingUsage(systemCode, factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Event Time";
			rawUsage.Summary.Header.Column3 = "Event Type";
			rawUsage.Summary.Header.Column4 = "Container Number";
			rawUsage.Summary.Header.Column5 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string eventTime = (string)reader["TX_Reference3"];
				string eventType = (string)reader["TX_Reference2"];
				string containerNumber = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
				line.Column2 = eventTime;
				line.Column3 = GetEventTypeDescription(eventType);
				line.Column4 = containerNumber;
				line.Column5 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Event Time";
			rawUsage.Summary.Header.Column3 = "Event Type";
			rawUsage.Summary.Header.Column4 = "Container Number";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string eventTime = (string)reader["TX_Reference3"];
				string eventType = (string)reader["TX_Reference2"];
				string containerNumber = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = eventTime;
				line.Column3 = GetEventTypeDescription(eventType);
				line.Column4 = containerNumber;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Event Time", "Event Type", "Container Number", "Message Time (UTC)" };
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

					string eventTime = (string)reader["TX_Reference3"];
					string eventType = (string)reader["TX_Reference2"];
					string containerNumber = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, eventTime, GetEventTypeDescription(eventType), containerNumber, messageTime.ToLongTimeString() };
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

					string eventTime = (string)reader["TX_Reference3"];
					string eventType = (string)reader["TX_Reference2"];
					string containerNumber = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(eventTime, ' ', GetEventTypeDescription(eventType), ' ', containerNumber).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}
		static string GetEventTypeDescription(string eventTypeCode)
		{
			string result = eventTypeCode;
			if (eventTypeCode.StartsWith("3", StringComparison.OrdinalIgnoreCase))
			{
				result += "(CODECO)";
			}
			else if (eventTypeCode.StartsWith("4", StringComparison.OrdinalIgnoreCase))
			{
				result += "(COARRI)";
			}
			return result;
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
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_Reference4 = ISNULL(TX_Reference4, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'OCT'
	AND TX_PriceItemCode = 'OCT'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion
	}
}

