using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class GBCustomsBillingSystem : TransactionBillingSystem
	{
		public GBCustomsBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.GBCustoms; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionSystemBillEx(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new GBCustomsUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			string currentMessageType = string.Empty;
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string messageType = (string)reader["TX_PriceItemCode"];
				string ref1 = (string)reader["TX_Reference1"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string ref4 = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				if (string.Compare(messageType, currentMessageType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetMessageTypeDescription(messageType);
					summarySection.Header.Column1 = "Client ID";

					if (messageType == "AWB")
					{
						summarySection.Header.Column2 = "MAWB";
						summarySection.Header.Column3 = "HAWB";
						summarySection.Header.Column4 = "ApplicationCode";
						summarySection.Header.Column5 = "Message Time (UTC)";
					}
					else if (messageType == "GTM")
					{
						summarySection.Header.Column2 = "Message Purpose";
						summarySection.Header.Column3 = "Direction";
						summarySection.Header.Column4 = "Sender";
						summarySection.Header.Column5 = "Recipient";
						summarySection.Header.Column6 = "Message Time (UTC)";
					}
					else if (messageType == "CUE")
					{
						summarySection.Header.Column2 = "Job Number";
						summarySection.Header.Column3 = "Job Type";
						summarySection.Header.Column4 = "BGM Reference";
						summarySection.Header.Column5 = "Entry Number";
						summarySection.Header.Column6 = "Message Time (UTC)";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentMessageType = messageType;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = clientId;

					if (messageType == "AWB")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column5 = messageTime.ToLongTimeString();
					}
					else if (messageType == "GTM")
					{
						line.Column2 = ref4;
						line.Column3 = ref1;
						line.Column4 = ref2;
						line.Column5 = ref3;
						line.Column6 = messageTime.ToLongTimeString();
					}
					else if (messageType == "CUE")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column5 = ref4;
						line.Column6 = messageTime.ToLongTimeString();
					}
				}
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionStlRawUsage(context, SystemCode);
			string currentMessageType = string.Empty;
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string messageType = (string)reader["TX_PriceItemCode"];
				string ref1 = (string)reader["TX_Reference1"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string ref4 = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				if (string.Compare(messageType, currentMessageType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetMessageTypeDescription(messageType);
					summarySection.Header.Column1 = "Company Code";

					if (messageType == "AWB")
					{
						summarySection.Header.Column2 = "MAWB";
						summarySection.Header.Column3 = "HAWB";
						summarySection.Header.Column4 = "Application Code";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}
					else if (messageType == "GTM")
					{
						summarySection.Header.Column2 = "Message Purpose";
						summarySection.Header.Column3 = "Direction";
						summarySection.Header.Column4 = "Sender";
						summarySection.Header.Column5 = "Recipient";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}
					else if (messageType == "CUE")
					{
						summarySection.Header.Column2 = "Job Number";
						summarySection.Header.Column3 = "Job Type";
						summarySection.Header.Column4 = "BGM Reference";
						summarySection.Header.Column5 = "Entry Number";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentMessageType = messageType;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = companyCode;

					if (messageType == "AWB")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column9 = messageTime.ToLongTimeString();
					}
					else if (messageType == "GTM")
					{
						line.Column2 = ref4;
						line.Column3 = ref1;
						line.Column4 = ref2;
						line.Column5 = ref3;
						line.Column9 = messageTime.ToLongTimeString();
					}
					else if (messageType == "CUE")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column5 = ref4;
						line.Column9 = messageTime.ToLongTimeString();
					}
				}
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string currentItemCode = "";

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					if (string.Compare(priceItemCode, currentItemCode, StringComparison.OrdinalIgnoreCase) != 0)
					{
						var headerColumns = GetCsvHeaderColumns(priceItemCode, isStlBilling);
						var headerCsvLine = new OCsvLine(headerColumns);
						action(headerCsvLine.ToString());
						currentItemCode = priceItemCode;
					}

					var dataValues = GetCsvDataValues(priceItemCode, isStlBilling, clientId, companyCode, ref1, ref2, ref3, ref4, messageTime);
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
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					string client = isStlBilling ? companyCode : clientId;
					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(ref1, ' ', ref2, ' ', ref3, ' ', ref4).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		string[] GetCsvHeaderColumns(string priceItemCode, bool isStlBilling)
		{
			var client = isStlBilling ? "Company Code" : "Client ID";
			switch (priceItemCode)
			{
				case "AWB":
					return new string[] { "Message Type", client, "MAWB", "HAWB", "Application Code", "Message Time (UTC)" };
				case "GTM":
					return new string[] { "Message Type", client, "Message Purpose", "Direction", "Sender", "Recipient", "Message Time (UTC)" };
				case "CUE":
					return new string[] { "Message Type", client, "Job Number", "Job Type", "BGM Reference", "Entry Numbe", "Message Time (UTC)" };
				default:
					return Array.Empty<string>();
			}
		}

		string[] GetCsvDataValues(string priceItemCode, bool isStlBilling, string clientId, string companyCode, string ref1, string ref2, string ref3, string ref4, ZDateTime messageTime)
		{
			var messageType = GetMessageTypeDescription(priceItemCode);

			string client = isStlBilling ? companyCode : clientId;

			var messageTimeAsText = messageTime.ToLongTimeString();

			switch (priceItemCode)
			{
				case "AWB":
					return new string[] { messageType, client, ref1, ref2, ref3, messageTimeAsText };
				case "GTM":
					return new string[] { messageType, client, ref4, ref1, ref2, ref3, messageTimeAsText };
				case "CUE":
					return new string[] { messageType, client, ref1, ref2, ref3, ref4, messageTimeAsText };
				default:
					return Array.Empty<string>();
			}
		}

		string GetMessageTypeDescription(string messageType)
		{
			var result = string.Empty;
			if (messageType == "AWB")
			{
				result = "Air Waybills";
			}
			else if (messageType == "GTM")
			{
				result = "General Text Messages";
			}
			else if (messageType == "CUE")
			{
				result = "Customs Entries";
			}
			return result;
		}

		#endregion

		#region SQL

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
	ISNULL(TX_Reference2, '') AS TX_Reference2,
	ISNULL(TX_Reference3, '') AS TX_Reference3,
	ISNULL(TX_Reference4, '') AS TX_Reference4,
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'GBC'
	AND TX_PriceItemCode IN ('AWB', 'GTM', 'CUE')
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER By
	CASE TX_PriceItemCode
		WHEN 'AWB' THEN 1
		WHEN 'GTM' THEN 2
		WHEN 'CUE' THEN 3
	END, 
	TX_ServiceOccuredUTC
";
		#endregion
	}
}

