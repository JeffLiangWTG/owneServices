using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ZACustomsBillingSystem : TransactionBillingSystem
	{
		public ZACustomsBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ZACustoms; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionSystemBillEx(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new ZACustomsUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			var currentMessageType = string.Empty;
			SummarySection summarySection = null;

			while (reader.Read())
			{
				var clientId = (string)reader["TX_ClientID"];
				var messageType = (string)reader["TX_PriceItemCode"];
				var ref1 = (string)reader["TX_Reference1"];
				var ref2 = (string)reader["TX_Reference2"];
				var ref3 = (string)reader["TX_Reference3"];
				var ref4 = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				if (string.Compare(messageType, currentMessageType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetMessageTypeDescription(messageType);
					summarySection.Header.Column1 = "Client ID";

					if (messageType == "ZX1")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Master Bill Number";
						summarySection.Header.Column4 = "Flight / Voyage";
						summarySection.Header.Column5 = "Job Number";
						summarySection.Header.Column6 = "Message Time (UTC)";
					}
					else if (messageType == "ZX2")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Flight / Voyage";
						summarySection.Header.Column4 = "Job Number";
						summarySection.Header.Column5 = "Message Time (UTC)";
					}
					else if (messageType == "ZX3")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Bill Date";
						summarySection.Header.Column4 = "Flight / Voyage";
						summarySection.Header.Column5 = "Job Number";
						summarySection.Header.Column6 = "Message Time (UTC)";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentMessageType = messageType;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = clientId;

					if (messageType == "ZX1")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column5 = ref4;
						line.Column6 = messageTime.ToLongTimeString();
					}
					else if (messageType == "ZX2")
					{
						line.Column2 = ref1;
						line.Column3 = ref3;
						line.Column4 = ref4;
						line.Column5 = messageTime.ToLongTimeString();
					}
					else if (messageType == "ZX3")
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
			var currentMessageType = string.Empty;
			SummarySection summarySection = null;

			while (reader.Read())
			{
				var companyCode = (string)reader["CompanyCode"];
				var messageType = (string)reader["TX_PriceItemCode"];
				var ref1 = (string)reader["TX_Reference1"];
				var ref2 = (string)reader["TX_Reference2"];
				var ref3 = (string)reader["TX_Reference3"];
				var ref4 = (string)reader["TX_Reference4"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				if (string.Compare(messageType, currentMessageType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetMessageTypeDescription(messageType);
					summarySection.Header.Column1 = "Company Code";

					if (messageType == "ZX1")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Master Bill Number";
						summarySection.Header.Column4 = "Flight / Voyage";
						summarySection.Header.Column5 = "Job Number";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}
					else if (messageType == "ZX2")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Flight / Voyage";
						summarySection.Header.Column4 = "Job Number";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}
					else if (messageType == "ZX3")
					{
						summarySection.Header.Column2 = "Carrier Code";
						summarySection.Header.Column3 = "Bill Date";
						summarySection.Header.Column4 = "Flight / Voyage";
						summarySection.Header.Column5 = "Job Number";
						summarySection.Header.Column9 = "Message Time (UTC)";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentMessageType = messageType;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = companyCode;

					if (messageType == "ZX1")
					{
						line.Column2 = ref1;
						line.Column3 = ref2;
						line.Column4 = ref3;
						line.Column5 = ref4;
						line.Column9 = messageTime.ToLongTimeString();
					}
					else if (messageType == "ZX2")
					{
						line.Column2 = ref1;
						line.Column3 = ref3;
						line.Column4 = ref4;
						line.Column9 = messageTime.ToLongTimeString();
					}
					else if (messageType == "ZX3")
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
				case "ZX1":
					return new string[] { "Message Type", client, "Carrier Code", "Master Bill Number", "Flight / Voyage", "Job Number", "Message Time (UTC)" };
				case "ZX2":
					return new string[] { "Message Type", client, "Carrier Code", "Flight / Voyage", "Job Number", "Message Time (UTC)" };
				case "ZX3":
					return new string[] { "Message Type", client, "Carrier Code", "Bill Date", "Flight / Voyage", "Job Number", "Message Time (UTC)" };
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
				case "ZX1":
					return new string[] { messageType, client, ref1, ref2, ref3, ref4, messageTimeAsText };
				case "ZX2":
					return new string[] { messageType, client, ref1, ref3, ref4, messageTimeAsText };
				case "ZX3":
					return new string[] { messageType, client, ref1, ref2, ref3, ref4, messageTimeAsText };
				default:
					return Array.Empty<string>();
			}
		}

		string GetMessageTypeDescription(string messageType)
		{
			var result = string.Empty;
			if (messageType == "ZX1")
			{
				result = "Outbound CUSCAR Messages [COH and HAB]";
			}
			else if (messageType == "ZX2")
			{
				result = "Outbound CUSCAR Messages [FFM, ECL, RFM, BBB and RMA]";
			}
			else if (messageType == "ZX3")
			{
				result = "Outbound CUSCAR Messages [COM and FWB]";
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
	TX_Category = 'ZAC'
	AND TX_PriceItemCode IN ('ZX1', 'ZX2', 'ZX3')
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER By TX_PriceItemCode, TX_ServiceOccuredUTC
";
		#endregion
	}
}

