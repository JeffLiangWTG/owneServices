using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing
{
	public class AirlineMessagingBillingSystem : TransactionBillingSystem
	{
		public AirlineMessagingBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.AirlineMessaging; }
		}

		protected override bool ShouldCreateSystemBillPerCurrency
		{
			get { return true; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new AirlineMessagingBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new AirlineMessagingUsage(factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			string currentProvider = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string messageType = (string)reader["TX_PriceItemCode"];
				string airlineCode = (string)reader["TX_Reference3"];
				string awbNumber = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				string provider = (string)reader["TX_Reference4"];
				if (string.Compare(provider, currentProvider, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + provider;
					summarySection.Header.Column1 = "Client ID";
					summarySection.Header.Column2 = "Message Type";
					summarySection.Header.Column3 = "Airline";
					summarySection.Header.Column4 = "AWB";
					summarySection.Header.Column5 = "Message Time (UTC)";
					rawUsage.SummarySections.Add(summarySection);
					currentProvider = provider;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = clientId;
					line.Column2 = messageType;
					line.Column3 = airlineCode;
					line.Column4 = awbNumber;
					line.Column5 = messageTime.ToLongTimeString();
				}
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionStlRawUsage(context, SystemCode);
			string currentProvider = "";
			SummarySection summarySection = null;
			var mappedCategoryAndUsageCodes = context.CategoryAndUsageCodes;

			while (reader.Read())
			{
				var usageCode = (string)reader["UsageCode"];
				if (!mappedCategoryAndUsageCodes.Any(x => x.Category == BillingConstants.BillingSystem.AirlineMessaging && x.Code == usageCode))
				{
					continue;
				}

				string companyCode = (string)reader["CompanyCode"];
				string messageType = (string)reader["TX_PriceItemCode"];
				string airlineCode = (string)reader["TX_Reference3"];
				string awbNumber = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				string provider = (string)reader["TX_Reference4"];
				if (string.Compare(provider, currentProvider, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + provider;
					summarySection.Header.Column1 = "Company Code";
					summarySection.Header.Column2 = "Message Type";
					summarySection.Header.Column3 = "Airline";
					summarySection.Header.Column4 = "AWB";
					summarySection.Header.Column9 = "Message Time (UTC)";
					rawUsage.SummarySections.Add(summarySection);
					currentProvider = provider;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = companyCode;
					line.Column2 = messageType;
					line.Column3 = airlineCode;
					line.Column4 = awbNumber;
					line.Column9 = messageTime.ToLongTimeString();
				}
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";
			var mappedCategoryAndUsageCodes = isStlBilling ? context.CategoryAndUsageCodes : null;

			var headerColumns = new string[] { "Provider", clientHeaderColumn, "Message Type", "Airline", "AWB", "Message Time (UTC)" };
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
						var usageCode = (string)reader["UsageCode"];
						if (!mappedCategoryAndUsageCodes.Any(x => x.Category == BillingConstants.BillingSystem.AirlineMessaging && x.Code == usageCode))
						{
							continue;
						}

						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}

					string provider = (string)reader["TX_Reference4"];
					string messageType = (string)reader["TX_PriceItemCode"];
					string airlineCode = (string)reader["TX_Reference3"];
					string awbNumber = (string)reader["TX_Reference1"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					var dataValues = new string[] { provider, client, messageType, airlineCode, awbNumber, messageTime.ToLongTimeString() };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			var mappedCategoryAndUsageCodes = isStlBilling ? context.CategoryAndUsageCodes : null;

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string client;
					if (isStlBilling)
					{
						var usageCode = (string)reader["UsageCode"];
						if (!mappedCategoryAndUsageCodes.Any(x => x.Category == BillingConstants.BillingSystem.AirlineMessaging && x.Code == usageCode))
						{
							continue;
						}

						string companyCode = (string)reader["CompanyCode"];
						client = companyCode;
					}
					else
					{
						string clientId = (string)reader["TX_ClientID"];
						client = clientId;
					}

					string provider = (string)reader["TX_Reference4"];
					string messageType = (string)reader["TX_PriceItemCode"];
					string airlineCode = (string)reader["TX_Reference3"];
					string awbNumber = (string)reader["TX_Reference1"];
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					int billableCount = (int)reader["TX_BillableCount"];
					if (messageType.StartsWith("FNA", StringComparison.OrdinalIgnoreCase))
					{
						billableCount = -billableCount;
					}
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(provider, ' ', airlineCode, ' ', awbNumber).Trim(), messageType, context.PriceItemDescription, billableCount);
				}
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		readonly string sql_Raw_Usage = @"
declare @isTraxonReport bit = case when @OrgPk = 
			(
				SELECT LC_OH
				FROM dbo.LicenceCompany JOIN dbo.LicenceEnterprise ON LE_PK = LC_LE
				WHERE
					LE_EnterpriseCode = '" + EDIDataRegistry.Instance.AirlineMessagingTraxonLicenceIdentifier.Value.Substring(0, 3) + @"' 
					AND LC_CompanyCode = '" + EDIDataRegistry.Instance.AirlineMessagingTraxonLicenceIdentifier.Value.Substring(3, 3) + @"'
			)
			then 1 else 0 end;
if @Period >= 201808
begin
	if @isTraxonReport = 0
	begin" + selectSql + @"
			AND 
			(
				TX_SystemId = @DatabaseId 
				AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
			)" + orderSql + @"
	end
	else
	begin" + selectSql + @"
			and TX_Reference4 IN ('TRAXON', 'TRAXONEDP', 'TRAXONRCF')" + orderSql + @"
	end
end
else
begin
	if @isTraxonReport = 0
	begin" + selectSqlPre201808 + @"
			AND 
			(
				TX_SystemId = @DatabaseId 
				AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
			)" + orderSql + @"
	end
	else
	begin" + selectSqlPre201808 + @"
			and TX_Reference4 IN ('TRAXON', 'TRAXONEDP', 'TRAXONRCF')" + orderSql + @"
	end
end
";

		const string selectSqlWithoutPriceCode = @"
	SELECT 
		TX_ClientID,
		CompanyCode = ISNULL(LCC_Code, ''),
		CASE TX_PriceItemCode
			WHEN '-WB' THEN 'FNA (FWB)'
			WHEN '-HL' THEN 'FNA (FHL)'
			ELSE TX_PriceItemCode
		END AS TX_PriceItemCode,
		TX_Reference1,
		ISNULL(TX_Reference3, '') AS TX_Reference3,
		TX_ServiceOccuredUTC,
		CASE 
			WHEN ISNULL(TX_Reference4, '') LIKE 'TRAXON%' THEN 'Traxon'
			ELSE ISNULL(TX_Reference4, '')
		END AS TX_Reference4,
		ISNULL(TX_Branch, '') TX_Branch,
		TX_BillableCount,
		UsageCode = ( " + GetUsageCodeQuery + @" ),
		TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
	FROM
		dbo.BillingViewChargeable
		LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
	WHERE
		TX_Category = 'AMG'
		AND TX_Period = @Period";

		const string selectSql = selectSqlWithoutPriceCode + @"
		AND TX_PriceItemCode IN ('FWB', 'FHL', 'FSU')";

		const string selectSqlPre201808 = selectSqlWithoutPriceCode + @"
		AND TX_PriceItemCode IN ('FWB', 'FHL', '-WB', '-HL', 'FSU')";

		const string orderSql = @"
	ORDER By
		TX_Reference4, TX_ClientID, TX_ServiceOccuredUTC
";

		public const string GetUsageCodeQuery = @"
	ISNULL((
	CASE
		WHEN TX_PriceItemCode IN ('FWB', '-WB') THEN 'W'
		WHEN TX_PriceItemCode IN ('FHL', '-HL') THEN 'H'
		WHEN TX_PriceItemCode = 'FSU' THEN 'S'
	END
	+
	CASE
		WHEN TX_Reference4 IN ('BT', 'BT_WithUnsupported') THEN '1'
		WHEN TX_Reference4 = 'Delta' THEN '2'
		WHEN TX_Reference4 = 'CCSJ' THEN '3'
		WHEN TX_Reference4 = 'CCN' THEN '4'
		WHEN TX_Reference4 IN ('Descartes', 'Descartes_WithUnsupported') THEN '5'
		WHEN TX_Reference4 IN ('GLSHK', 'GLSHK_WithUnsupported') THEN '6'
		WHEN TX_Reference4 IN ('Traxon', 'Traxon_WithUnsupported', 'TraxonEDP', 'TraxonRCF') 
			AND
			(
						(@Period < 201511 AND (TX_Reference3 NOT IN ('CX', '160', 'LY', '114', 'AI', '098', '5X', '406', 'US', '037') OR TX_Reference3 IS NULL)) 
				OR
						(@Period >= 201511 AND (TX_Reference3 NOT IN ('CX', '160', 'AI', '098', '5X', '406', 'US', '037') OR TX_Reference3 IS NULL))
			)
			THEN 'A'
		WHEN TX_Reference4 IN ('Traxon', 'Traxon_WithUnsupported', 'TraxonEDP', 'TraxonRCF') 
			AND 
			(
						(@Period < 201511 AND TX_Reference3 IN ('CX', '160', 'LY', '114', 'AI', '098', '5X', '406', 'US', '037'))
				OR
						(@Period >= 201511 AND TX_Reference3 IN ('CX', '160', 'AI', '098', '5X', '406', 'US', '037'))
			)
			THEN 'X'
		WHEN TX_Reference4 = 'Nallian'    THEN 'L'
		WHEN TX_Reference4 = 'ARINC'      THEN 'I'
		WHEN TX_Reference4 = 'Qatar'      THEN 'Q'
		WHEN TX_Reference4 = 'Cargonaut'  THEN 'G'		
		WHEN TX_Reference4 = 'CargoStart' THEN 'T'
		WHEN TX_Reference4 = 'Tradevan'   THEN 'V'
		WHEN TX_Reference4 = 'PakFresh'   THEN 'F'
		ELSE 'U'
	END
	+
	'C'), '') ";

		#endregion

		public static void CheckUnmappedUsages(ILogger logger, ZDateTime period)
		{
			try
			{
				if (Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.ClientChargeableUsage WHERE U1_PeriodStart = @Period AND U1_Code = 'AMG' AND U1_SubCode like '_U_'",
								(cmd) => cmd.AddParameter("@Period", SqlDbType.SmallDateTime, period.ToDateTime())) > 0)
				{
					var subject = "AMG Usage Mapping Error Detected – Action Required";
					var body = "Some AMG usages are not mapping correctly. Please check and resolve. Ref:WI00894811,AirlineMessagingBillingSystem.CheckUnmappedUsages()";
					logger?.Error(subject);
					logger?.Error(body);
					var mail = new EmailDef()
					{
						Subject = subject,
						Body = body
					};
					var registryItem = EDIDataRegistry.Instance.InternalNotificationGroup;
					Env.OutgoingMailManager.CreateAndSave(mail, registryItem.Value, GroupSourceLocator.GetFromRegistryItem(registryItem));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				logger?.Error(e.Message, e);
			}
		}
	}
}

