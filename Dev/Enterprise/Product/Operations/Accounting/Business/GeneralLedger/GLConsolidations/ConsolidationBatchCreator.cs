using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchCreator
	{
		public ConsolidationBatchCreator(ZGuid consolidationGroupPK)
		{
			this.consolidationGroupPK = consolidationGroupPK;
		}

		readonly ZGuid consolidationGroupPK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		const int sqlCommandTimeoutInSeconds = 1800; // 30 minutes.

		public void ReadDataAndCreateBatches()
		{
			var factory = new BusinessObjectFactory();
			var consolidationGroup = factory.Load<AccConsolidationGroup>(consolidationGroupPK);
			var childGroups = consolidationGroup.GetAllDirectChildrenGroups();

			foreach (var childGroup in childGroups)
			{
				new ConsolidationBatchCreator(childGroup.PK).ReadDataAndCreateBatches();
			}

			ReadDataAndCreateBatchesCore(factory);
		}

		void ReadDataAndCreateBatchesCore(BusinessObjectFactory factory)
		{
			var result = new List<ConsolidationBatchInfo>();
			var consolidationGroup = factory.Load<AccConsolidationGroup>(consolidationGroupPK);
			var childGroups = consolidationGroup.GetAllGetChildGroupsIncludingDescendents();
			ZString childGroupPKsAsString = GetCommaDelimitedString(childGroups);
			var allCompaniesToProcess = new List<ZGuid>(consolidationGroup.GetChildCompanies());

			bool allCompaniesHaveEliminationCategoryDefined = true;

			foreach (var company in allCompaniesToProcess)
			{
				var eliminationCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(company.ToGuid(), Guid.Empty, Guid.Empty).EliminationCategory;

				if (eliminationCategory == null)
				{
					allCompaniesHaveEliminationCategoryDefined = false;
					break;
				}
			}

			if (allCompaniesHaveEliminationCategoryDefined)
			{
				foreach (var childGroup in childGroups)
				{
					allCompaniesToProcess.AddRange(childGroup.GetChildCompanies());
				}

				foreach (var company in allCompaniesToProcess)
				{
					var eliminationCategoryCode = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(company.ToGuid(), Guid.Empty, Guid.Empty).EliminationCategory.Code;
					var batches = GetPotentialBatches(consolidationGroup.PK, company, childGroupPKsAsString, consolidationGroup.YR_HighWatermark, eliminationCategoryCode);

					foreach (var batch in batches)
					{
						AssignBatchNumberAndSaveToDatabase(batch);
						result.Add(batch);
					}

					CreateAutoJournalBatches(consolidationGroup.PK, company, childGroupPKsAsString, consolidationGroup.YR_HighWatermark);
				}
			}
		}

		void CreateAutoJournalBatches(ZGuid consolidationGroup, ZGuid company, ZString childGroupPKs, ZDateTime highWaterMark)
		{
			var autoJournalPKs = new List<Guid>();

			using (var command = CommandForGetAutoJournalsForBatching(consolidationGroup, company, childGroupPKs, highWaterMark))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var aH_PK = (Guid)reader[AccTransactionHeaderSchema.Constants.PK];
						autoJournalPKs.Add(aH_PK);
					}
				}
			}

			foreach (var autoJournalPK in autoJournalPKs)
			{
				using (var manager = Db.Connection.BeginTransactionWithManager()) // Not using factories because of performance for batch operations
				{
					var readonlyFactory = new BusinessObjectFactory();
					var autoJournal = readonlyFactory.Load<GLJournals.GLJournal>(autoJournalPK);
					var linePKs = autoJournal.Lines.GetPKs();
					var periodsQuery = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, autoJournal.AH_GC);
					periodsQuery.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, autoJournal.AH_PostDate);
					periodsQuery.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, autoJournal.AH_DueDate);
					var periodPKs = readonlyFactory.Load<AccPeriodManagement>(periodsQuery).Select(x => x.PK);

					var batchNumber = Env.NumberFountains.GeneralLedgerConsolidationBatchNo.GetNext(Db.Connection);
					InsertGenExportBatchSequenceRecords(batchNumber, "ENP", linePKs);
					InsertAccConsolidationBatchRecords(batchNumber, autoJournal.AH_GC, consolidationGroup, periodPKs);

					manager.CommitTransaction(); // Not using factories because of performance for batch operations
				}
			}
		}

		void InsertGenExportBatchSequenceRecords(long batchNumber, string xB_Type, IEnumerable<ZGuid> linePKs)
		{
			var insertSQL =
@"INSERT INTO dbo.GenExportBatchSequence (XB_PK, XB_Type, XB_BatchNumber, XB_Sequence, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser) 
VALUES (newid(), '{0}', '{1}', 0, 'AL', '{2}', GETUTCDATE(), '~BP') ";

			var sql = new StringBuilder();

			foreach (var linePK in linePKs)
			{
				sql.AppendFormat(insertSQL, xB_Type, batchNumber.ToString(CultureInfo.InvariantCulture), linePK.ToString());
			}

			using (var cmd = GetDbCommandWithExtendedTimeout(sql.ToString())) // Not using factories because of performance for batch operations
			{
				cmd.ExecuteNonQuery();
			}
		}

		void InsertAccConsolidationBatchRecords(long batchNumber, ZGuid companyPK, ZGuid currentConsolidationGroupPK, IEnumerable<ZGuid> periodPKs)
		{
			var insertSQL = @"
INSERT INTO dbo.AccConsolidationBatch (YB_PK, YB_GC_Company, YB_BatchNumber, YB_AM_Period, YB_AH_EliminationJournal, YB_YR_ConsolidationGroup)
VALUES (newid(), '{0}', {1}, '{2}', NULL, '{3}') ";

			var sql = new StringBuilder();

			foreach (var periodPK in periodPKs)
			{
				sql.AppendFormat(CultureInfo.InvariantCulture, insertSQL, companyPK.ToString(), batchNumber.ToString(CultureInfo.InvariantCulture), periodPK.ToString(), currentConsolidationGroupPK.ToString());
			}

			using (var cmd = GetDbCommandWithExtendedTimeout(sql.ToString())) // Not using factories because of performance for batch operations
			{
				cmd.ExecuteNonQuery();
			}
		}

		DbCommand CommandForGetAutoJournalsForBatching(ZGuid consolidationGroup, ZGuid company, ZString childGroupPKs, ZDateTime highWaterMark)
		{
			var eliminationCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(company.ToGuid(), Guid.Empty, Guid.Empty).EliminationCategory.Code;
			var companyParameterName = "@Company_" + ParameterSuffixer.Instance.GetParameterSuffix(ZDateTime.Now.ToDateTime(), AccTransactionHeaderSchema.AH_GC, company.ToGuid());

			var sql = string.Format(CultureInfo.InvariantCulture,
@"SELECT DISTINCT AH_PK
FROM dbo.AccTransactionHeader
JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
LEFT OUTER JOIN dbo.GenExportBatchSequence
	ON XB_ParentID = AL_PK
	AND XB_Type IN ('ENP', 'ERP')
INNER JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC AND AM_EndDate BETWEEN AH_PostDate AND AH_DueDate
WHERE AH_GC = {1}
AND AH_Ledger = 'GL'
AND AH_TransactionType = 'AJL'
AND AH_TransactionCategory IN ('', '{0}')
AND XB_ParentID IS NULL", eliminationCategory, companyParameterName);

			var command = GetDbCommandWithExtendedTimeout(sql); // Not using factories because of performance for batch operations
			command.AddParameter(companyParameterName, SqlDbType.UniqueIdentifier, company.ToGuid());

			if (highWaterMark.IsValid)
			{
				command.CommandText += " AND AH_SystemLastEditTimeUtc > @HighWaterMark;";
				command.AddParameter("@HighWaterMark", SqlDbType.SmallDateTime, highWaterMark.AddDays(-2).ToDateTime());
			}
			else
			{
				command.CommandText += ";";
			}

			return command;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetDbCommandWithExtendedTimeout(string sqlText)
		{
			return Db.Connection.Command(sqlText, sqlCommandTimeoutInSeconds); // Not using factories because of performance for batch operations
		}

		IEnumerable<ConsolidationBatchInfo> GetPotentialBatches(ZGuid consolidationGroup, ZGuid company, ZString childGroupPKs, ZDateTime highWaterMark, ZString eliminationJournalCategory)
		{
			var batches = new List<ConsolidationBatchInfo>();
			ConsolidationBatchInfo currentBatch = null;

			using (var command = CommandForGetPotentialBatch(consolidationGroup, company, childGroupPKs, highWaterMark, eliminationJournalCategory))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var type = (string)reader["Type"];
						var periodInt = (int)reader["PeriodInt"];
						var periodPK = (Guid)reader["PeriodPK"];
						var parentID = (Guid)reader["ParentID"];
						var parentTableCode = (string)reader["ParentTableCode"];

						if (currentBatch == null || currentBatch.PeriodInt != periodInt)
						{
							currentBatch = new ConsolidationBatchInfo(company, consolidationGroupPK, periodPK, periodInt);
							batches.Add(currentBatch);
						}

						currentBatch.BatchRows.Add(new BatchRow(parentID, parentTableCode, type));
					}
				}
			}

			return batches;
		}

		DbCommand CommandForGetPotentialBatch(ZGuid consolidationGroup, ZGuid company, ZString childGroupPKs, ZDateTime highWaterMark, ZString eliminationJournalCategory)
		{
			var command = GetDbCommandWithExtendedTimeout((NoResString)"EXEC GLConsolidationsGetPotentialBatch @ConsolidationGroup, @Company, @ChildGroupPKs, @EliminationJournalCategory"); // This is a call to a stored proc, not something we're showing to the user.
			command.AddParameter("@ConsolidationGroup", SqlDbType.UniqueIdentifier, consolidationGroup.ToGuid());
			command.AddParameter("@Company", SqlDbType.UniqueIdentifier, company.ToGuid());
			command.AddParameter("@ChildGroupPKs", SqlDbType.VarChar, int.MaxValue, childGroupPKs.ToString());
			command.AddParameter("@EliminationJournalCategory", SqlDbType.Char, 3, eliminationJournalCategory.ToString());

			if (highWaterMark.IsValid)
			{
				command.CommandText += ", @HighWaterMark;";
				command.AddParameter("@HighWaterMark", SqlDbType.SmallDateTime, highWaterMark.AddDays(-2).ToDateTime());
			}
			else
			{
				command.CommandText += ";";
			}

			return command;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types")]
		void AssignBatchNumberAndSaveToDatabase(ConsolidationBatchInfo batchInfo)
		{
			if (batchInfo.BatchRows.Any())
			{
				using (var manager = Db.Connection.BeginTransactionWithManager(onAutoRollbackAction: () => // Not using factories because of performance for batch operations
				{
					batchInfo.BatchNumber = -1;
					batchInfo.BatchPK = ZGuid.Empty;
				}))
				{
					batchInfo.BatchNumber = Env.NumberFountains.GeneralLedgerConsolidationBatchNo.GetNext(Db.Connection);
					batchInfo.BatchPK = ZGuid.NewZGuid();

					InsertGenExportBatchSequenceRows(batchInfo);
					InsertAccConsolidationBatchRows(batchInfo);

					using (var command = GetDbCommandWithExtendedTimeout(string.Format("UPDATE dbo.AccConsolidationGroup SET YR_HighWatermark = GETDATE(), YR_SystemLastEditTimeUtc = GETUTCDATE(), YR_SystemLastEditUser = @SystemLastEditUser WHERE YR_PK = '{0}';", consolidationGroupPK))) // Not using factories because of performance for batch operations
					{
						command.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
						command.ExecuteNonQuery();
					}

					manager.CommitTransaction(); // Not using factories because of performance for batch operations
				}
			}
		}

		void InsertAccConsolidationBatchRows(ConsolidationBatchInfo batchInfo)
		{
			var sql = string.Format(@"INSERT INTO dbo.AccConsolidationBatch (YB_PK, YB_GC_Company, YB_BatchNumber, YB_AM_Period, YB_AH_EliminationJournal, YB_YR_ConsolidationGroup)
                                   VALUES ('{0}', '{1}', {2}, '{3}', NULL, '{4}')", batchInfo.BatchPK, batchInfo.Company, batchInfo.BatchNumber, batchInfo.PeriodPK, batchInfo.ConsolidationGroup);

			using (var command = GetDbCommandWithExtendedTimeout(sql)) // Not using factories because of performance for batch operations
			{
				command.ExecuteNonQuery();
			}
		}

		void InsertGenExportBatchSequenceRows(ConsolidationBatchInfo batchInfo)
		{
			ZStringBuilder sql = null;
			int totalRows = batchInfo.BatchRows.Count;
			int rowNumber = 0;
			const string sqlForSingleRow = "(NEWID(), '{0}', {1}, 0, '{2}', '{3}', GETUTCDATE(), '~BP'),";

			foreach (var batchRow in batchInfo.BatchRows)
			{
				if (sql == null)
				{
					sql = new ZStringBuilder("INSERT INTO dbo.GenExportBatchSequence (XB_PK, XB_Type, XB_BatchNumber, XB_Sequence, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser) VALUES ");
				}

				sql.Append(string.Format(sqlForSingleRow, batchRow.Type, batchInfo.BatchNumber, batchRow.ParentTableCode, batchRow.ParentID));
				rowNumber++;

				if (rowNumber % 500 == 0 || rowNumber == totalRows)
				{
					using (var command = GetDbCommandWithExtendedTimeout(sql.ToStringWithNewLineBetweenAppends().TrimEnd(','))) // Not using factories because of performance for batch operations
					{
						command.ExecuteNonQuery();
					}

					sql = null;
				}
			}
		}

		string GetCommaDelimitedString(IEnumerable<AccConsolidationGroup> childGroups)
		{
			if (!childGroups.Any())
			{
				return ZGuid.Empty.ToString();
			}

			ZStringBuilder builder = new ZStringBuilder();

			foreach (var childGroup in childGroups)
			{
				builder.Append(childGroup.PK.ToString());
			}

			return builder.ToStringWithDelimiterBetweenAppends(",");
		}
	}
}
