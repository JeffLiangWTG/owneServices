using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class USCustomsBillingSystem : TransactionBillingSystem
	{
		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.USCustoms; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new USCustomsBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, CargoWise.EntityFramework.BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new USCustomsUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Message Type";
			rawUsage.Summary.Header.Column2 = "Unique Job Identifier";
			rawUsage.Summary.Header.Column3 = "Port Of Entry";
			rawUsage.Summary.Header.Column4 = "First Transmit Time";

			while (reader.Read())
			{
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				string uniqueJobID = (string)reader["TX_Reference1"];
				string portOfEntry = (string)reader["TX_Reference2"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = GetMessageType(priceItemCode);
				line.Column2 = uniqueJobID;
				line.Column3 = portOfEntry;
				line.Column4 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column1 = "Message Type";
			rawUsage.Summary.Header.Column2 = "Unique Job Identifier";
			rawUsage.Summary.Header.Column3 = "Port Of Entry";
			rawUsage.Summary.Header.Column9 = "First Transmit Time";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				string uniqueJobID = (string)reader["TX_Reference1"];
				string portOfEntry = (string)reader["TX_Reference2"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = GetMessageType(priceItemCode);
				line.Column3 = uniqueJobID;
				line.Column4 = portOfEntry;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Message Type", "Unique Job Identifier", "Port Of Entry", "First Transmit Time" };
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

					string priceItemCode = (string)reader["TX_PriceItemCode"];
					string uniqueJobID = (string)reader["TX_Reference1"];
					string portOfEntry = (string)reader["TX_Reference2"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, GetMessageType(priceItemCode), uniqueJobID, portOfEntry, messageTime.ToLongTimeString() };
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

					string priceItemCode = (string)reader["TX_PriceItemCode"];
					string uniqueJobID = (string)reader["TX_Reference1"];
					string portOfEntry = (string)reader["TX_Reference2"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(GetMessageType(priceItemCode), ' ', uniqueJobID, ' ', portOfEntry).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		static string GetMessageType(string priceItemCode)
		{
			string messageTypeName = string.Empty;
			switch (priceItemCode)
			{
				case "UJL":
					messageTypeName = "Drawback";
					break;
				case "UPL":
					messageTypeName = "Protest";
					break;
				case "URB":
					messageTypeName = "Recon";
					break;
				case "UQT":
					messageTypeName = "Inbond";
					break;
				case "URR":
				case "USO":
					messageTypeName = "Import";
					break;
				default:
					messageTypeName = "Export";
					break;
			}
			return messageTypeName;
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage = @"
SELECT
	TX_ClientID,
	CompanyCode = ISNULL(LCC_Code, ''),
	TX_PriceItemCode,
	TX_Reference1,
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	dbo.BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'USC'
	AND
	(
		(@PriceItemCode = '' AND TX_PriceItemCode IN ('UJL','UPL','URB','UQT','URR','USO','UFT','UXT')) 
		OR 
		(@PriceItemCode != '' AND TX_PriceItemCode = @PriceItemCode)
	)
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER BY 
	TX_ServiceOccuredUTC";

		#endregion
	}
}

