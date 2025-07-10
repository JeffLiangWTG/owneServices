using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.DeniedPartyScreening
{
	public class DpsBillingSystem : TransactionBillingSystem
	{
		public DpsBillingSystem()
		{
		}

		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.DeniedPartyScreening; }
		}

		#endregion

		#region System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionalSystemBill(SystemCode, Context.Factory);
		}

		#endregion

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);
			LoadUsageCommon(reader, rawUsage.Summary, false);
			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			LoadUsageCommon(reader, rawUsage.Summary, true);
			return rawUsage;
		}

		void LoadUsageCommon(IDataReader reader, SummarySection summary, bool isStl)
		{
			summary.Header.Column1 = "Company Code";
			summary.Header.Column2 = "User";
			if (isStl)
			{
				summary.Header.Column9 = "Screen Count";
			}
			else
			{
				summary.Header.Column3 = "Screen Count";
			}

			while (reader.Read())
			{
				string licenceCode = (string)reader["TX_ClientId"];
				string userName = (string)reader["TX_Reference4"];
				int unitCount = (int)reader["UnitCount"];

				SummaryLine line = summary.Lines.AddNew();
				line.Column1 = licenceCode.Substring(3, 3);
				line.Column2 = userName;
				if (isStl)
				{
					line.Column9 = unitCount.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					line.Column3 = unitCount.ToString(CultureInfo.InvariantCulture);
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			var headerColumns = new string[] { "Company Code", "User", "Screen Count" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string licenceCode = (string)reader["TX_ClientId"];
					string userName = (string)reader["TX_Reference4"];
					int unitCount = (int)reader["UnitCount"];
					var companyCode = licenceCode.Substring(3, 3);

					var dataValues = new string[] { companyCode, userName, unitCount.ToString(CultureInfo.InvariantCulture) };
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
					string licenceCode = (string)reader["TX_ClientId"];
					string userName = (string)reader["TX_Reference4"];
					int unitCount = (int)reader["UnitCount"];
					var companyCode = licenceCode.Substring(3, 3);
					ZDateTime usageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					var licenceCodeWithDashes = licenceCode.Substring(0, 3) + '-' + companyCode + '-' + licenceCode.Substring(6, 3);

					writer.WriteCsvUsageReport(usageTime, companyCode, "", "", string.Concat(licenceCodeWithDashes, ' ', userName).Trim(), context.PriceItemCode, context.PriceItemDescription, unitCount);
				}
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage = @"
SELECT
	TX_ClientId,
	TX_Reference4 = TX_Reference4,
	TX_ServiceOccuredUTC = MAX(TX_ServiceOccuredUTC),
	UnitCount = count(*)
FROM
	dbo.BillingViewChargeable
WHERE
	TX_Category = 'DPS'
	AND TX_PriceItemCode = 'DPS'
	AND TX_Reference1 = 'SCR'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
GROUP BY
    TX_ClientId, TX_Reference4
ORDER By
	TX_Reference4
";

		#endregion
	}
}

