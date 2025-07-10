using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing
{
	public class IsfBillingSystem : TransactionBillingSystem
	{
		public IsfBillingSystem()
			: base()
		{
		}

		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ImporterSecurityFiling; }
		}

		#endregion

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new TransactionSystemBillEx(SystemCode, Context.Factory);
		}

		#endregion

		#region Load Raw Usage

		readonly ZDateTime BillingTransactionRawUsageStartDate = new ZDateTime(2016, 4, 1);

		#region ODPL

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			if (context.Period < BillingTransactionRawUsageStartDate)
			{
				return LoadOdplRawUsageFromChargeableUsage(context);
			}
			else
			{
				return base.LoadOdplRawUsage(context);
			}
		}

		SystemRawUsage LoadOdplRawUsageFromChargeableUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "Importer Security Filing Usage Summary";

			rawUsage.Summary.Header.Column1 = "Port Code";
			rawUsage.Summary.Header.Column2 = "Usage Count";

			using (var cmd = Db.Connection.Command(OdplRawUsageSql))
			{
				cmd.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				cmd.AddParameterBasedOnDbColumn("@ClientCompanyPk", context.ClientCompanyPK.ToGuid(), LicenceCompanySchema.PK);

				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var line = rawUsage.Summary.Lines.AddNew();
						line.Column1 = reader.GetString(0);
						line.Column2 = reader.GetInt32(1).ToString(CultureInfo.InvariantCulture);
					}
				}
			}

			return rawUsage;
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new SystemCodeRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Client ID";
			rawUsage.Summary.Header.Column2 = "Postcode";
			rawUsage.Summary.Header.Column3 = "Transaction Number";
			rawUsage.Summary.Header.Column4 = "Message Time (UTC)";

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				string postcode = (string)reader["TX_Reference2"];
				string transactionNumber = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = clientId.ToUpper(CultureInfo.InvariantCulture);
				line.Column2 = postcode;
				line.Column3 = transactionNumber;
				line.Column4 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		#endregion

		#region STL

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			if (context.Period < BillingTransactionRawUsageStartDate)
			{
				return LoadStlRawUsageFromChargeableUsage(context);
			}
			else
			{
				return base.LoadStlRawUsage(context);
			}
		}

		StlRawUsage LoadStlRawUsageFromChargeableUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "Importer Security Filing Usage Summary";

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Port Code";
			rawUsage.Summary.Header.Column9 = "Usage Count";

			using (var cmd = GetConnectionForReader().Command(StlRawUsageSql))
			{
				cmd.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				cmd.AddParameterBasedOnDbColumn("@DatabasePk", context.DatabasePK.ToGuid(), LicenceDatabaseSchema.PK);

				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var line = rawUsage.Summary.Lines.AddNew();
						line.Column1 = reader.GetString(0);
						line.Column2 = reader.GetString(1);
						line.Column9 = reader.GetInt32(2).ToString(CultureInfo.InvariantCulture);
					}
				}
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Postcode";
			rawUsage.Summary.Header.Column3 = "Transaction Number";
			rawUsage.Summary.Header.Column9 = "Message Time (UTC)";

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				string postcode = (string)reader["TX_Reference2"];
				string transactionNumber = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = companyCode;
				line.Column2 = postcode;
				line.Column3 = transactionNumber;
				line.Column9 = messageTime.ToLongTimeString();
			}

			return rawUsage;
		}

		#endregion

		#region CSV format

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			if (context.Period < BillingTransactionRawUsageStartDate)
			{
				if (isStlBilling)
				{
					LoadStlRawUsageInCsvFromChargeableUsage(context, action);
				}
				else
				{
					LoadOdplRawUsageInCsvFromChargeableUsage(context, action);
				}
			}
			else
			{
				var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

				var headerColumns = new string[] { clientHeaderColumn, "Postcode", "Transaction Number", "Message Time (UTC)" };
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

						string postcode = (string)reader["TX_Reference2"];
						string transactionNumber = (string)reader["TX_Reference1"];
						ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

						var dataValues = new string[] { client, postcode, transactionNumber, messageTime.ToLongTimeString() };
						var dataCsvLine = new OCsvLine(dataValues);
						action(dataCsvLine.ToString());
					}
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			if (context.Period < BillingTransactionRawUsageStartDate)
			{
				if (isStlBilling)
				{
					LoadStlRawUsageInCsvFromChargeableUsage(context, writer);
				}
				else
				{
					LoadOdplRawUsageInCsvFromChargeableUsage(context, writer);
				}
			}
			else
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

						string postcode = (string)reader["TX_Reference2"];
						string transactionNumber = (string)reader["TX_Reference1"];
						ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
						int billableCount = (int)reader["TX_BillableCount"];
						string branchCode = (string)reader["TX_Branch"];
						string staff = (string)reader["TX_ClientStaffCode"];

						writer.WriteCsvUsageReport(messageTime, client, branchCode, staff, string.Concat(postcode, ' ', transactionNumber).Trim(), context.PriceItemCode, context.PriceItemDescription, billableCount);
					}
				}
			}
		}

		void LoadOdplRawUsageInCsvFromChargeableUsage(BillingLoadRawUsageContext context, Action<string> action)
		{
			var headerColumns = new string[] { "Port Code", "Usage Count" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetConnectionForReader().Command(OdplRawUsageSql))
			{
				command.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				command.AddParameterBasedOnDbColumn("@ClientCompanyPk", context.ClientCompanyPK.ToGuid(), ClientCompanySchema.PK);

				using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var dataValues = new string[] { reader.GetString(0), reader.GetInt32(1).ToString(CultureInfo.InvariantCulture) };
						var dataCsvLine = new OCsvLine(dataValues);
						action(dataCsvLine.ToString());
					}
				}
			}
		}

		void LoadOdplRawUsageInCsvFromChargeableUsage(BillingLoadRawUsageContext context, ICsvUsageReportWriter writer)
		{
			using (var command = GetConnectionForReader().Command(OdplRawUsageSql))
			{
				command.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				command.AddParameterBasedOnDbColumn("@ClientCompanyPk", context.ClientCompanyPK.ToGuid(), ClientCompanySchema.PK);

				using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var portCode = reader.GetString(0);
						var usageCount = reader.GetInt32(1);
						var messageTime = reader.GetDateTime(2);

						writer.WriteCsvUsageReport(messageTime, "", "", "", portCode, context.PriceItemCode, context.PriceItemDescription, usageCount);
					}
				}
			}
		}

		void LoadStlRawUsageInCsvFromChargeableUsage(BillingLoadRawUsageContext context, Action<string> action)
		{
			var headerColumns = new string[] { "Company Code", "Port Code", "Usage Count" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetConnectionForReader().Command(StlRawUsageSql))
			{
				command.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				command.AddParameterBasedOnDbColumn("@DatabasePk", context.DatabasePK.ToGuid(), LicenceDatabaseSchema.PK);

				using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var dataValues = new string[] { reader.GetString(0), reader.GetString(1), reader.GetInt32(2).ToString(CultureInfo.InvariantCulture) };
						var dataCsvLine = new OCsvLine(dataValues);
						action(dataCsvLine.ToString());
					}
				}
			}
		}

		void LoadStlRawUsageInCsvFromChargeableUsage(BillingLoadRawUsageContext context, ICsvUsageReportWriter writer)
		{
			using (var command = GetConnectionForReader().Command(StlRawUsageSql))
			{
				command.AddParameterBasedOnDbColumn("@PeriodStart", context.Period.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				command.AddParameterBasedOnDbColumn("@DatabasePk", context.DatabasePK.ToGuid(), LicenceDatabaseSchema.PK);

				using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var companyCode = reader.GetString(0);
						var portCode = reader.GetString(1);
						var usageCount = reader.GetInt32(2);
						var messageTime = reader.GetDateTime(3);

						writer.WriteCsvUsageReport(messageTime, companyCode, "", "", portCode, context.PriceItemCode, context.PriceItemDescription, usageCount);
					}
				}
			}
		}

		#endregion

		#endregion

		#region SQL Queries

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage =
@"SELECT 
	TX_ClientID,
	CompanyCode = ISNULL(LCC_Code, ''),
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
	AND TX_PriceItemCode = 'ISF'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
ORDER BY
	TX_ServiceOccuredUTC
";

		const string OdplRawUsageSql =
@"select U1_SubCode, CAST(U1_UnitCount AS INT) U1_UnitCount, U1_SystemCreateTimeUtc
from dbo.ClientChargeableUsage
where 
	U1_PeriodStart = @PeriodStart 
	and U1_Code = 'ISF' 
	and U1_LCC = @ClientCompanyPk";

		const string StlRawUsageSql =
@"select 
	LCC_Code, U1_SubCode, CAST(U1_UnitCount AS INT) U1_UnitCount, U1_SystemCreateTimeUtc
from 
	dbo.ClientChargeableUsage
	JOIN dbo.ClientCompany ON U1_LCC = LCC_PK
where 
	U1_PeriodStart = @PeriodStart 
	and U1_Code = 'ISF' 
	and LCC_LD = @DatabasePk
";

		#endregion

		#region Load Usages

		protected override void AddAdditionalFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			chargeableUsageQuery.OrderBy = ClientChargeableUsageSchema.Constants.U1_SubCode;
		}

		#endregion

	}
}

