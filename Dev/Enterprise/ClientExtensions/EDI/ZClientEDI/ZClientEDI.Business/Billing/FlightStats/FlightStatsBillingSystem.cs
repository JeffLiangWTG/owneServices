using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class FlightStatsBillingSystem : TransactionBillingSystem
	{
		public FlightStatsBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.FlightStats; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionalSystemBill(SystemCode, Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new FlightStatsUsage(
				Context.Factory,
				SystemCode,
				BillingConstants.PriceHeaderType.FlightStats,
				user,
				periodStart);
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var usages = base.CreateSystemUsages(chargeableUsages);

			foreach (var usage in usages.OfType<PriceItemUsage>())
			{
				usage.CalculatePriceItemByTotalUnitCount();
			}

			return usages;
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "MAWB";
			rawUsage.Summary.Header.Column3 = "Job Num";
			rawUsage.Summary.Header.Column4 = "Origin-Destination";
			rawUsage.Summary.Header.Column5 = "Message Time (UTC)";

			while (reader.Read())
			{
				var clientID = (string)reader["TX_ClientID"];
				var mawbNum = (string)reader["TX_Reference1"];
				var jobNum = (string)reader["TX_Reference2"];
				var originAndDestination = (string)reader["TX_Reference3"];
				var messageTimeUTC = ToMessageTimeFormat((DateTime)reader["TX_ServiceOccuredUTC"]);

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientID;
				line.Column2 = mawbNum;
				line.Column3 = jobNum;
				line.Column4 = originAndDestination;
				line.Column5 = messageTimeUTC;
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "MAWB";
			rawUsage.Summary.Header.Column3 = "Job Num";
			rawUsage.Summary.Header.Column4 = "Origin-Destination";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				var companyCode = (string)reader["CompanyCode"];
				var mawbNum = (string)reader["TX_Reference1"];
				var jobNum = (string)reader["TX_Reference2"];
				var originAndDestination = (string)reader["TX_Reference3"];
				var messageTimeUTC = ToMessageTimeFormat((DateTime)reader["TX_ServiceOccuredUTC"]);

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = mawbNum;
				line.Column3 = jobNum;
				line.Column4 = originAndDestination;
				line.Column9 = messageTimeUTC;
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			var headerColumns = new string[] { clientHeaderColumn, "MAWB", "Job Num", "Origin-Destination", "Message Time (UTC)" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var client = isStlBilling ? (string)reader["CompanyCode"] : (string)reader["TX_ClientID"];
					var mawbNum = (string)reader["TX_Reference1"];
					var jobNum = (string)reader["TX_Reference2"];
					var originAndDestination = (string)reader["TX_Reference3"];
					var messageTimeUTC = ToMessageTimeFormat((DateTime)reader["TX_ServiceOccuredUTC"]);

					var dataValues = new string[] { client, mawbNum, jobNum, originAndDestination, messageTimeUTC };
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			var priceDescription = BillingConstants.GetBillingSystemList().GetDescriptionFromCode(BillingConstants.BillingSystem.FlightStats);

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var client = isStlBilling ? (string)reader["CompanyCode"] : (string)reader["TX_ClientID"];
					var mawbNum = (string)reader["TX_Reference1"];
					var jobNum = (string)reader["TX_Reference2"];
					var originAndDestination = (string)reader["TX_Reference3"];
					var messageTimeUTC = (DateTime)reader["TX_ServiceOccuredUTC"];
					var branchCode = (string)reader["TX_Branch"];
					var billableCount = (int)reader["TX_BillableCount"];
					var staffCode = (string)reader["TX_ClientStaffCode"];
					var priceCode = (string)reader["TX_PriceItemCode"];

					writer.WriteCsvUsageReport(messageTimeUTC, client, branchCode, staffCode, string.Join(" ", new[] { mawbNum, jobNum, originAndDestination }), priceCode, priceDescription, billableCount);
				}
			}
		}

		#endregion Load Raw Usage

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
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'FMS'
	AND TX_PriceItemCode  = 'FMS'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER By TX_ServiceOccuredUTC
";

		#endregion SQL
	}
}


