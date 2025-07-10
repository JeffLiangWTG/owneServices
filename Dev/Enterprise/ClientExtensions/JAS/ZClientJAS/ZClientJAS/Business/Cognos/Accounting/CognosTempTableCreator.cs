using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	class CognosTempTableCreator : IDisposable
	{
		public static class TempTableNames
		{
			public const string CounterCompanies = "#CounterCompanies";
			public const string CognosModes = "#CognosModes";
			public const string CognosRawAggregate = "#CognosRawAggregate";
			public const string CognosExport = "#CognosExport";
		}

		public CognosTempTableCreator()
		{
			CreateCounterCompanyTable();
			CreateCognosModeTable();
			CreateCognosRawAggregateTable();
			CreateCognosExportTable();
		}

		#region Counter Company

		void CreateCounterCompanyTable()
		{
			ExecuteNonQuery(CreateCounterCompanyTableScript);
			ExecuteNonQuery(InsertCounterCompaniesScript, true);
		}

		void DropCounterCompanyTable()
		{
			DropTable(TempTableNames.CounterCompanies);
		}

		const string CreateCounterCompanyTableScript = @"
IF OBJECT_ID('tempdb..#CounterCompanies') IS NOT NULL
    DROP TABLE #CounterCompanies

CREATE TABLE #CounterCompanies
(
    T2_OH               uniqueidentifier,
    T2_CompanyCode      nvarchar(8),
    T2_OG_CreditorGroup uniqueidentifier,
    T2_OJ_DebtorGroup   uniqueidentifier,
    T2_BusinessType     nvarchar(3),
    T2_Geographical     nvarchar(3)
)";

		const string InsertCounterCompaniesScript = @"
INSERT INTO #CounterCompanies
SELECT  OH_PK AS T2_OH,
        CASE
            WHEN OK_OH IS NULL THEN ''
            ELSE OK_CustomsRegNo
        END AS T2_CompanyCode,        
        OB_OG_APCreditorGroup AS T2_OG_CreditorGroup,
        OB_OJ_ARDebtorGroup AS T2_OJ_DebtorGroup,
        '' AS T2_BusinessType,
        CASE
            WHEN LocoZone.ZoneCode IN ('SAMR') THEN 'SAM'
            WHEN LocoZone.ZoneCode IN ('NAMR', 'USAR') THEN 'NAM'
            WHEN LocoZone.ZoneCode IN ('SEAR') THEN 'SEA'
            WHEN LocoZone.ZoneCode IN ('AUSR', 'NZDR') THEN 'ANZ'
            WHEN LocoZone.ZoneCode IN ('MEAR') THEN 'MEA'
            WHEN LocoZone.ZoneCode IN ('EUOR', 'EURR') THEN 'EUR'
            WHEN LocoZone.ZoneCode IN ('AFOR', 'AFRR', 'SAFR') THEN 'AFR'
            WHEN LocoZone.ZoneCode IN ('AMER', 'ASIR', 'CAMR', 'INDR', 'NASR', 'OCER', 'PACR') THEN 'OTH'
            ELSE ''
        END AS T2_Geographical
FROM    dbo.OrgHeader
        LEFT OUTER JOIN dbo.OrgCusCode ON OK_CodeType = 'UNC' AND OK_OH = OH_PK
		LEFT OUTER JOIN csfn_LocoReportingZones(NULL) LocoZone ON LocoZone.LocoCode = OH_RL_NKClosestPort
        LEFT OUTER JOIN dbo.OrgCompanyData ON OB_OH = OH_PK AND OB_GC = @CompanyPK
WHERE   OH_PK IN
        (
            SELECT DISTINCT AH_OH
            FROM    dbo.AccTransactionHeader            
            INNER JOIN dbo.GlbBranch ON GB_PK = AH_GB
            WHERE   GB_GC = @CompanyPK
        )
";

		#endregion

		#region Cognos Modes

		void CreateCognosModeTable()
		{
			ExecuteNonQuery(CreateCognosModeTableScript);

			List<DbCommand> insertCognosModesDbCommandBatchList = new List<DbCommand>();
			CognosModeMapping modeMapping = JASDataRegistry.Instance.CognosModeMapping;
			using (var reader = ExecuteReader(RetrieveAllDepartmentPKScript))
			{
				const int InsertBatchSize = 50;
				int insertCount = 0;
				int batchCount = 0;
				DbCommand currentInsertDbCommand = GetDbCommand("");

				while (reader.Read())
				{
					ZGuid departmentPK = new ZGuid(reader[GlbDepartmentSchema.Constants.PK]);
					string mode = modeMapping.GetCognosMode(departmentPK);
					if (!string.IsNullOrEmpty(mode))
					{
						string t3_GEParamName = string.Format("@T3_GE_{0}_{1}", batchCount, insertCount);
						string t3_ModeParamName = string.Format("@T3_Mode_{0}_{1}", batchCount, insertCount);

						currentInsertDbCommand.CommandText += string.Format(InsertCognosModeScript, t3_GEParamName, t3_ModeParamName);
						currentInsertDbCommand.AddParameter(t3_GEParamName, SqlDbType.UniqueIdentifier, departmentPK.ToGuid());
						currentInsertDbCommand.AddParameter(t3_ModeParamName, SqlDbType.NVarChar, 250, mode);

						insertCount++;
						if (insertCount >= InsertBatchSize)
						{
							insertCount = 0;
							batchCount++;
							insertCognosModesDbCommandBatchList.Add(currentInsertDbCommand);
							currentInsertDbCommand = GetDbCommand("");
						}
					}
				}

				if (!string.IsNullOrEmpty(currentInsertDbCommand.CommandText))
				{
					insertCognosModesDbCommandBatchList.Add(currentInsertDbCommand);
				}
			}

			foreach (DbCommand insertDbCommand in insertCognosModesDbCommandBatchList)
			{
				insertDbCommand.ExecuteNonQuery();
			}
		}

		void DropCognosModeTable()
		{
			DropTable(TempTableNames.CognosModes);
		}

		const string CreateCognosModeTableScript = @"
IF OBJECT_ID('tempdb..#CognosModes') IS NOT NULL
    DROP TABLE #CognosModes

CREATE TABLE #CognosModes
(
    T3_GE           uniqueidentifier,
    T3_Mode         nvarchar(4)    
)";

		const string RetrieveAllDepartmentPKScript = "SELECT GE_PK FROM dbo.GlbDepartment";
		const string InsertCognosModeScript = "INSERT INTO #CognosModes VALUES ({0}, {1})\r\n";

		#endregion

		#region Cognos Raw Aggregate

		void CreateCognosRawAggregateTable()
		{
			ExecuteNonQuery(CreateCognosRawAggregateTableScript);
		}

		void DropCognosRawAggregateTable()
		{
			DropTable(TempTableNames.CognosRawAggregate);
		}

		const string CreateCognosRawAggregateTableScript = @"
IF OBJECT_ID('tempdb..#CognosRawAggregate') IS NOT NULL
    DROP TABLE #CognosRawAggregate

CREATE TABLE #CognosRawAggregate
(
    T5_AJ                   uniqueidentifier,    
    T5_Age                  varchar(2),
    T5_CompanyCode          nvarchar(8),
    T5_OG_CreditorGroup     uniqueidentifier,
    T5_OJ_DebtorGroup       uniqueidentifier,    
    T5_Mode                 nvarchar(4),
    T5_Branch               nvarchar(3),
    T5_BusinessType         nvarchar(3),
    T5_Amount               money,
    T5_TransactionCurrency  char(3),
    T5_TransactionAmount    money,
    T5_Geographical         nvarchar(3)
)";

		#endregion

		#region Cognos Export

		void CreateCognosExportTable()
		{
			ExecuteNonQuery(CreateCognosExportTableScript);
		}

		void DropCognosExportTable()
		{
			DropTable(TempTableNames.CognosExport);
		}

		const string CreateCognosExportTableScript = @"
IF OBJECT_ID('tempdb..#CognosExport') IS NOT NULL
    DROP TABLE #CognosExport

CREATE TABLE #CognosExport
(
    T6_AJ                   uniqueidentifier,
    T6_CompanyCode          nvarchar(8),
    T6_Mode                 nvarchar(4),
    T6_Branch               nvarchar(3),
    T6_BusinessType         nvarchar(3),
    T6_Amount               money,
    T6_TransactionCurrency  varchar(3),
    T6_TransactionAmount    money,
    T6_Geographical         nvarchar(3)
)";

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			DropCounterCompanyTable();
			DropCognosModeTable();
			DropCognosRawAggregateTable();
			DropCognosExportTable();
		}

		#endregion

		#region Implementation

		void DropTable(string tableName)
		{
			ExecuteNonQuery(string.Format("IF OBJECT_ID('tempdb..{0}') IS NOT NULL DROP TABLE {0}", tableName));
		}

		void ExecuteNonQuery(string commandText)
		{
			ExecuteNonQuery(commandText, false);
		}

		void ExecuteNonQuery(string commandText, bool requiresCompanyPK)
		{
			DbCommand dbCommand = GetDbCommand(commandText, requiresCompanyPK);
			dbCommand.ExecuteNonQuery();
		}

		IDataReader ExecuteReader(string commandText)
		{
			DbCommand dbCommand = GetDbCommand(commandText, false);
			return dbCommand.ExecuteReader();
		}

		DbCommand GetDbCommand(string commandText)
		{
			return GetDbCommand(commandText, false);
		}

		DbCommand GetDbCommand(string commandText, bool requiresCompanyPK)
		{
			DbCommand result = Db.Connection.Command(commandText);
			if (requiresCompanyPK)
			{
				result.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, Env.CurrentCompany.PK);
			}
			return result;
		}

		#endregion
	}
}
