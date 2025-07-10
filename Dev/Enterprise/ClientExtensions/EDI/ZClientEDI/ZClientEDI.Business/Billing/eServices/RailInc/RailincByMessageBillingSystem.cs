using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class RailincByMessageBillingSystem : TransactionBillingSystem
	{
		public RailincByMessageBillingSystem()
		{
			// Will cause the PriceItemUsage to have a SubCode that matches the UsageSubCode,
			// which in turn will cause EdiBilledUsage.BU9_UsageSubCode to be set.
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.RailincByMessage; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new RailincBill(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new RailincUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "eHub Client ID";
			rawUsage.Summary.Header.Column2 = "Consol";
			rawUsage.Summary.Header.Column3 = "Container";
			rawUsage.Summary.Header.Column4 = "Message Time (UTC)";

			while (reader.Read())
			{
				string eHubClientId = (string)reader["ClientID"];
				string consol = (string)reader["Consol"];
				string container = (string)reader["Container"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = eHubClientId;
				line.Column2 = consol;
				line.Column3 = container;
				line.Column4 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Consol";
			rawUsage.Summary.Header.Column3 = "Container";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				ZString client = (string)reader["ClientID"];
				string consol = (string)reader["Consol"];
				string container = (string)reader["Container"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = client.SubstringSafe(3, 3);
				line.Column2 = consol;
				line.Column3 = container;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";
			string[] headerColumns = new string[] { clientHeaderColumn, "Consol", "Container", "Message Time (UTC)" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					ZString client = (string)reader["ClientID"];
					if (isStlBilling)
					{
						client = client.SubstringSafe(3, 3);
					}

					string consol = (string)reader["Consol"];
					string container = (string)reader["Container"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					string[] dataValues = new string[] { client, consol, container, messageTime.ToLongTimeString() };
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
					ZString client = (string)reader["ClientID"];
					if (isStlBilling)
					{
						client = client.SubstringSafe(3, 3);
					}

					string consol = (string)reader["Consol"];
					string container = (string)reader["Container"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(consol, ' ', container).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage_Message; }
		}

		const string sql_Raw_Usage_Message = @"
SELECT
	TX_ClientID as ClientID,
	ISNULL(TX_Reference2, '') as Consol, 
	ISNULL(TX_Reference3, '') as Container,
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
WHERE
	TX_Category = 'RIM'
	AND TX_PriceItemCode = 'RIC'
	AND TX_SystemId = @DatabaseId 
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion
	}
}
