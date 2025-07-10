using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ShippingPortMessagingBillingSystem : TransactionBillingSystem
	{
		public ShippingPortMessagingBillingSystem()
		{
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ShippingPortMessaging; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new ShippingPortMessagingBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new ShippingPortMessagingUsage("", factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Message Recipient";
			rawUsage.Summary.Header.Column3 = "Message Role";
			rawUsage.Summary.Header.Column4 = "Release / Container / Shipment";
			rawUsage.Summary.Header.Column5 = "Unit Count";
			rawUsage.Summary.Header.Column6 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientID = (string)reader["TX_ClientID"];
				string recipient = (string)reader["Recipient"];
				string role = (string)reader["Role"];
				string number = (string)reader["Number"];
				int unitCount = (int)reader["UnitCount"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientID;
				line.Column2 = recipient;
				line.Column3 = role;
				line.Column4 = number;
				line.Column5 = unitCount.ToString(CultureInfo.InvariantCulture);
				line.Column6 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Message Recipient";
			rawUsage.Summary.Header.Column3 = "Message Role";
			rawUsage.Summary.Header.Column4 = "Release / Container / Shipment";
			rawUsage.Summary.Header.Column5 = "Unit Count";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string recipient = (string)reader["Recipient"];
				string role = (string)reader["Role"];
				string number = (string)reader["Number"];
				int unitCount = (int)reader["UnitCount"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = recipient;
				line.Column3 = role;
				line.Column4 = number;
				line.Column5 = unitCount.ToString(CultureInfo.InvariantCulture);
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Message Recipient", "Message Role", "Release / Container / Shipment", "Unit Count", "Message Time (UTC)" };
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

					string recipient = (string)reader["Recipient"];
					string role = (string)reader["Role"];
					string number = (string)reader["Number"];
					int unitCount = (int)reader["UnitCount"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, recipient, role, number, unitCount.ToString(CultureInfo.InvariantCulture), messageTime.ToLongTimeString() };
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

					string recipient = (string)reader["Recipient"];
					string role = (string)reader["Role"];
					string number = (string)reader["Number"];
					int unitCount = (int)reader["UnitCount"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];
					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(recipient, ' ', role, ' ', number).Trim(), context.PriceItemCode, context.PriceItemDescription, unitCount);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"
SELECT 
	TX_ClientID, 
	CompanyCode = ISNULL(LCC_Code, ''),
	Number,
	Recipient,
	Role,
	UnitCount,
	TX_ServiceOccuredUTC,
	TX_Branch,
	TX_ClientStaffCode
FROM
	(
		SELECT 
			TX_ClientID, 
			TX_LCC, 
			Number = TX_Reference1,
			Recipient = ISNULL(TX_Reference2, ''),
			Role = ISNULL(TX_Reference3, ''),
			UnitCount = TX_BillableCount,
			TX_ServiceOccuredUTC,
			ISNULL(TX_Branch, '') TX_Branch,
			TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
		FROM
			dbo.BillingViewChargeable
		WHERE
			TX_Category = 'SPM'
			AND TX_PriceItemCode in ('SPA', 'SPE')
			AND @PriceItemCode in ('SDT', '')
			AND TX_Period = @Period
			AND TX_SystemId = @DatabaseId
			AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)

		UNION ALL

		SELECT 
			TX_ClientID, 
			TX_LCC, 
			TX_Reference1,
			ISNULL(TX_Reference2, ''), 
			ISNULL(TX_Reference3, ''), 
			1 AS UnitCount, 
			TX_ServiceOccuredUTC,
			ISNULL(TX_Branch, '') TX_Branch,
			TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
		FROM
			BillingViewChargeable
		WHERE
			TX_Category = 'SPM'
			AND TX_PriceItemCode = 'SPR' 
			AND (@PriceItemCode = 'SPT' OR @PriceItemCode = '')
			AND TX_Period = @Period
			AND TX_SystemId = @DatabaseId
			AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	) c
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
ORDER BY 
	TX_ServiceOccuredUTC
";

		#endregion
	}
}

