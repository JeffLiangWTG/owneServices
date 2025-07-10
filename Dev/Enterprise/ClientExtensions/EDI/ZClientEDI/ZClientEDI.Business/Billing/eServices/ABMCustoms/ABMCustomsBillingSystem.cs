using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing
{
	public class ABMCustomsBillingSystem : TransactionBillingSystem
	{
		public ABMCustomsBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
			IncludeReference1InSystemUsage = true;
			IncludeReference2InSystemUsage = true;
			IncludeReference3InSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ABMCustoms; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new ABMCustomsBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new ABMCustomsUsage(factory, user, periodStart);
		}

		protected override UsingParty GetUsingPartyForSystemUsage(ClientChargeableUsage chargeableUsage)
		{
			var licHeaderQuery = new ZQuery(LicenceHeaderSchema.LA_LC, chargeableUsage.U1_LC);
			licHeaderQuery.AddToFilter(LicenceHeaderSchema.LA_LD, chargeableUsage.U1_LD);

			var licHeader = chargeableUsage.Factory.LoadTop1<LicenceHeader>(licHeaderQuery);
			if (licHeader != null)
			{
				return new UsingParty(licHeader);
			}
			else
			{
				return base.GetUsingPartyForSystemUsage(chargeableUsage);
			}
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			string currentTransactionType = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string jurisdictionCode = (string)reader["TX_Reference1"];
				string procedureCode = (string)reader["TX_Reference2"];
				string reference = (string)reader["TX_Reference3"];
				string department = (string)reader["TX_Reference5"];
				int count = (int)reader["TX_BillableCount"];

				string transactionType = (string)reader["TX_PriceItemCode"];
				if (string.Compare(transactionType, currentTransactionType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetTransactionDescription(transactionType);
					summarySection.Header.Column1 = "Client ID";
					summarySection.Header.Column2 = "Jurisdiction";
					summarySection.Header.Column3 = "Department";
					summarySection.Header.Column4 = "Provider";

					rawUsage.SummarySections.Add(summarySection);
					currentTransactionType = transactionType;
				}

				var line = summarySection.Lines.AddNew();
				line.Column1 = clientId;
				line.Column2 = jurisdictionCode;
				line.Column3 = department;
				line.Column4 = procedureCode;

				if (transactionType == ABMCustomsTransactionTypes.Codes.Customs)
				{
					summarySection.Header.Column5 = "Transactions";
					line.Column5 = count.ToString(CultureInfo.InvariantCulture);
				}
				else if (transactionType == ABMCustomsTransactionTypes.Codes.PortCommunity)
				{
					summarySection.Header.Column5 = "Reference";
					summarySection.Header.Column6 = "Transactions";
					line.Column5 = reference;
					line.Column6 = count.ToString(CultureInfo.InvariantCulture);
				}
				else if (transactionType == ABMCustomsTransactionTypes.Codes.FiscalRep)
				{
					summarySection.Header.Column5 = "Invoice Reference";
					summarySection.Header.Column6 = "Transactions";
					line.Column5 = reference;
					line.Column6 = count.ToString(CultureInfo.InvariantCulture);
				}
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			string currentTransactionType = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string jurisdictionCode = (string)reader["TX_Reference1"];
				string procedureCode = (string)reader["TX_Reference2"];
				string reference = (string)reader["TX_Reference3"];
				string department = (string)reader["TX_Reference5"];
				int count = (int)reader["TX_BillableCount"];

				string transactionType = (string)reader["TX_PriceItemCode"];
				if (string.Compare(transactionType, currentTransactionType, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = " - " + GetTransactionDescription(transactionType);
					summarySection.Header.Column1 = "Company Code";
					summarySection.Header.Column2 = "Jurisdiction";
					summarySection.Header.Column3 = "Department";
					summarySection.Header.Column4 = "Provider";
					rawUsage.SummarySections.Add(summarySection);
					currentTransactionType = transactionType;
				}

				var line = summarySection.Lines.AddNew();
				line.Column1 = context.CompanyCode;
				line.Column2 = jurisdictionCode;
				line.Column3 = department;
				line.Column4 = procedureCode;

				if (transactionType == ABMCustomsTransactionTypes.Codes.Customs)
				{
					summarySection.Header.Column9 = "Transactions";
					line.Column9 = count.ToString(CultureInfo.InvariantCulture);
				}
				else if (transactionType == ABMCustomsTransactionTypes.Codes.PortCommunity)
				{
					summarySection.Header.Column5 = "Reference";
					summarySection.Header.Column9 = "Transactions";
					line.Column5 = reference;
					line.Column9 = count.ToString(CultureInfo.InvariantCulture);
				}
				else if (transactionType == ABMCustomsTransactionTypes.Codes.FiscalRep)
				{
					summarySection.Header.Column5 = "Invoice Reference";
					summarySection.Header.Column9 = "Transactions";
					line.Column5 = reference;
					line.Column9 = count.ToString(CultureInfo.InvariantCulture);
				}
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { "Type", clientHeaderColumn, "Jurisdiction", "Department", "Provider", "Reference", "Transactions" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string client;
					string clientId = (string)reader["TX_ClientID"];
					if (isStlBilling)
					{
						client = clientId.Length >= 6 ? clientId.Substring(3, 3) : "";
					}
					else
					{
						client = clientId;
					}

					string transactionType = (string)reader["TX_PriceItemCode"];
					string jurisdictionCode = (string)reader["TX_Reference1"];
					string department = (string)reader["TX_Reference5"];
					string procedureCode = (string)reader["TX_Reference2"];
					string reference = (string)reader["TX_Reference3"];
					int count = (int)reader["TX_BillableCount"];

					var dataValues = new string[] { GetTransactionDescription(transactionType), client, jurisdictionCode, department, procedureCode, reference, count.ToString(CultureInfo.InvariantCulture) };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			var priceCode = context.PriceItemCode;
			var priceDescription = context.PriceItemDescription;

			if (priceCode.IsEmpty)
			{
				priceCode = BillingConstants.BillingSystem.ABMCustoms;
			}

			if (priceDescription.IsEmpty)
			{
				priceDescription = BillingConstants.GetBillingSystemList().GetDescriptionFromCode(BillingConstants.BillingSystem.ABMCustoms);
			}

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string client;
					string clientId = (string)reader["TX_ClientID"];
					if (isStlBilling)
					{
						client = clientId.Length >= 6 ? clientId.Substring(3, 3) : "";
					}
					else
					{
						client = clientId;
					}

					string transactionType = (string)reader["TX_PriceItemCode"];
					var transactionDescription = GetTransactionDescription(transactionType);
					string jurisdictionCode = (string)reader["TX_Reference1"];
					string department = (string)reader["TX_Reference5"];
					string procedureCode = (string)reader["TX_Reference2"];
					string reference = (string)reader["TX_Reference3"];
					int count = (int)reader["TX_BillableCount"];
					ZDateTime usageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					string branchCode = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					writer.WriteCsvUsageReport(usageTime, client, branchCode, staff, string.Concat(transactionDescription, ' ', jurisdictionCode, ' ', department, ' ', procedureCode, ' ', reference).Trim(), priceCode, priceDescription, count);
				}
			}
		}

		ZString GetTransactionDescription(string code)
		{
			if (ABMCustomsTransactionTypes == null)
			{
				ABMCustomsTransactionTypes = new ABMCustomsTransactionTypes();
			}
			return ABMCustomsTransactionTypes.GetDescriptionFromCode(code);
		}
		ABMCustomsTransactionTypes ABMCustomsTransactionTypes;

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage = @"
SELECT
	TX_ClientID,
	TX_PriceItemCode,
	TX_BillableCount,
	TX_Reference1,
	ISNULL(TX_Reference2, '') AS TX_Reference2,
	ISNULL(TX_Reference3, '') AS TX_Reference3,
	ISNULL(TX_Reference5, '') AS TX_Reference5,
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
WHERE
	TX_Category = 'ABM'
	AND TX_PriceItemCode in ('CTM', 'POC', 'FRP')
	AND TX_Period = @Period
	and TX_SystemId = @DatabaseId 
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
ORDER BY 
	CASE TX_PriceItemCode
	WHEN 'CTM' THEN 1
	WHEN 'POC' THEN 2
	WHEN 'FRP' THEN 3
	END
";

		#endregion

	}
}
