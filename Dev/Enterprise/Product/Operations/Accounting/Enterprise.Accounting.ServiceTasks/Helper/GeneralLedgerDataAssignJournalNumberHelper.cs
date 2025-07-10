using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ServiceTasks
{
	public class GeneralLedgerDataAssignJournalNumberHelper
	{
		public void AssignJournalNumberForGLD(GlbCompany company)
		{
			var companyPk = company.PK.ToGuid();
			var journalEntriesNumberCustomisationRegistry = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			if (journalEntriesNumberCustomisationRegistry.AllocationOption != AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN)
			{
				return;
			}

			IDisposable tempEnvironment;
			if ((tempEnvironment = DisposableEnvironment.ForCompany(company.GC_Code, reportInactive: false)) != null)
			{
				using (tempEnvironment)
				{
					var startDate = AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
					while ((startDate = QueryEarliestPostDate(companyPk, startDate)) < DateTime.MaxValue)
					{
						var pendingJournalNumberList = QueryPendingJournalNumberData(companyPk, startDate);
						var existJournalNumberList = GetExistJournalNumber(pendingJournalNumberList, companyPk, startDate);
						for (var i = 1; i <= MaxHandleSetCount; i++)
						{
							var needUpdatedGLDList = pendingJournalNumberList.Where(row => row.Sequence == i).ToList();
							var generalLedgerCombinedDataSource = needUpdatedGLDList.FirstOrDefault();
							if (generalLedgerCombinedDataSource == null)
							{
								break;
							}

							using (var manager = Db.Connection.BeginTransactionWithManager())
							{
								var journalNumberEntry = existJournalNumberList.FirstOrDefault(x => x.UniqueKey == generalLedgerCombinedDataSource.UniqueKey);
								journalNumberEntry ??= GenerateJournalNumberAndRuleCode(generalLedgerCombinedDataSource, journalEntriesNumberCustomisationRegistry, companyPk);
								BatchUpdateJournalNumberAndRuleCode(needUpdatedGLDList.Select(row => row.Pk), journalNumberEntry.JournalNumber, journalNumberEntry.JournalRuleCode);

								manager.CommitTransaction();
							}
						}
					}
				}
			}
		}

		List<GeneralLedgerCombinedDataSource> QueryPendingJournalNumberData(Guid companyPk, DateTime startDate)
		{
			var gldList = new List<GeneralLedgerCombinedDataSource>();
			Action<DbCommand> commandAction = command =>
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@StartDate", SqlDbType.SmallDateTime, startDate);
				command.AddParameter("@MaxHandleSetCount", SqlDbType.Int, MaxHandleSetCount);
			};

			Db.Connection.ExecuteReader(
					QueryPendingJournalNumberDataSql,
					commandAction,
					reader =>
					{
						var combinedEntry = new GeneralLedgerCombinedDataSource((Guid)reader["GLD_PK"],
								(DateTime)reader["GLD_PostDate"], reader["Ledger"].ToString(),
								reader["TransactionType"].ToString(),
								reader["TransactionNum"].ToString(),
								(Guid)reader["Branch"],
								(Guid)reader["Department"],
								reader["GLD_AH_TransactionHeader"] != DBNull.Value ? (Guid)reader["GLD_AH_TransactionHeader"] : Guid.Empty,
								reader["GLD_ATM_TaxGLMovement"] != DBNull.Value ? (Guid)reader["GLD_ATM_TaxGLMovement"] : Guid.Empty,
								Convert.ToInt32(reader["Sequence"]),
								Factory);
						gldList.Add(combinedEntry);
					});
			return gldList;
		}

		DateTime QueryEarliestPostDate(Guid companyPk, DateTime startDate)
		{
			Action<DbCommand> commandAction = command =>
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@StartDate", SqlDbType.SmallDateTime, startDate);
			};

			var earliestPostDate = Db.Connection.ExecuteScalar(QueryEarliestPostDateSql, commandAction);
			if (earliestPostDate is DBNull)
			{
				earliestPostDate = DateTime.MaxValue;
			}

			return (DateTime)earliestPostDate;
		}

		List<GeneralLedgerCombinedDataSource> GetExistJournalNumber(List<GeneralLedgerCombinedDataSource> generalLedgerCombinedEntrieList, Guid companyPk, DateTime startDate)
		{
			var headerSet = new HashSet<Guid>();
			var taxGLMovementSet = new HashSet<Guid>();
			var jobNumSet = new HashSet<string>();

			generalLedgerCombinedEntrieList.ForEach(data =>
			{
				if (data.TransactionHeaderPk != Guid.Empty)
				{
					headerSet.Add(data.TransactionHeaderPk);
				}

				if (data.TaxGLMovement != Guid.Empty)
				{
					taxGLMovementSet.Add(data.TaxGLMovement);
				}

				if (data.TransactionType == TransactionLineTypes.Accrual || data.TransactionType == TransactionLineTypes.WIP)
				{
					jobNumSet.Add(data.TransactionNum);
				}
			});

			Action<DbCommand> commandAction = command =>
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@StartDate", SqlDbType.DateTime, startDate);
				command.AddTableValuedParameter("@HeaderPKs", TVPHelper.TVP_uniqueidentifier, headerSet.ToList());
				command.AddTableValuedParameter("@TaxGLMovementPKs", TVPHelper.TVP_uniqueidentifier, taxGLMovementSet.ToList());
				command.AddTableValuedParameter("@JobNums", TVPHelper.TVP_varchar, jobNumSet.ToList());
			};

			var existJournalNumberCollection = new List<GeneralLedgerCombinedDataSource>();
			Db.Connection.ExecuteReader(
					QueryExistJournalNumberSql,
					commandAction,
					data =>
					{
						existJournalNumberCollection.Add(new GeneralLedgerCombinedDataSource(data["Ledger"].ToString(),
								data["TransactionType"].ToString(),
								data["TransactionNum"].ToString(),
								data["JournalNumber"].ToString(),
								data["JournalRuleCode"].ToString()));
					});

			return existJournalNumberCollection;
		}

		GeneralLedgerCombinedDataSource GenerateJournalNumberAndRuleCode(GeneralLedgerCombinedDataSource data, JournalEntriesNumberCustomisationSetting journalEntriesNumberCustomisationRegistry, Guid companyPk)
		{
			var journalEntriesNumber = string.Empty;
			var journalEntriesNumberRuleCode = string.Empty;

			var ledger = data.Ledger;
			var transactionType = data.TransactionType;
			var journalEntriesNumberRule = journalEntriesNumberCustomisationRegistry.NumberRule;
			var groupCollection = AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			var groupCode = groupCollection.Cast<JournalEntriesClassificationGroup>().FirstOrDefault(x =>
							x.TransactionType == transactionType && (string.IsNullOrEmpty(ledger) || x.Ledger == ledger))?.GroupCode ?? ZString.Empty;

			switch (journalEntriesNumberRule)
			{
				case AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL:
					var journalEntriesNumberPool = new AccountingNumberFountainPooler(AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, ZString.Empty, 1000L);
					var journalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(journalEntriesNumberPool,
							AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, groupCode);
					journalEntriesNumber = journalEntriesNumberFountainWrapper.Generate(data);
					journalEntriesNumberRuleCode = journalEntriesNumberRule;
					break;
				case AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP:
					var groupJournalEntriesNumberPool = new AccountingNumberFountainPooler(groupCode, ZString.Empty, 1000L);
					var groupJournalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(groupJournalEntriesNumberPool, groupCode, groupCode);
					journalEntriesNumber = groupJournalEntriesNumberFountainWrapper.Generate(data);
					journalEntriesNumberRuleCode = journalEntriesNumberRule + groupCode;
					break;
				case AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN:
					var journalEntriesNumberKey = ledger + transactionType;
					var transactionTypeJournalEntriesNumberPool = new AccountingNumberFountainPooler(journalEntriesNumberKey, ZString.Empty, 1000L);
					var transactionTypeJournalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(transactionTypeJournalEntriesNumberPool, journalEntriesNumberKey, groupCode);
					journalEntriesNumber = transactionTypeJournalEntriesNumberFountainWrapper.Generate(data);
					journalEntriesNumberRuleCode = ledger.IsNullOrEmpty()
							? AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN + transactionType
							: ledger + transactionType;
					break;
			}

			return new GeneralLedgerCombinedDataSource(journalEntriesNumber, journalEntriesNumberRuleCode);
		}

		void BatchUpdateJournalNumberAndRuleCode(IEnumerable<Guid> gldPks, string journalNumber, string journalRuleCode)
		{
#if NETFRAMEWORK
			var chunkList = gldPks.Chunk(BatchUpdateSize);
#else
			var chunkList = IEnumerableExtensions.Chunk(gldPks, BatchUpdateSize);
#endif

			chunkList.ForEach(batch =>
			{
				Action<DbCommand> commandAction = command =>
				{
					command.AddParameter("@JournalNumber", SqlDbType.NVarChar, journalNumber);
					command.AddParameter("@JournalRuleCode", SqlDbType.NVarChar, journalRuleCode);
					command.AddParameter("@EditUser", SqlDbType.NVarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					command.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", batch);
				};

				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					Db.Connection.ExecuteNonQuery(UpdateJournalNumberSql, commandAction);
					manager.CommitTransaction();
				}
			});
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		readonly int MaxHandleSetCount = 100;
		readonly int BatchUpdateSize = 1000;

		readonly string QueryPendingJournalNumberDataSql = @"
SELECT * FROM (SELECT GLD_PK,
	   GLD_PostDate,
	   Ledger,
	   TransactionType,
	   TransactionNum,
	   Branch,
	   Department,
	   GLD_AH_TransactionHeader,
	   GLD_ATM_TaxGLMovement,
       DENSE_RANK() over ( 
	   ORDER BY
	       CASE 
	           WHEN TransactionType = 'CTR' THEN 
			       CAST(CAST(NeedJournalNumber.GLD_PostDate AS date) AS VARCHAR) + '-' + TransactionNum
	           ELSE 
			       CAST(CAST(NeedJournalNumber.GLD_PostDate AS date) AS VARCHAR) + '-' + Ledger + '-' + TransactionType + '-' + TransactionNum 
	       END
	   ) AS Sequence
FROM (SELECT GLD_PK,
			 GLD_PostDate,
			 AH_Ledger          AS Ledger,
			 AH_TransactionType AS TransactionType,
			 AH_TransactionNum  AS TransactionNum,
			 GLD_GB_Branch      AS Branch,
			 GLD_GE_Department  AS Department,
			 GLD_AH_TransactionHeader,
			 GLD_ATM_TaxGLMovement
	  FROM dbo.AccGeneralLedgerData
			   INNER JOIN dbo.AccTransactionHeader
						  ON GLD_AH_TransactionHeader = AH_PK
	  WHERE GLD_AH_TransactionHeader IS NOT NULL
		AND GLD_JournalEntriesNumber = ''
		AND GLD_GC_Company = @CompanyPk
		AND GLD_PostDate >= @StartDate
		AND GLD_PostDate < DATEADD(day, 1, @StartDate)

	  UNION ALL

	  SELECT GLD_PK,
			 GLD_PostDate,
			 'JC'              AS Ledger,
			 AL_LineType       AS TransactionType,
			 JH_JobNum         AS TransactionNum,
			 GLD_GB_Branch     AS Branch,
			 GLD_GE_Department AS Department,
			 GLD_AH_TransactionHeader,
			 GLD_ATM_TaxGLMovement
	  FROM dbo.AccGeneralLedgerData
			   INNER JOIN dbo.AccTransactionLines ON GLD_AL_TransactionLine = AL_PK
			   INNER JOIN dbo.JobHeader ON JH_PK = AL_JH
	  WHERE GLD_JournalEntriesNumber = ''
		AND GLD_AH_TransactionHeader IS NULL
		AND GLD_AL_TransactionLine IS NOT NULL
		AND GLD_GC_Company = @CompanyPk
		AND GLD_PostDate >= @StartDate
		AND GLD_PostDate < DATEADD(day, 1, @StartDate)
		AND AL_LineType IN ('WIP', 'ACR')


	  UNION ALL

	  SELECT GLD_PK,
			 GLD_PostDate,
			 AH_Ledger          AS Ledger,
			 AH_TransactionType AS TransactionType,
			 AH_TransactionNum  AS TransactionNum,
			 GLD_GB_Branch      AS Branch,
			 GLD_GE_Department  AS Department,
			 GLD_AH_TransactionHeader,
			 GLD_ATM_TaxGLMovement
	  FROM dbo.AccGeneralLedgerData
			   INNER JOIN dbo.AccTaxGLMovement ON ATM_PK = GLD_ATM_TaxGLMovement
			   INNER JOIN dbo.AccTaxTransaction ON ATT_PK = ATM_ATT_TaxTransaction
			   INNER JOIN dbo.AccTransactionHeader ON AH_PK = ATT_AH
	  WHERE GLD_JournalEntriesNumber = ''
		AND GLD_AH_TransactionHeader IS NULL
		AND GLD_AL_TransactionLine IS NULL
		AND GLD_ATM_TaxGLMovement IS NOT NULL
		AND GLD_GC_Company = @CompanyPk
		AND GLD_PostDate >= @StartDate
		AND GLD_PostDate < DATEADD(day, 1, @StartDate)) NeedJournalNumber) TopSet
WHERE Sequence <= @MaxHandleSetCount";

		readonly string QueryEarliestPostDateSql = @"
SELECT CAST(MIN(GLD_PostDate) AS DATE)
FROM dbo.AccGeneralLedgerData
WHERE GLD_GC_Company = @CompanyPk
AND GLD_JournalEntriesNumber = ''
AND GLD_PostDate >= @StartDate
";

		readonly string QueryExistJournalNumberSql = @"
SELECT AH_Ledger                        AS Ledger,
	   AH_TransactionType               AS TransactionType,
	   AH_TransactionNum                AS TransactionNum,
	   GLD_JournalEntriesNumber         AS JournalNumber,
	   GLD_JournalEntriesNumberRuleCode AS JournalRuleCode
FROM dbo.AccGeneralLedgerData
		 INNER JOIN dbo.AccTransactionHeader ON GLD_AH_TransactionHeader = AH_PK
WHERE GLD_JournalEntriesNumber != ''
  AND GLD_AH_TransactionHeader IN (SELECT Value FROM @HeaderPKs)
  AND GLD_GC_Company = @CompanyPk
  AND GLD_PostDate >= @StartDate
  AND GLD_PostDate < DATEADD(day, 1, @StartDate)

UNION ALL

SELECT 'JC'                             AS Ledger,
	   AL_LineType                      AS TransactionType,
	   JH_JobNum                        AS TransactionNum,
	   GLD_JournalEntriesNumber         AS Number,
	   GLD_JournalEntriesNumberRuleCode AS RuleCode
FROM dbo.AccGeneralLedgerData
		 INNER JOIN dbo.AccTransactionLines ON GLD_AL_TransactionLine = AL_PK
		 INNER JOIN dbo.JobHeader ON JH_PK = AL_JH
WHERE GLD_AH_TransactionHeader IS NULL
  AND AL_LineType IN ('WIP', 'ACR')
  AND GLD_JournalEntriesNumber != ''
  AND JH_JobNum IN (SELECT Value FROM @JobNums)
  AND GLD_GC_Company = @CompanyPk
  AND GLD_PostDate >= @StartDate
  AND GLD_PostDate < DATEADD(day, 1, @StartDate)

UNION ALL

SELECT AH_Ledger                        AS Ledger,
	   AH_TransactionType               AS TransactionType,
	   AH_TransactionNum                AS Number,
	   GLD_JournalEntriesNumber         AS Number,
	   GLD_JournalEntriesNumberRuleCode AS RuleCode
FROM dbo.AccGeneralLedgerData
		 INNER JOIN dbo.AccTaxGLMovement ON ATM_PK = GLD_ATM_TaxGLMovement
		 INNER JOIN dbo.AccTaxTransaction ON ATT_AH = ATM_ATT_TaxTransaction
		 INNER JOIN dbo.AccTransactionHeader ON AH_PK = ATT_AH
WHERE GLD_AH_TransactionHeader IS NULL
  AND GLD_AL_TransactionLine IS NULL
  AND GLD_JournalEntriesNumber != ''
  AND GLD_ATM_TaxGLMovement IN (SELECT Value FROM @TaxGLMovementPKs)
  AND GLD_GC_Company = @CompanyPk
  AND GLD_PostDate >= @StartDate
  AND GLD_PostDate < DATEADD(day, 1, @StartDate)";

		readonly string UpdateJournalNumberSql = @"
UPDATE dbo.AccGeneralLedgerData
SET GLD_JournalEntriesNumber = @JournalNumber,
	GLD_JournalEntriesNumberRuleCode = @JournalRuleCode,
	GLD_SystemLastEditTimeUtc = getutcdate(),
	GLD_SystemLastEditUser = @EditUser
WHERE GLD_PK IN (SELECT Value FROM @PKs)";
	}
}
