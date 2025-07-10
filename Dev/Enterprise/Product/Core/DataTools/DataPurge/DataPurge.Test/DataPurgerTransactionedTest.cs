using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	public class DataPurgerTransactionedTest : TransactionedTestCase
	{
		#region CompanySpecificScriptPurgesJobStorages

		public void TestCompanySpecificScriptPurgesJobStorages()
		{
			// Setup
			var company1Pk = CreateCompany("TC1", "Test Company 1");
			var company2Pk = CreateCompany("TC2", "Test Company 2");
			var branch1Pk = CreateBranch("TB1", "Test Branch 1 ", company1Pk);
			var branch2Pk = CreateBranch("TB2", "Test Branch 2", company2Pk);
			var departmentPk = CreateDepartment("TSD", "Test Department");
			var bookingDate1 = ZDateTime.Now.AddDays(-1);
			var bookingDate2 = ZDateTime.Now.AddDays(-2);

			var warehousePk = CreateWarehouse("W1");
			var clientPk = GetOrgHeader();

			CreateWhsDocket(ZGuid.Empty, clientPk, warehousePk, "INW", "REC", "FIN", "R1", ZDateTime.Today, ZGuid.Empty, ZString.Empty);
			CreateJobStorage("111", "AAA", ZDateTime.Now, bookingDate1, bookingDate1, clientPk, warehousePk);

			var jobStorage1WithJobHeaderPk = CreateJobStorage("222", "BBB", ZDateTime.Now, bookingDate2, bookingDate2, clientPk, warehousePk);
			CreateJobHeader(jobStorage1WithJobHeaderPk, "ET", "111", company1Pk, branch1Pk, departmentPk);

			var jobStorage2WithJobHeaderPk = CreateJobStorage("333", "CCC", ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, clientPk, warehousePk);
			CreateJobHeader(jobStorage2WithJobHeaderPk, "ET", "222", company2Pk, branch2Pk, departmentPk);

			var jobStorageCountSql = "SELECT count(*) FROM dbo.JobStorage";

			var jobStoragesCountForCompanySql = String.Format(@"
				SELECT count(*)
				FROM dbo.JobHeader
				WHERE JH_GC = '{0}'", company1Pk);

			var jobStoragesWithoutJobHeadersCountSql = @"
				SELECT count(*)
				FROM dbo.JobStorage
				WHERE ET_PK NOT in (SELECT JH_ParentID FROM dbo.JobHeader)";

			// Preconditions
			AssertEquals("[BEFORE PURGE] There should be 3 job storages before purge.", 3, TestConnection.ExecuteScalar(jobStorageCountSql));

			var jobStorage1Count1 = Convert.ToInt32(TestConnection.ExecuteScalar(jobStoragesCountForCompanySql));
			AssertEquals("[BEFORE PURGE] There should be 1 Job Storage linked to the company.", 1, jobStorage1Count1);

			var jobStorageWithoutJobHeadersCount1 = Convert.ToInt32(TestConnection.ExecuteScalar(jobStoragesWithoutJobHeadersCountSql));
			AssertEquals("[BEFORE PURGE] There should be 1 orphaned Job Storage.", 1, jobStorageWithoutJobHeadersCount1);

			// Purge
			var purger = new DataPurger { CompanySpecificPk = company1Pk };
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts(isCompanySpecificPurge: true));

			// Postconditions
			AssertEquals("[AFTER PURGE] There should be 1 job storage left.", 1, TestConnection.ExecuteScalar(jobStorageCountSql));

			var jobStorage1Count2 = Convert.ToInt32(TestConnection.ExecuteScalar(jobStoragesCountForCompanySql));
			AssertEquals("[AFTER PURGE] There should be NO JobHeaders linked to the company", 0, jobStorage1Count2);

			var jobStorageWithoutJobHeadersCount2 = Convert.ToInt32(TestConnection.ExecuteScalar(jobStoragesWithoutJobHeadersCountSql));
			AssertEquals("[AFTER PURGE] The linked Job Storage should be deleted.", 0, jobStorageWithoutJobHeadersCount2);
		}

		#endregion

		#region AccGeneralLedgerData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		public void TestCompanySpecificScriptPurgeAccGeneralLedgerDatas()
		{
			var company1Pk = CreateCompany("TC3", "Test Company 1");
			var company2Pk = CreateCompany("TC4", "Test Company 2");
			var branch1Pk = CreateBranch("TB1", "Test Branch 1 ", company1Pk);
			var branch2Pk = CreateBranch("TB2", "Test Branch 2", company2Pk);
			var departmentPk = CreateDepartment("TD1", "Test Department");
			var postDate = new DateTime(2023, 05, 23);
			var glAccount1Pk = CreateAccGLHeader("1111.11.91", "P&L", "DR");
			var glAccount2Pk = CreateAccGLHeader("1111.11.92", "P&L", "DR");

			var testTransactionHeaderPk = Guid.NewGuid();
			var sqlText = String.Format("INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES ('{0}', '{1}', '{2}', '{3}', getdate(), 'JC', 'OVP')", testTransactionHeaderPk, company1Pk, branch1Pk, departmentPk);
			TestConnection.ExecuteNonQuery(sqlText);

			var testTransactionLinePk = Guid.NewGuid();
			sqlText = String.Format("INSERT dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', 'DRC')", testTransactionLinePk, testTransactionHeaderPk, company1Pk, branch1Pk, departmentPk);
			TestConnection.ExecuteNonQuery(sqlText);

			var cashBasisVATPk = Guid.NewGuid();
			sqlText = String.Format("INSERT dbo.AccCashBasisVAT (YC_PK, YC_PostDate, YC_TaxAmount, YC_TaxBaseAmount, YC_AL_TransactionLine, YC_GC, YC_MatchGroupNum, YC_SystemCreateTimeUtc, YC_SystemCreateUser, YC_SystemLastEditTimeUtc, YC_SystemLastEditUser)  VALUES ('{0}', GetDate(), 9, 0.9, '{1}', '{2}', 'M00011047', GETUTCDATE(), 'E', GETUTCDATE(), 'E')", cashBasisVATPk, testTransactionLinePk, company1Pk);
			TestConnection.ExecuteNonQuery(sqlText);

			var taxConfigPk = Guid.NewGuid();
			sqlText = String.Format("INSERT dbo.AccTaxConfiguration(ETC_PK, ETC_ParentId, ETC_ParentTableCode, ETC_Code,ETC_TaxAuthorityCode, ETC_TaxSystemCode,ETC_RN_NKCountry,ETC_Ledger, ETC_CancellationPolicy, ETC_TaxRealisationMethod, ETC_TaxRecordCreationTrigger, ETC_Description, ETC_SystemCreateTimeUtc, ETC_SystemCreateUser, ETC_SystemLastEditTimeUtc, ETC_SystemLastEditUser) values ('{0}', NewID(), 'GB', 'EUU','EUU','S','AU','AR', 'NAL', 'PDT', 'PDT', 'Desc', GETUTCDATE(),'E',GETUTCDATE(),'E')", taxConfigPk);
			TestConnection.ExecuteNonQuery(sqlText);

			var taxPk = Guid.NewGuid();
			sqlText = String.Format($@"
INSERT INTO dbo.AccTaxRate (AT_PK, AT_Code, AT_IsActive, AT_Description, AT_Type, AT_ExtraTaxRateType, AT_RN_NKCountry, AT_A9_DefaultVatClass, AT_PostingGroupId, AT_ReferenceExtraRateType, AT_ReferenceRateType)
VALUES ('{taxPk}', 'GST', 1, '', 'RAT', '', '', NULL, 1, '', '')");
			TestConnection.ExecuteNonQuery(sqlText);

			var taxTransactionPk = Guid.NewGuid().ToString();
			sqlText = String.Format("INSERT dbo.AccTaxTransaction (ATT_PK, ATT_GC, ATT_AH, ATT_ETC, ATT_TaxDate, ATT_PostDate, ATT_AT_TaxID, ATT_GB, ATT_GE_Department, ATT_Ledger, ATT_Basis, ATT_RX_NKOSTaxCurrency, ATT_RateDenominator, ATT_RateNumerator, ATT_TaxSystemCode, ATT_TaxSuperType, ATT_SystemCreateTimeUtc, ATT_SystemCreateUser, ATT_SystemLastEditTimeUtc, ATT_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}','{3}', GETDATE(), GETDATE(), '{4}', '{5}', '{6}', 'AR', 'PST', 'USD', 1, 2, 'S', 'PER', GETUTCDATE(), 'E', GETUTCDATE(), 'E')", taxTransactionPk, company1Pk, testTransactionHeaderPk, taxConfigPk, taxPk, branch1Pk, departmentPk);
			TestConnection.ExecuteNonQuery(sqlText);
			var gLMovementPk = Guid.NewGuid();
			sqlText = String.Format("INSERT dbo.AccTaxGLMovement (ATM_PK, ATM_ATT_TaxTransaction, ATM_Type, ATM_Amount, ATM_Date, ATM_Period, ATM_AG_DebitAccount, ATM_AG_CreditAccount, ATM_SystemCreateTimeUtc, ATM_SystemCreateUser, ATM_SystemLastEditTimeUtc, ATM_SystemLastEditUser) VALUES ('{0}', '{1}', 'NRM', 100, GETDATE(), '202305', '{2}', '{3}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')", gLMovementPk, taxTransactionPk, glAccount1Pk, glAccount2Pk);
			TestConnection.ExecuteNonQuery(sqlText);

			CreateAccGeneralLedgerData(company1Pk, branch1Pk);
			CreateAccGeneralLedgerData(company2Pk, branch1Pk);
			CreateAccGeneralLedgerData(company2Pk, branch2Pk, testTransactionHeaderPk);
			CreateAccGeneralLedgerData(company2Pk, branch2Pk, linePk: testTransactionLinePk);
			CreateAccGeneralLedgerData(company2Pk, branch2Pk, cashBasisVatPk: cashBasisVATPk);
			CreateAccGeneralLedgerData(company2Pk, branch2Pk, taxGLMovementPk: gLMovementPk);
			CreateAccGeneralLedgerData(company2Pk, branch2Pk);

			AssertRelatedTableRowCount(false);

			var purger = new DataPurger { CompanySpecificPk = company1Pk };
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts(isCompanySpecificPurge: true));

			AssertRelatedTableRowCount(true);

			void AssertRelatedTableRowCount(bool isEmpty)
			{
				var headerCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccTransactionHeader where AH_GC = '{company1Pk}'");
				var linesCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccTransactionLines where AL_GC = '{company1Pk}'");
				var cashBasisVatCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccCashBasisVAT  where YC_GC = '{company1Pk}'");
				var glMovementCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccTaxGLMovement where ATM_ATT_TaxTransaction in (select ATT_PK from dbo.AccTaxTransaction where ATT_GC = '{company1Pk}')");
				var glDataCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccGeneralLedgerData where GLD_GC_Company = '{company1Pk}'");
				var otherGlDataCount = TestConnection.ExecuteScalar<int>($"SELECT count(*) FROM dbo.AccGeneralLedgerData where GLD_GC_Company != '{company1Pk}'");

				Assert("Data of non-specific company has no changes", otherGlDataCount > 0);
				if (isEmpty)
				{
					AssertArrayEqualsByElements("There is no data related to specific company after purging", new int[] { 0, 0, 0, 0, 0 }, new int[] { headerCount, linesCount, cashBasisVatCount, glMovementCount, glDataCount });
				}
				else
				{
					CombineAssertions("There is data related to specific company before purging", () =>
					{
						Assert(headerCount > 0);
						Assert(linesCount > 0);
						Assert(cashBasisVatCount > 0);
						Assert(glMovementCount > 0);
						Assert(glDataCount > 0);
					});
				}
			}

			void CreateAccGeneralLedgerData(ZGuid companyPk, ZGuid branchPk, Guid? headerPk = null, Guid? linePk = null, Guid? cashBasisVatPk = null, Guid? taxGLMovementPk = null)
			{
				using (var command = TestConnection.Command(@"
				INSERT dbo.AccGeneralLedgerData (
GLD_GC_Company,GLD_PostDate,GLD_PostPeriod,GLD_AG_GLAccount,GLD_AH_TransactionHeader,GLD_AL_TransactionLine,GLD_GB_Branch,GLD_GB_TaxBranch,GLD_GE_Department,GLD_Currency,GLD_YC_CashBasisVAT,GLD_ATM_TaxGLMovement,GLD_SystemCreateTimeUtc,GLD_SystemCreateUser,GLD_SystemLastEditTimeUtc,GLD_SystemLastEditUser,GLD_GLAccountType,GLD_Type,GLD_PK)
VALUES (@companyPk, @postDate, @postPeriod, @gLAccount, @headerPk, @linePk, @branchPk, @branchPk, @departmentPk, @currency, @cashBasisVATPk, @taxGLMovementPk, GetUTCDate(), 'E', GetUTCDate(), 'E', 'ARC', 'PST', NEWID())"))
				{
					command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk.ToGuid());
					command.AddParameter("@postDate", SqlDbType.SmallDateTime, postDate);
					command.AddParameter("@postPeriod", SqlDbType.Int, 202304);
					command.AddParameter("@gLAccount", SqlDbType.UniqueIdentifier, glAccount1Pk.ToGuid());
					if (headerPk != null)
					{
						command.AddParameter("@headerPk", SqlDbType.UniqueIdentifier, headerPk);
					}
					else
					{
						command.AddParameter("@headerPk", SqlDbType.UniqueIdentifier, DBNull.Value);
					}

					if (linePk != null)
					{
						command.AddParameter("@linePk", SqlDbType.UniqueIdentifier, linePk);
					}
					else
					{
						command.AddParameter("@linePk", SqlDbType.UniqueIdentifier, DBNull.Value);
					}

					command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPk.ToGuid());
					command.AddParameter("@departmentPk", SqlDbType.UniqueIdentifier, departmentPk.ToGuid());
					command.AddParameter("@currency", SqlDbType.VarChar, "TWD");

					if (cashBasisVatPk != null)
					{
						command.AddParameter("@cashBasisVATPk", SqlDbType.UniqueIdentifier, cashBasisVatPk);
					}
					else
					{
						command.AddParameter("@cashBasisVATPk", SqlDbType.UniqueIdentifier, DBNull.Value);
					}

					if (taxGLMovementPk != null)
					{
						command.AddParameter("@taxGLMovementPk", SqlDbType.UniqueIdentifier, taxGLMovementPk);
					}
					else
					{
						command.AddParameter("@taxGLMovementPk", SqlDbType.UniqueIdentifier, DBNull.Value);
					}

					command.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Work Items

		public void TestPurgeWorkItem()
		{
			// Setup
			var company1Pk = CreateCompany("TC1", "Test Company 1");
			var company2Pk = CreateCompany("TC2", "Test Company 2");
			var company3Pk = ZGuid.Empty;

			var workItemComp1 = CreateWorkItem("WI00000001", company1Pk);
			var workItemComp2 = CreateWorkItem("WI00000002", company2Pk);
			var workItemNoComp = CreateWorkItem("WI00000003", company3Pk);

			var workItemCmdCount = "SELECT count(*) FROM dbo.WorkItem";
			AssertEquals("[BEFORE PURGE] There should be 3 WI before purge.", 3, TestConnection.ExecuteScalar(workItemCmdCount));

			// Purge company spec
			var purger = new DataPurger { CompanySpecificPk = company1Pk };
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts(isCompanySpecificPurge: true));

			// Postconditions
			var workItemsCounts2 = Convert.ToInt32(TestConnection.ExecuteScalar(workItemCmdCount));
			AssertEquals("[AFTER PURGE] There should be 2 Work Items left.", 2, workItemsCounts2);

			// Purge all WI
			purger = new DataPurger();
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts());

			// Postconditions
			var workItemsCounts3 = Convert.ToInt32(TestConnection.ExecuteScalar(workItemCmdCount));
			AssertEquals("[AFTER PURGE] There should be NO Work Item left", 0, workItemsCounts3);
		}

		public void TestRunPurge_AlsoPurgesWorkRequestsAndRelatedWorkItems()
		{
			var workItemPK = CreateWorkItem("WI00000069", ZGuid.Empty);
			var workRequestPK = CreateWorkRequest("WR00000069", "0118999881999119725...3");

			CreateWorkRequestLink(workItemPK, workRequestPK);

			AssertTableCount(WorkItemSchema.Constants.TableName, 1);
			AssertTableCount(WorkRequestSchema.Constants.TableName, 1);
			AssertTableCount(WorkItemRequestLinkSchema.Constants.TableName, 1);

			var purger = new DataPurger();

			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts());

			AssertTableCount(WorkItemSchema.Constants.TableName, 0);
			AssertTableCount(WorkRequestSchema.Constants.TableName, 0);
			AssertTableCount(WorkItemRequestLinkSchema.Constants.TableName, 0);
		}

		public void TestRunPurge_CompanySpecific_AlsoPurgesWorkRequestsAndRelatedWorkItems()
		{
			var workItemPK = CreateWorkItem("WI00000069", Env.CurrentCompanyPK);
			var workRequestPK = CreateWorkRequest("WR00000069", "0118999881999119725...3");

			CreateWorkRequestLink(workItemPK, workRequestPK);

			AssertTableCount(WorkItemSchema.Constants.TableName, 1);
			AssertTableCount(WorkRequestSchema.Constants.TableName, 1);
			AssertTableCount(WorkItemRequestLinkSchema.Constants.TableName, 1);

			var purger = new DataPurger
			{
				CompanySpecificPk = Env.CurrentCompanyPK,
			};

			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts(isCompanySpecificPurge: true));

			AssertTableCount(WorkItemSchema.Constants.TableName, 0);
			AssertTableCount(WorkRequestSchema.Constants.TableName, 0);
			AssertTableCount(WorkItemRequestLinkSchema.Constants.TableName, 0);
		}

		ZGuid CreateWorkItem(ZString workItemNum, ZGuid companyPk)
		{
			using (DbCommand command = TestConnection.Command(@"INSERT dbo.WorkItem (WKI_PK,	WKI_ActivitySubtype,	WKI_ActivityType, WKI_DateOfChange,	WKI_Details, WKI_GB_AssignedBranch,WKI_GC_AssignedCompany,	WKI_GE_AssignedDepartment,	WKI_PortOrCountry,
								WKI_Priority,	WKI_Risk,	WKI_Status,	WKI_Summary, WKI_SystemCreateTimeUtc,	WKI_SystemCreateUser,	WKI_SystemLastEditTimeUtc,	WKI_SystemLastEditUser,	WKI_WorkItemArea,	WKI_WorkItemNumber,	WKI_WorkItemType)
								VALUES (@workItemPk, @undifinedValue, @undifinedValue, @dateTime , @details, NULL ,@companyPk , @departementPk, 'ADALV', @undifinedValue,'','OPN', 'testDataPurge', @dateTime,'~BP', @dateTime,'E', @undifinedValue, @workItemNum ,@undifinedValue )"))
			{
				var workItemPk = ZGuid.NewZGuid();
				var departementPk = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GlbDepartment");
				var dateTime = DateTime.Today;
				var details = Array.Empty<byte>();
				var undifinedValue = "UDF";

				command.AddParameter("@workItemPk", SqlDbType.UniqueIdentifier, workItemPk.ToGuid());
				command.AddParameter("@workItemNum", SqlDbType.VarChar, workItemNum.ToString());
				command.AddParameter("@departementPk", SqlDbType.UniqueIdentifier, departementPk);
				if (!companyPk.IsEmpty)
				{
					command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk.ToGuid());
				}
				else
				{
					command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.AddParameter("@dateTime", SqlDbType.DateTime, dateTime);
				command.AddParameter("@details", SqlDbType.VarBinary, details);
				command.AddParameter("@undifinedValue", SqlDbType.VarChar, undifinedValue);
				command.ExecuteNonQuery();
				return workItemPk;
			}
		}

		ZGuid CreateWorkRequest(string requestNumber, string summary)
		{
			var personPK = ZGuid.NewZGuid();
			var orgPK = ZGuid.NewZGuid();
			var contactPK = ZGuid.NewZGuid();
			var requestPK = ZGuid.NewZGuid();

			var sql = FormattableString.Invariant($@"
INSERT dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{personPK}', 'Eyehole Man')
INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{orgPK}', 'EYEHOLES')
INSERT dbo.OrgContact (OC_PK, OC_OH, OC_PER) VALUES ('{contactPK}', '{orgPK}', '{personPK}')

INSERT dbo.WorkRequest (WKR_PK, WKR_RequestNumber, WKR_Summary, WKR_OC_Client, WKR_SystemCreateTimeUtc, WKR_SystemCreateUser, WKR_SystemLastEditTimeUtc, WKR_SystemLastEditUser)
VALUES ('{requestPK}', '{requestNumber}', '{summary}', '{contactPK}', GetUtcDate(), 'E', GetUtcDate(), 'E')
");

			TestConnection.ExecuteNonQuery(sql);

			return requestPK;
		}

		void CreateWorkRequestLink(ZGuid workItemPK, ZGuid workRequestPK)
		{
			var sql = FormattableString.Invariant($@"
INSERT dbo.WorkItemRequestLink (WKL_PK, WKL_WKI_WorkItem, WKL_WKR_Request, WKL_SystemCreateTimeUtc, WKL_SystemLastEditTimeUtc, WKL_SystemCreateUser, WKL_SystemLastEditUser)
VALUES (NEWID(), '{workItemPK}', '{workRequestPK}', GetUtcDate(), GetUtcDate(), 'E', 'E')");

			TestConnection.ExecuteNonQuery(sql);
		}

		void AssertTableCount(string tableName, int expectedCount)
		{
			AssertEquals("Row count for table " + tableName, expectedCount, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName));
		}

		#endregion

		#region Purge Accounting

		public void TestPurge_AccountingEInvoicingBatchAndPivot()
		{
			var companyPk = CreateCompany("TC1", "Test Company 1");
			var branchPk = CreateBranch("TB1", "Test Branch 1", companyPk);
			var departmentPk = CreateDepartment("TSD", "Test Department");
			var headerPk = ZGuid.NewZGuid();
			var batchPk = ZGuid.NewZGuid();

			var createSql = $@"
INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES ('{headerPk}', '{companyPk}', '{branchPk}', '{departmentPk}', getdate(), 'AR', 'INV');
INSERT dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_Status, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser, AIB_SystemLastEditTimeUtc, AIB_SystemLastEditUser) VALUES ('{batchPk}', '{companyPk}', 1, 'SNT', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_AIB, AIP_GC, AIP_ParentID, AIP_RN_NKCountryCode, AIP_SystemCreateTimeUtc, AIP_SystemLastEditTimeUtc) VALUES (NEWID(), '{batchPk}', '{companyPk}', '{headerPk}', 'MY', GetUtcDate(), GetUtcDate());
";
			TestConnection.ExecuteNonQuery(createSql);

			var purger = new DataPurger();
			DataPurger.RunScriptCollection(TestConnection, purger.GetPurgeScripts());

			AssertTableCount(AccTransactionHeaderSchema.Constants.TableName, 0);
			AssertTableCount(AccEInvoicingBatchSchema.Constants.TableName, 0);
			AssertTableCount(AccEInvoicingTransactionPivotSchema.Constants.TableName, 0);
		}

		#endregion

		#region Implementation

		#region CreateWarehouse

		ZGuid CreateWarehouse(ZString warehouseCode)
		{
			return CreateWarehouse(warehouseCode, "PRW");
		}

		ZGuid CreateWarehouse(ZString warehouseCode, ZString warehouseType)
		{
			using (DbCommand command = TestConnection.Command(@"
				INSERT dbo.WhsWarehouse (WW_PK, WW_OA_WarehouseAddress, WW_GB_RelatedCompanyBranch, WW_WarehouseCode, WW_WarehouseType, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
				VALUES (@warehousePk, @addressPk, @branchPk, @warehouseCode, @warehouseType, 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				var warehousePk = ZGuid.NewZGuid();
				var branchPk = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
				var addressPk = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 OA_PK FROM dbo.OrgAddress");

				command.AddParameter("@warehousePk", SqlDbType.UniqueIdentifier, warehousePk.ToGuid());
				command.AddParameter("@addressPk", SqlDbType.UniqueIdentifier, addressPk);
				command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPk);
				command.AddParameter("@warehouseCode", SqlDbType.VarChar, WhsWarehouseSchema.WW_WarehouseCode.MaxLength, warehouseCode.ToString());
				command.AddParameter("@warehouseType", SqlDbType.Char, WhsWarehouseSchema.WW_WarehouseType.MaxLength, warehouseType.ToString());

				command.ExecuteNonQuery();
				return warehousePk;
			}
		}

		#endregion

		#region CreateWhsDocket

		ZGuid CreateWhsDocket(ZGuid parentDocketPk, ZGuid clientPk, ZGuid warehousePk, ZString docketType, ZString docketSubType, ZString docketStatus, ZString docketId, ZDateTime finalisedDate, ZGuid pickPk, ZString pickOption)
		{
			using (DbCommand command = TestConnection.Command(@"
				INSERT dbo.WhsDocket (WD_PK, WD_WD_ParentDocket, WD_OH_Client, WD_WW_Whs, WD_DocketType, WD_DocketSubType, WD_DocketStatus, WD_ExternalReference, WD_DocketID, WD_BookingDate, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_UnloadCompletedTime, WD_WP, WD_PickOption, WD_SystemCreateTimeUtc, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser)
				VALUES (@docketPk, @parentDocketPk, @clientPk, @warehousePk, @docketType, @docketSubType, @docketStatus, @docketId, @docketId, '17-OCT-2013', @finalisedDate, @finalizedBy, @unloadCompleteTime, @pickPk, @pickOption, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				var docketPk = ZGuid.NewZGuid();

				command.AddParameter("@docketPk", SqlDbType.UniqueIdentifier, docketPk.ToGuid());
				command.AddParameter("@parentDocketPk", SqlDbType.UniqueIdentifier, parentDocketPk != ZGuid.Empty ? parentDocketPk.ToGuid() : DBNull.Value);
				command.AddParameter("@clientPk", SqlDbType.UniqueIdentifier, clientPk.ToGuid());
				command.AddParameter("@warehousePk", SqlDbType.UniqueIdentifier, warehousePk.ToGuid());
				command.AddParameter("@docketType", SqlDbType.VarChar, WhsDocketSchema.WD_DocketType.MaxLength, docketType.ToString());
				command.AddParameter("@docketSubType", SqlDbType.VarChar, WhsDocketSchema.WD_DocketSubType.MaxLength, docketSubType.ToString());
				command.AddParameter("@docketStatus", SqlDbType.VarChar, WhsDocketSchema.WD_DocketStatus.MaxLength, docketStatus.ToString());
				command.AddParameter("@docketId", SqlDbType.VarChar, WhsDocketSchema.WD_DocketID.MaxLength, docketId.ToString());
				command.AddParameter("@finalisedDate", SqlDbType.SmallDateTime, !finalisedDate.IsEmpty ? finalisedDate.ToDateTime() : DBNull.Value);
				command.AddParameter("@unloadCompleteTime", SqlDbType.SmallDateTime, docketType == "INW" && !finalisedDate.IsEmpty ? finalisedDate.ToDateTime() : DBNull.Value);
				command.AddParameter("@finalizedBy", SqlDbType.VarChar, WhsDocketSchema.WD_GS_NKFinalizedBy.MaxLength, !finalisedDate.IsEmpty ? "~BP" : string.Empty);
				command.AddParameter("@pickPk", SqlDbType.UniqueIdentifier, pickPk != ZGuid.Empty ? pickPk.ToGuid() : DBNull.Value);
				command.AddParameter("@pickOption", SqlDbType.VarChar, WhsDocketSchema.WD_PickOption.MaxLength, pickOption.ToString());

				command.ExecuteNonQuery();
				return docketPk;
			}
		}

		#endregion

		#region CreateJobHeader

		ZGuid CreateJobHeader(ZGuid parentId, ZString parentTableCode, ZString jobNum, ZGuid companyPk, ZGuid branchPk, ZGuid departmentPk)
		{
			return CreateJobHeader(parentId, parentTableCode, jobNum, companyPk, branchPk, departmentPk, Guid.Empty);
		}

		ZGuid CreateJobHeader(ZGuid parentId, ZString parentTableCode, ZString jobNum, ZGuid companyPk, ZGuid branchPk, ZGuid departmentPk, ZGuid parentJobPk)
		{
			using (DbCommand command = TestConnection.Command(@"
				INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_JH_ParentJob, JH_Status)
				VALUES (@jobHeaderPk, @parentId, @parentTableCode, @jobNum, @companyPk, @branchPk, @departmentPk, @parentJobPk, @status)"))
			{
				ZGuid jobHeaderPk = ZGuid.NewZGuid();

				command.AddParameter("@jobHeaderPk", SqlDbType.UniqueIdentifier, jobHeaderPk.ToGuid());
				command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId.ToGuid());
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode.ToString());
				command.AddParameter("@jobNum", SqlDbType.VarChar, jobNum.ToString());
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk.ToGuid());
				command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPk.ToGuid());
				command.AddParameter("@departmentPk", SqlDbType.UniqueIdentifier, departmentPk.ToGuid());
				command.AddParameter("@parentJobPk", SqlDbType.UniqueIdentifier, parentJobPk != Guid.Empty ? parentJobPk.ToGuid() : DBNull.Value);
				command.AddParameter("@status", SqlDbType.VarChar, JobHeaderStatus.Working.Code);

				command.ExecuteNonQuery();
				return jobHeaderPk;
			}
		}

		#endregion

		#region CreateJobStorage

		ZGuid CreateJobStorage(ZString storageJobNumber, ZString storageType, ZDateTime billingDate, ZDateTime storageFromDate, ZDateTime storageToDate, ZGuid clientPk, ZGuid warehousePk)
		{
			using (DbCommand command = TestConnection.Command(@"
				INSERT dbo.JobStorage (ET_PK, ET_StorageJobNumber, ET_StorageType, ET_BillingDate, ET_StorageFromDate, ET_StorageToDate, ET_OH_Client, ET_WW, ET_SystemCreateTimeUtc, ET_SystemCreateUser, ET_SystemLastEditTimeUtc, ET_SystemLastEditUser)
				VALUES (@storagePk, @storageJobNumber, @storageType, @billingDate, @storageFromDate, @storageToDate, @clientPk, @warehousePk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				var storagePk = ZGuid.NewZGuid();

				command.AddParameter("@storagePk", SqlDbType.UniqueIdentifier, storagePk.ToGuid());
				command.AddParameter("@storageJobNumber", SqlDbType.VarChar, JobStorageSchema.ET_StorageJobNumber.MaxLength, storageJobNumber.ToString());
				command.AddParameter("@storageType", SqlDbType.VarChar, JobStorageSchema.ET_StorageType.MaxLength, storageType.ToString());
				command.AddParameter("@billingDate", SqlDbType.SmallDateTime, !billingDate.IsEmpty ? billingDate.ToDateTime() : DBNull.Value);
				command.AddParameter("@storageFromDate", SqlDbType.SmallDateTime, !storageFromDate.IsEmpty ? storageFromDate.ToDateTime() : DBNull.Value);
				command.AddParameter("@storageToDate", SqlDbType.SmallDateTime, !storageToDate.IsEmpty ? storageToDate.ToDateTime() : DBNull.Value);
				command.AddParameter("@clientPk", SqlDbType.UniqueIdentifier, clientPk.ToGuid());
				command.AddParameter("@warehousePk", SqlDbType.UniqueIdentifier, warehousePk != ZGuid.Empty ? warehousePk.ToGuid() : DBNull.Value);

				command.ExecuteNonQuery();
				return storagePk;
			}
		}

		#endregion

		#region GetOrgHeader

		ZGuid GetOrgHeader(params ZGuid[] orgHeadersToIgnore)
		{
			ZGuid result;
			if (orgHeadersToIgnore != null && orgHeadersToIgnore.Length > 0)
			{
				var orgHeaderPksList = String.Join(", ", orgHeadersToIgnore.Select(o => String.Format("'{0}'", o.ToString())));
				result = (Guid)TestConnection.ExecuteScalar(String.Format("SELECT TOP 1 OH_PK FROM dbo.OrgHeader WHERE OH_PK NOT in ({0})", orgHeaderPksList));
			}
			else
			{
				result = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
			}
			return result;
		}

		#endregion

		#region CreateCompany

		ZGuid CreateCompany(ZString code, ZString name)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name)
				VALUES (@pk, @code, @name)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@name", SqlDbType.NVarChar, GlbCompanySchema.GC_Name.MaxLength, name.ToString());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		#endregion

		#region CreateBranch

		ZGuid CreateBranch(ZString code, ZString name, ZGuid companyPk)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_GC)
				VALUES (@pk, @code, @name, @companyPk)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@name", SqlDbType.NVarChar, GlbBranchSchema.GB_BranchName.MaxLength, name.ToString());
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk.ToGuid());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		#endregion

		#region CreateDepartment

		ZGuid CreateDepartment(ZString code, ZString description)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc)
				VALUES (@pk, @code, @description)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, code.ToString());
				command.AddParameter("@description", SqlDbType.VarChar, GlbDepartmentSchema.GE_Desc.MaxLength, description.ToString());

				command.ExecuteNonQuery();
				return pk;
			}
		}

		#endregion

		#region CreateAccGLHeader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		ZGuid CreateAccGLHeader(string accountNum, string accountType, string debitCredit)
		{
			using (var command = TestConnection.Command(@"
				INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_AccountType, AG_DebitCredit)
				VALUES (@pk, @accountNum, @accountType, @debitCredit)"))
			{
				var pk = ZGuid.NewZGuid();

				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk.ToGuid());
				command.AddParameter("@accountNum", SqlDbType.Char, accountNum);
				command.AddParameter("@accountType", SqlDbType.VarChar, accountType);
				command.AddParameter("@debitCredit", SqlDbType.VarChar, debitCredit);

				command.ExecuteNonQuery();
				return pk;
			}
		}

		#endregion

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;

		#endregion
	}
}

