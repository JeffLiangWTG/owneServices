using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class E2EBillingSystem : TransactionBillingSystem
	{
		public E2EBillingSystem()
		{
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.E2E; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionSystemBillEx(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new PriceItemUsage(factory, user, periodStart, BillingConstants.BillingSystem.E2E, true);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);
			LoadUsage(reader, rawUsage.Summary, false);
			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			LoadUsage(reader, rawUsage.Summary, true);
			return rawUsage;
		}

		void LoadUsage(IDataReader reader, SummarySection summary, bool isStl)
		{
			summary.Header.Column2 = "Sender";
			summary.Header.Column3 = "Job";
			summary.Header.Column4 = "Event";
			summary.Header.Column5 = "Message Time (UTC)";
			if (isStl)
			{
				summary.Header.Column1 = "Company Code";
				summary.Header.Column9 = "eHub ID";
			}
			else
			{
				summary.Header.Column1 = "Client ID";
				summary.Header.Column6 = "eHub ID";
			}

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string companyCode = (string)reader["CompanyCode"];
				string senderId = (string)reader["TX_Reference1"];
				string topJobId = (string)reader["TX_Reference2"];
				string level1JobId = (string)reader["TX_Reference3"];
				string level2JobIdOrEvent = (string)reader["TX_Reference4"];
				string level3JobId = (string)reader["TX_Reference5"];
				string trackingId = (string)reader["TX_MessageTrackingID"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
				bool isWIP = "E2W" == (string)reader["SubCode"];

				string jobId = "";
				string eventCode = "";
				if (level2JobIdOrEvent.StartsWith("Event Code:", StringComparison.CurrentCultureIgnoreCase))
				{
					eventCode = level2JobIdOrEvent;
					jobId = level1JobId;
					if (jobId.Length == 0)
					{
						jobId = topJobId;
					}
				}
				else
				{
					jobId = level3JobId;
					if (jobId.Length == 0)
					{
						jobId = level2JobIdOrEvent;
						if (jobId.Length == 0)
						{
							jobId = level1JobId;
							if (jobId.Length == 0)
							{
								jobId = topJobId;
							}
						}
					}
				}

				var line = summary.Lines.AddNew();
				line.Column2 = senderId;
				line.Column3 = jobId;
				line.Column4 = eventCode;
				line.Column5 = messageTime.ToLongTimeString();
				if (isStl)
				{
					line.Column1 = companyCode.ToUpper(CultureInfo.InvariantCulture);
					line.Column9 = trackingId;
				}
				else
				{
					if (isWIP)
					{
						line.Column2 += " (WIP)";
					}
					line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
					line.Column6 = trackingId;
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "Sender", "WIP", "Top Job", "Job 1", "Job 2 / Event", "Job 3", "Tracking ID", "Message Time (UTC)" };
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

					string senderId = (string)reader["TX_Reference1"];
					string topJobId = (string)reader["TX_Reference2"];
					string level1JobId = (string)reader["TX_Reference3"];
					string level2JobIdOrEvent = (string)reader["TX_Reference4"];
					string level3JobId = (string)reader["TX_Reference5"];
					string trackingId = (string)reader["TX_MessageTrackingID"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					bool isWIP = "E2W" == (string)reader["SubCode"];

					var dataValues = new string[] { client, senderId, isWIP ? "Y" : "N", topJobId, level1JobId, level2JobIdOrEvent, level3JobId, trackingId, messageTime.ToLongTimeString() };
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

					string senderId = (string)reader["TX_Reference1"];
					string topJobId = (string)reader["TX_Reference2"];
					string level1JobId = (string)reader["TX_Reference3"];
					string level2JobIdOrEvent = (string)reader["TX_Reference4"];
					string level3JobId = (string)reader["TX_Reference5"];
					string trackingId = (string)reader["TX_MessageTrackingID"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];
					bool isWIP = "E2W" == (string)reader["SubCode"];
					string senderAndWIP = senderId + (isWIP ? " (WIP)" : string.Empty);

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(senderAndWIP, ' ', topJobId, ' ', level1JobId, ' ', level2JobIdOrEvent, ' ', level3JobId, ' ', trackingId).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
				}
			}
		}

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"
declare @BillingData table (
	TX_ClientID          varchar (9)      NOT NULL,
    TX_Reference1        VARCHAR (50)     NOT NULL,
    TX_Reference2        VARCHAR (50)     NULL,
    TX_Reference3        VARCHAR (50)     NULL,
    TX_Reference4        VARCHAR (50)     NULL,
    TX_Reference5        VARCHAR (50)     NULL,
    TX_MessageTrackingID VARCHAR (36)     NULL,
    TX_ServiceOccuredUTC DATETIME2 (7)    NOT NULL,
    TX_BillableCount     INT              NOT NULL,
    TX_Branch            VARCHAR (3)      NULL,
    TX_LCC               UNIQUEIDENTIFIER NULL,
    TX_ClientStaffCode   VARCHAR (3)      NULL
)

insert @BillingData
      (TX_ClientID, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_MessageTrackingID, TX_ServiceOccuredUTC, TX_BillableCount, TX_Branch, TX_LCC, TX_ClientStaffCode)
SELECT TX_ClientID, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_MessageTrackingID, TX_ServiceOccuredUTC, TX_BillableCount, TX_Branch, TX_LCC, TX_ClientStaffCode
FROM
	BillingViewChargeable
WHERE
	TX_Category = 'E2E'
	AND TX_PriceItemCode = 'E2E'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period;

DECLARE @WIPSenders TABLE
(
	[WIP_Client] [VARCHAR](50) NOT NULL,
	[WIP_CreateTimeUtc] [SMALLDATETIME]
);

INSERT @WIPSenders(WIP_Client, WIP_CreateTimeUtc)
SELECT WIP_Client = TX_Reference1, WIP_CreateTimeUtc = MIN(PR_SystemCreateTimeUtc)
FROM 
(
	SELECT DISTINCT TX_Reference1
	FROM @BillingData
	WHERE LEN(TX_Reference1) = 9
)AS Senders
JOIN dbo.LicenceEnterprise ON LE_EnterpriseCode = LEFT(TX_Reference1, 3) COLLATE DATABASE_DEFAULT
JOIN dbo.LicenceDatabase ON LD_LE = LE_PK and LD_ServerCode = RIGHT(TX_Reference1, 3) COLLATE DATABASE_DEFAULT
JOIN dbo.EdiViewLicenceDatabaseOwner dbOwner on LicenceDatabase.LD_PK = dbOwner.LD_PK
LEFT JOIN dbo.ClientCompany LCC ON LCC.LCC_LD = LicenceDatabase.LD_PK AND LCC.LCC_Code = SUBSTRING(TX_Reference1, 4, 3) COLLATE DATABASE_DEFAULT
LEFT JOIN dbo.EdiViewClientCompanyLicence VCCL ON VCCL.LCC_PK = LCC.LCC_PK
JOIN dbo.OrgRelatedParty ON PR_OH_Parent = ISNULL(VCCL.LC_OH, dbOwner.LC_OH) and PR_PartyType = 'WRP'
JOIN dbo.OrgHeader RelatedOrg ON PR_OH_RelatedParty = RelatedOrg.OH_PK AND RelatedOrg.OH_IsUserFlag24 = 1
GROUP BY TX_Reference1;

select *
from
(
	SELECT 
		TX_ClientID,
		LCC_PK,
		CompanyCode = ISNULL(LCC_Code, ''),
		TX_Reference1 = ISNULL(TX_Reference1, ''),
		TX_Reference2 = ISNULL(TX_Reference2, ''),
		TX_Reference3 = ISNULL(TX_Reference3, ''),
		TX_Reference4 = ISNULL(TX_Reference4, ''),
		TX_Reference5 = ISNULL(TX_Reference5, ''),
		TX_MessageTrackingID = ISNULL(TX_MessageTrackingID, ''),
		TX_ServiceOccuredUTC,
		TX_BillableCount,
		ISNULL(TX_Branch, '') TX_Branch,
		SubCode = CASE WHEN @Period >= 201705 AND WIP_CreateTimeUtc IS NOT NULL AND WIP_CreateTimeUtc <= TX_ServiceOccuredUTC THEN 'E2W'
			ELSE 'E2E' END,
		TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
	FROM
		@BillingData
		LEFT JOIN @WIPSenders WIPSenders ON WIP_Client = TX_Reference1
		LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
) x
where ISNULL(@PriceItemCode, '') = '' or SubCode = @PriceItemCode
ORDER BY
	TX_ServiceOccuredUTC
";

		#endregion
	}
}

