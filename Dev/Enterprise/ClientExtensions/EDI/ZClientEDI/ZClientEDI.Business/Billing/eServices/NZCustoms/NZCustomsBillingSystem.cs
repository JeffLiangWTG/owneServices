using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class NZCustomsBillingSystem : TransactionBillingSystem
	{
		public NZCustomsBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.NZCustoms; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new NZCustomsBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new NZCustomsUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Message Type";
			rawUsage.Summary.Header.Column3 = "Job ID";
			rawUsage.Summary.Header.Column4 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string messageType = (string)reader["MessageType"];
				string jobId = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId;
				line.Column2 = messageType;
				line.Column3 = jobId;
				line.Column4 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Message Type";
			rawUsage.Summary.Header.Column3 = "Job ID";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string messageType = (string)reader["MessageType"];
				string jobId = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = messageType;
				line.Column3 = jobId;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Message Type", "Job ID", "Message Time (UTC)" };
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

					string messageType = (string)reader["MessageType"];
					string jobId = (string)reader["TX_Reference1"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { client, messageType, jobId, messageTime.ToLongTimeString() };
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

					string messageType = (string)reader["MessageType"];
					string jobId = (string)reader["TX_Reference1"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(messageType, ' ', jobId).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		const string sql_Raw_Usage = @"

SELECT 
	TX_ClientId,
	CompanyCode = ISNULL(LCC_Code, ''),
	MessageType = CASE TX_PriceItemCode
		WHEN 'DEC' THEN 'CUSDEC'
		WHEN 'CAR' THEN 'CUSCAR'
		ELSE TX_PriceItemCode END,
	TX_Reference1,
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	dbo.BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'NZC'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
	AND
	(
		(@PriceItemCode = '' AND TX_PriceItemCode IN ('DEC', 'CAR', 'IM1', 'EX1', 'CRE', 'OCR', 'ICR'))
		OR 
		(@PriceItemCode != '' AND TX_PriceItemCode = @PriceItemCode)
	)
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion

	}
}

