using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UnapprovedTransactionCandidateCollection))]
	public class UnapprovedTransactionCandidateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexUsedWithoutKeyLookups()
		{
			CreateTestData(helper, Factory);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					var newFactory = new BusinessObjectFactory();
					newFactory.SuspendValidation();

					var uapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(newFactory);
					uapprovedTransactionCandidateCollection.Load();

					newFactory.ResumeValidation();

					AssertEquals("Precondition", 60, uapprovedTransactionCandidateCollection.Count);

					var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains($"FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}"));
					var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

					AssertEquals($"{AccTransactionHeaderSchema.Constants.Indexes.NR_UX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_TransactionNum_AH_TransactionCount} must be used in UA.", true, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == AccTransactionHeaderSchema.Constants.Indexes.NR_UX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_TransactionNum_AH_TransactionCount));
					AssertEquals($"{AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_OH_AH_GB_AH_TransactionType} must be used in canceled AR.", true, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_OH_AH_GB_AH_TransactionType));
					AssertEquals($"{AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_OH_AH_GB_AH_TransactionType_AH_FullyPaidDate} must be used in subQuery for AR.", true, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_OH_AH_GB_AH_TransactionType_AH_FullyPaidDate));
					AssertEquals("Should not contain any Lookup", false, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexKind.Contains("Lookup")));
					AssertEquals("Should not contain any TableScan", false, queryPlanAnalyzer.TableScans.Any());
					AssertEquals("Should not contain any RowIDLookups", false, queryPlanAnalyzer.RowIDLookups.Any());
					AssertEquals("Should not contain any IndexScans", false, queryPlanAnalyzer.IndexScans.Any());
				}
			}

			void CreateTestData(UnapprovedTransactionTestHelper unapprovedTransactionTestHelper, BusinessObjectFactory factory)
			{
				unapprovedTransactionTestHelper.SetupSource();
				unapprovedTransactionTestHelper.SetupTarget();

				var sourceProxySourceCompanyData = unapprovedTransactionTestHelper.GetOrCreateOrgCompanyData(unapprovedTransactionTestHelper.SourceProxy, unapprovedTransactionTestHelper.SourceCompany);
				sourceProxySourceCompanyData.OB_IsCreditor = true;
				sourceProxySourceCompanyData.OB_IsDebtor = true;

				var sourceProxyTargetCompanyData = unapprovedTransactionTestHelper.GetOrCreateOrgCompanyData(unapprovedTransactionTestHelper.SourceProxy, unapprovedTransactionTestHelper.TargetCompany);
				sourceProxyTargetCompanyData.OB_IsCreditor = true;
				sourceProxyTargetCompanyData.OB_IsDebtor = true;

				CreateAR_Normal(unapprovedTransactionTestHelper);
				CreateAR_Canceled(unapprovedTransactionTestHelper);
				CreateUA(unapprovedTransactionTestHelper);

				factory.Save();
			}

			void CreateAR_Normal(UnapprovedTransactionTestHelper unapprovedTransactionTestHelper)
			{
				for (int i = 0; i < 20; i++)
				{
					var transactioNum = $"AR_Normal_{i.ToString().PadLeft(5, '0')}";
					var invoice = unapprovedTransactionTestHelper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, transactioNum, ZGuid.Empty, ZString.Empty, 10);
				}

				for (int i = 0; i < 100; i++)
				{
					var transactioNum = $"AR_Normal_OtherGC_{i.ToString().PadLeft(5, '0')}";
					var invoice = unapprovedTransactionTestHelper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, transactioNum, ZGuid.Empty, ZString.Empty, 10);
					invoice.AH_GB = helper.TargetBranch.PK;
					invoice.AH_OH = helper.SourceCompanyBranch.OrgProxy.PK;
					invoice.Lines.Select(line => (AccTransactionLines)line).ForEach(line => line.AL_GB = invoice.AH_GB);
				}
			}

			void CreateAR_Canceled(UnapprovedTransactionTestHelper unapprovedTransactionTestHelper)
			{
				for (int i = 0; i < 20; i++)
				{
					var transactioNum = $"AR_Canceled_{i.ToString().PadLeft(5, '0')}";
					var invoice = unapprovedTransactionTestHelper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, true, transactioNum, ZGuid.Empty, ZString.Empty, 0);

					new InvoicingBaseReversing(invoice).Reverse();
				}

				for (int i = 0; i < 100; i++)
				{
					var transactioNum = $"AR_Canceled_OtherGC_{i.ToString().PadLeft(5, '0')}";
					var invoice = unapprovedTransactionTestHelper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, true, transactioNum, ZGuid.Empty, ZString.Empty, 0);
					invoice.AH_GB = helper.TargetBranch.PK;
					invoice.AH_OH = helper.SourceCompanyBranch.OrgProxy.PK;
					invoice.Lines.Select(line => (AccTransactionLines)line).ForEach(line => line.AL_GB = invoice.AH_GB);

					new InvoicingBaseReversing(invoice).Reverse();
				}
			}

			void CreateUA(UnapprovedTransactionTestHelper unapprovedTransactionTestHelper)
			{
				for (int i = 0; i < 20; i++)
				{
					var transactioNum = $"UA_{i.ToString().PadLeft(5, '0')}";
					unapprovedTransactionTestHelper.CreateInvoice<UAInvoice>(GlbBranch.CurrentBranch, GlbBranch.CurrentBranch.OrgProxy, false, transactioNum, ZGuid.Empty, ZString.Empty, 10);
				}

				for (int i = 0; i < 100; i++)
				{
					var transactioNum = $"UA_OtherGC_{i.ToString().PadLeft(5, '0')}";
					var invoice = unapprovedTransactionTestHelper.CreateInvoice<UAInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, true, transactioNum, ZGuid.Empty, ZString.Empty, 10);
				}
			}
		}

		public void TestCreditNoteAreExcluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			var creditNote = helper.CreateInvoice<ARCreditNote>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 20);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 1, collection.Count);
				AssertCollectionContains(creditNote, collection);

				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 0, collection.Count);
			}
		}

		public void TestBranchOrgProxyIsCreditorForOtherCompanyAreIncluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			var sourceProxyTargetCompanyData = helper.GetOrCreateOrgCompanyData(helper.SourceProxy, helper.TargetCompany);
			sourceProxyTargetCompanyData.OB_IsCreditor = false;

			helper.SourceCompany.GC_OH_OrgProxy = ZGuid.Empty;
			helper.SourceCompanyBranch.GB_OH_OrgProxy = helper.SourceProxy.PK;

			var invoice = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 10);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 0, collection.Count);
			}

			sourceProxyTargetCompanyData.OB_IsCreditor = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 1, collection.Count);
				AssertCollectionContains(invoice, collection);
			}
		}

		public void TestCompanyOrgProxyIsCreditorForOtherCompanyAreIncluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			var sourceProxyTargetCompanyData = helper.GetOrCreateOrgCompanyData(helper.SourceProxy, helper.TargetCompany);
			sourceProxyTargetCompanyData.OB_IsCreditor = false;

			helper.SourceCompany.GC_OH_OrgProxy = helper.SourceProxy.PK;
			helper.SourceCompanyBranch.GB_OH_OrgProxy = ZGuid.Empty;

			var invoice = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 10);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 0, collection.Count);
			}

			sourceProxyTargetCompanyData.OB_IsCreditor = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 1, collection.Count);
				AssertCollectionContains(invoice, collection);
			}
		}

		public void TestFilterMultipleRecordsFromCompanyAndBranchProxies_IsOrgActive()
		{
			helper.SetupTarget();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var proxyActive = TestObjectCreator.CreateOrgHeader("ORGY", true, true);
				proxyActive.OH_IsActive = true;
				var proxyInactive = TestObjectCreator.CreateOrgHeader("ORGN", true, true);
				proxyInactive.OH_IsActive = false;

				var companyOrgActive = TestObjectCreator.CreateNewCompany("CpY");
				companyOrgActive.GC_OH_OrgProxy = proxyActive.PK;
				var branchOrgActive_SourceY = TestObjectCreator.CreateNewBranch(companyOrgActive, "CYY");
				branchOrgActive_SourceY.GB_OH_OrgProxy = proxyActive.PK;
				var branchOrgInactive_SourceY = TestObjectCreator.CreateNewBranch(companyOrgActive, "CYN");
				branchOrgInactive_SourceY.GB_OH_OrgProxy = proxyInactive.PK;

				var companyOrgInactive = TestObjectCreator.CreateNewCompany("CpN");
				companyOrgInactive.GC_OH_OrgProxy = proxyInactive.PK;
				var branchOrgActive_SourceN = TestObjectCreator.CreateNewBranch(companyOrgInactive, "CNY");
				branchOrgActive_SourceN.GB_OH_OrgProxy = proxyActive.PK;
				var branchOrgInactive_SourceN = TestObjectCreator.CreateNewBranch(companyOrgInactive, "CNN");
				branchOrgInactive_SourceN.GB_OH_OrgProxy = proxyInactive.PK;

				Factory.Save();

				var candidateAR = new List<ARInvoice>();
				candidateAR.Add(helper.CreateInvoice<ARInvoice>(branchOrgActive_SourceY, helper.TargetCompany.OrgProxy, false, "1", ZGuid.Empty, ZString.Empty, 1));
				candidateAR.Add(helper.CreateInvoice<ARInvoice>(branchOrgInactive_SourceY, helper.TargetCompany.OrgProxy, false, "1", ZGuid.Empty, ZString.Empty, 1));
				candidateAR.Add(helper.CreateInvoice<ARInvoice>(branchOrgActive_SourceN, helper.TargetCompany.OrgProxy, false, "1", ZGuid.Empty, ZString.Empty, 1));
				helper.CreateInvoice<ARInvoice>(branchOrgInactive_SourceN, helper.TargetCompany.OrgProxy, false, "1", ZGuid.Empty, ZString.Empty, 1);

				Factory.Save();

				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", candidateAR.Count, collection.Count);
				CombineAssertions(() =>
					candidateAR.ForEach(ar => collection.Contains(ar.PK))
				);
			}
		}

		public void TestIsCreditorOrganazationExcludeWhenInactive()
		{
			helper.SetupSource();
			helper.SetupTarget();

			var sourceProxyTargetCompanyData = helper.GetOrCreateOrgCompanyData(helper.SourceProxy, helper.TargetCompany);
			sourceProxyTargetCompanyData.OB_IsCreditor = true;

			helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 10);

			TestCompanyOrgProxy(true);
			TestBranchOrgProxy(true);

			TestCompanyOrgProxy(false);
			TestBranchOrgProxy(false);

			void TestCompanyOrgProxy(bool isOrgActive)
			{
				helper.SourceProxy.OH_IsActive = isOrgActive;
				helper.SourceCompany.GC_OH_OrgProxy = helper.SourceProxy.PK;
				helper.SourceCompanyBranch.GB_OH_OrgProxy = ZGuid.Empty;

				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var collection = new UnapprovedTransactionCandidateCollection(Factory);
					collection.Load();

					AssertEquals("collection.Count", isOrgActive ? 1 : 0, collection.Count);
				}
			}

			void TestBranchOrgProxy(bool isOrgActive)
			{
				helper.SourceProxy.OH_IsActive = isOrgActive;
				helper.SourceCompany.GC_OH_OrgProxy = ZGuid.Empty;
				helper.SourceCompanyBranch.GB_OH_OrgProxy = helper.SourceProxy.PK;

				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var collection = new UnapprovedTransactionCandidateCollection(Factory);
					collection.Load();

					AssertEquals("collection.Count", isOrgActive ? 1 : 0, collection.Count);
				}
			}
		}

		public void TestDebtorFromOtherCompanyOrgProxyAreExcluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			helper.SourceCompany.GC_OH_OrgProxy = helper.SourceDebtor.PK;
			helper.SourceCompanyBranch.GB_OH_OrgProxy = ZGuid.Empty;
			helper.TargetCompany.GC_OH_OrgProxy = helper.SourceDebtor2.PK;
			helper.TargetBranch.GB_OH_OrgProxy = ZGuid.Empty;

			ARInvoice invoiceToExclude = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 10);
			ARInvoice invoiceToInclude = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor2, false, "02", ZGuid.Empty, ZString.Empty, 20);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 1, collection.Count);
				AssertCollectionContains(invoiceToInclude, collection);
			}
		}

		public void TestDebtorFromOtherCompanyBranchOrgProxyAreExcluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			helper.SourceCompany.GC_OH_OrgProxy = ZGuid.Empty;
			helper.SourceCompanyBranch.GB_OH_OrgProxy = helper.SourceDebtor.PK;
			helper.TargetCompany.GC_OH_OrgProxy = ZGuid.Empty;
			helper.TargetBranch.GB_OH_OrgProxy = helper.SourceDebtor2.PK;

			ARInvoice invoiceToExclude = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "01", ZGuid.Empty, ZString.Empty, 10);
			ARInvoice invoiceToInclude = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor2, false, "02", ZGuid.Empty, ZString.Empty, 20);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("collection.Count", 1, collection.Count);
				AssertCollectionContains(invoiceToInclude, collection);
			}
		}

		public void TestPaidTransactionsAreExcluded()
		{
			helper.SetupSource();
			helper.SetupTarget();

			ARInvoice unpaidInvoice = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "13", ZGuid.Empty, ZString.Empty, 10);
			InvoicingLineBase line = (InvoicingLineBase)unpaidInvoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_OSExTaxAmount = 10m;
			line.AL_GB = unpaidInvoice.Company.Branches[0].PK;
			ARInvoice paidInvoice = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "13", ZGuid.Empty, ZString.Empty, 0);

			Factory.Save();

			GlbBranch originalBranch = GlbBranch.CurrentBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				UnapprovedTransactionCandidateCollection collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();

				AssertEquals("There should be 1 item in the collection", 1, collection.Count);
				AssertCollectionContains(unpaidInvoice, collection);
			}
		}

		public void TestAdditionalFiltersWork()
		{
			helper.SetupSource();
			helper.SetupTarget();

			var unpaidInvoice1 = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "100", ZGuid.Empty, ZString.Empty, 10);
			var unpaidInvoice2 = helper.CreateInvoice<ARInvoice>(helper.SourceCompanyBranch, helper.SourceDebtor, false, "101", ZGuid.Empty, ZString.Empty, 30);
			Factory.Save();

			GlbBranch originalBranch = GlbBranch.CurrentBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, helper.TargetBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Load();
				AssertEquals("There should be 2 items in the collection when no AdditionalFilters are set.", 2, collection.Count);

				var additionalFilter = new ZQuery();
				additionalFilter.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceAmount, unpaidInvoice1.AH_InvoiceAmount);
				collection = new UnapprovedTransactionCandidateCollection(Factory, additionalFilter);
				collection.Load();
				AssertEquals("There should be 1 item in the collection when filtered by AdditionalFilters.", 1, collection.Count);
				AssertCollectionContains("Unpaid invoice for $10 should be in the collection.", unpaidInvoice1, collection);

				additionalFilter = new ZQuery();
				additionalFilter.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceAmount, unpaidInvoice2.AH_InvoiceAmount);
				collection = new UnapprovedTransactionCandidateCollection(Factory, additionalFilter);
				collection.Load();
				AssertEquals("There should be 1 item in the collection when filtered by AdditionalFilters.", 1, collection.Count);
				AssertCollectionContains("Unpaid invoice for $30 should be in the collection.", unpaidInvoice2, collection);
			}
		}

		[StressTest]
		public void TestCurrentCompanyCanHaveThousandsOfCreditorsWithoutSqlException_WI00250823()
		{
			var factory = new BusinessObjectFactory();
			const int numberOfCreditorOrgs = 20_000;
			for (int i = 0; i < numberOfCreditorOrgs; i++)
			{
				// BusinessObjectFactory inserts OrgHeaders ~50/sec. Using raw SQL is ~2000/sec
				var pK = Guid.NewGuid();
				string orgHeaderSql = $"INSERT INTO {OrgHeaderSchema.Constants.SqlSchemaName}.{OrgHeaderSchema.Constants.TableName} (OH_PK, OH_IsValid, OH_Code, OH_IsActive, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@OrgHeaderPK, 0, @Code, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'); INSERT INTO {OrgCompanyDataSchema.Constants.SqlSchemaName}.{OrgCompanyDataSchema.Constants.TableName} (OB_PK, OB_IsValid, OB_IsDebtor, OB_IsCreditor, OB_GC, OB_OH, OB_SystemCreateTimeUtc, OB_SystemCreateUser, OB_SystemLastEditTimeUtc, OB_SystemLastEditUser) VALUES (NEWID(), 0, 0, 1, @CurrentCompany, @OrgHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				var cmd = Db.Connection.Command(orgHeaderSql);
				cmd.AddParameter("@OrgHeaderPK", SqlDbType.UniqueIdentifier, pK);
				cmd.AddParameter("@Code", SqlDbType.Char, "Z" + i.ToString("00000"));
				cmd.AddParameter("@CurrentCompany", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			var collection = new UnapprovedTransactionCandidateCollection(Factory);
			AssertNoExceptionThrown("Collection should not throw any exception when loading large number of creditor orgs.", () => collection.Load());
			AssertEquals("Collection should not find any results.", 0, collection.Count);
		}

		public void TestInvoicePostAndConvert()
		{
			TestSingleTransactionPostAndConvert(PostType.Invoice);
		}

		public void TestCreditNotePostAndConvert()
		{
			TestSingleTransactionPostAndConvert(PostType.CreditNote);
		}

		public void TestInvoicePostAndReverse()
		{
			TestSingleTransactionWithReversal(PostType.Invoice);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCreditNotePostAndReverse()
		{
			TestSingleTransactionWithReversal(PostType.CreditNote);
		}

		public void TestImportCreditNoteWhenInvoiceHasBeenImported()
		{
			GlbCompany companyDestination = TestObjectCreator.CreateNewCompany("D");
			GlbCompany companySource = TestObjectCreator.CreateNewCompany("S");
			GlbBranch branchD = TestObjectCreator.CreateNewBranch(companyDestination, "D");
			GlbBranch branchS = TestObjectCreator.CreateNewBranch(companySource, "S");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				OrgHeader proxyD = TestObjectCreator.CreateOrgHeader("BD", true, true);
				OrgHeader proxyS = TestObjectCreator.CreateOrgHeader("BS", true, true);
				branchD.GB_OH_OrgProxy = proxyD.PK;
				branchS.GB_OH_OrgProxy = proxyS.PK;
				Factory.Save();

				ZGuid transactionGroup1 = ZGuid.NewZGuid();
				ZGuid transactionGroup2 = ZGuid.NewZGuid();

				ARInvoice invoice101 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, true, "101", ZGuid.Empty, "S00001011", 95);
				ARInvoice invoice102 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "102", ZGuid.Empty, @"S00001011\A", 10);
				ARInvoice invoice103 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "103", ZGuid.Empty, @"S00001011\B", 30);
				ARInvoice invoice104 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, true, "105", ZGuid.Empty, "S00001012", 100);

				InvoicingBaseReversing reverserForInvoice103 = new InvoicingBaseReversing(invoice103);
				reverserForInvoice103.Reverse();

				InvoicingBaseReversing reverserForInvoice104 = new InvoicingBaseReversing(invoice104);
				reverserForInvoice104.Reverse();

				ARCreditNote creditNote105 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, true, "114", ZGuid.Empty, "S00001015", -95);
				ARCreditNote creditNote106 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, false, "115", ZGuid.Empty, "S00001016", -10);
				ARCreditNote creditNote107 = (ARCreditNote)reverserForInvoice103.ReverseTransaction;
				ARCreditNote creditNote108 = (ARCreditNote)reverserForInvoice104.ReverseTransaction;

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(newFactory);
				headers.Load("SELECT COUNT(*) AS Count FROM dbo.AccTransactionHeader");
				AssertEquals("Precondition: transactions must exist", 8, (ZInt)headers[0]["Count"]);

				UnapprovedTransactionCandidateCollection unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(newFactory);
				unapprovedTransactionCandidateCollection.Load();

				AssertEquals("Wrong number of items in the collection", 3, unapprovedTransactionCandidateCollection.Count);

				Assert("invoice101 should NOT be included", !unapprovedTransactionCandidateCollection.Contains(invoice101));
				Assert("invoice102 should be included", unapprovedTransactionCandidateCollection.Contains(invoice102));
				Assert("invoice103 should NOT be included", !unapprovedTransactionCandidateCollection.Contains(invoice103));
				Assert("invoice104 should NOT be included", !unapprovedTransactionCandidateCollection.Contains(invoice104));

				Assert("creditNote105 should NOT be included", !unapprovedTransactionCandidateCollection.Contains(creditNote105));
				Assert("creditNote106 should be included", unapprovedTransactionCandidateCollection.Contains(creditNote106));
				Assert("creditNote107 should NOT be included", !unapprovedTransactionCandidateCollection.Contains(creditNote107));
				Assert("creditNote108 should be included", unapprovedTransactionCandidateCollection.Contains(creditNote108));
			}
		}

		public void TestFilteringForOrgProxies()
		{
			//Variable name meaning:
			//D:Destination , S : Source , (1,2,3):company serial. S2 => Source,use company 2
			// (B,C) : no meaning.  proxyBS1 => proxyB,for S1(Source 1)
			// (y,n): is branch have org proxy. "branchS3y => branch for S3 , have org proxy" | "branchS3n => branch for S3 , no org proxy"
			// P : is linked org proxy IsPayables(OH_IsCreditor). branchS3yP => branch for S3 , have org proxy with IsPayables
			GlbCompany companySource2 = TestObjectCreator.CreateNewCompany("S2");
			GlbCompany companySource3 = TestObjectCreator.CreateNewCompany("S3");

			GlbBranch branchD = TestObjectCreator.CreateNewBranch(helper.TargetCompany, "Dy");

			GlbBranch branchS1yP = TestObjectCreator.CreateNewBranch(helper.SourceCompany, "S1P");
			GlbBranch branchS1y = TestObjectCreator.CreateNewBranch(helper.SourceCompany, "S1y");
			GlbBranch branchS1n = TestObjectCreator.CreateNewBranch(helper.SourceCompany, "S1n");

			GlbBranch branchS2yP = TestObjectCreator.CreateNewBranch(companySource2, "S2P");
			GlbBranch branchS2y = TestObjectCreator.CreateNewBranch(companySource2, "S2y");
			GlbBranch branchS2n = TestObjectCreator.CreateNewBranch(companySource2, "S2n");

			GlbBranch branchS3yP = TestObjectCreator.CreateNewBranch(companySource3, "S3P");
			GlbBranch branchS3y = TestObjectCreator.CreateNewBranch(companySource3, "S3y");
			GlbBranch branchS3n = TestObjectCreator.CreateNewBranch(companySource3, "S3n");

			OrgHeader proxyD = TestObjectCreator.CreateOrgHeader("BD", true, true);
			OrgHeader proxyBS1P = TestObjectCreator.CreateOrgHeader("BS1P", true, true);
			OrgHeader proxyBS1 = TestObjectCreator.CreateOrgHeader("BS1", false, true);
			OrgHeader proxyCS2P = TestObjectCreator.CreateOrgHeader("CS2P", true, true);
			OrgHeader proxyBS2P = TestObjectCreator.CreateOrgHeader("BS2P", true, true);
			OrgHeader proxyBS2 = TestObjectCreator.CreateOrgHeader("BS2", false, true);
			OrgHeader proxyCS3 = TestObjectCreator.CreateOrgHeader("CS3", false, true);
			OrgHeader proxyBS3P = TestObjectCreator.CreateOrgHeader("BS3P", true, true);
			OrgHeader proxyBS3 = TestObjectCreator.CreateOrgHeader("BS3", false, true);

			branchS1yP.GB_OH_OrgProxy = proxyBS1P.PK;
			branchS1y.GB_OH_OrgProxy = proxyBS1.PK;

			companySource2.GC_OH_OrgProxy = proxyCS2P.PK;
			branchS2yP.GB_OH_OrgProxy = proxyBS2P.PK;
			branchS2y.GB_OH_OrgProxy = proxyBS2.PK;

			companySource3.GC_OH_OrgProxy = proxyCS3.PK;
			branchS3yP.GB_OH_OrgProxy = proxyBS3P.PK;
			branchS3y.GB_OH_OrgProxy = proxyBS3.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				List<TransactionHeader> validARTransactions = new List<TransactionHeader>();

				helper.CreateInvoice<APInvoice>(branchS1yP, proxyD, false, "1", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS1y, proxyD, false, "2", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS1n, proxyD, false, "3", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2yP, proxyD, false, "4", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2y, proxyD, false, "5", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2n, proxyD, false, "6", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3yP, proxyD, false, "7", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3y, proxyD, false, "8", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3n, proxyD, false, "9", ZGuid.Empty, ZString.Empty, 1);

				validARTransactions.Add(helper.CreateInvoice<ARInvoice>(branchS1yP, proxyD, false, "10", ZGuid.Empty, ZString.Empty, 1));
				helper.CreateInvoice<ARInvoice>(branchS1y, proxyD, false, "11", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS1n, proxyD, false, "12", ZGuid.Empty, ZString.Empty, 1);
				validARTransactions.Add(helper.CreateInvoice<ARInvoice>(branchS2yP, proxyD, false, "13", ZGuid.Empty, ZString.Empty, 1));
				validARTransactions.Add(helper.CreateInvoice<ARInvoice>(branchS2y, proxyD, false, "14", ZGuid.Empty, ZString.Empty, 1));
				validARTransactions.Add(helper.CreateInvoice<ARInvoice>(branchS2n, proxyD, false, "15", ZGuid.Empty, ZString.Empty, 1));
				validARTransactions.Add(helper.CreateInvoice<ARInvoice>(branchS3yP, proxyD, false, "16", ZGuid.Empty, ZString.Empty, 1));
				helper.CreateInvoice<ARInvoice>(branchS3y, proxyD, false, "17", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS3n, proxyD, false, "18", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<APInvoice>(branchS1yP, proxyD, true, "19", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS1y, proxyD, true, "20", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS1n, proxyD, true, "21", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2yP, proxyD, true, "22", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2y, proxyD, true, "23", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS2n, proxyD, true, "24", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3yP, proxyD, true, "25", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3y, proxyD, true, "26", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(branchS3n, proxyD, true, "27", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<ARInvoice>(branchS1yP, proxyD, true, "28", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS1y, proxyD, true, "29", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS1n, proxyD, true, "30", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS2yP, proxyD, true, "31", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS2y, proxyD, true, "32", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS2n, proxyD, true, "33", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS3yP, proxyD, true, "34", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS3y, proxyD, true, "35", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(branchS3n, proxyD, true, "36", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, true, "37", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, true, "38", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, false, "39", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<APInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, false, "40", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<ARPayment>(branchS2yP, proxyD, false, "201", ZGuid.Empty, ZString.Empty, 1);
				validARTransactions.Add(helper.CreateInvoice<ARCreditNote>(branchS2yP, proxyD, false, "202", ZGuid.Empty, ZString.Empty, 1));
				helper.CreateInvoice<ARAdjustmentNote>(branchS2yP, proxyD, false, "203", ZGuid.Empty, ZString.Empty, 1);
				validARTransactions.Add(helper.CreateInvoice<ARCreditNote>(branchS2yP, proxyD, false, "204", ZGuid.Empty, ZString.Empty, 1));
				helper.CreateInvoice<ARReceipt>(branchS2yP, proxyD, false, "205", ZGuid.Empty, ZString.Empty, 1);

				List<UAInvoice> validUAInvoices = new List<UAInvoice>();

				helper.CreateInvoice<UAInvoice>(branchS1yP, proxyD, false, "110", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS1y, proxyD, false, "111", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS1n, proxyD, false, "112", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2yP, proxyD, false, "113", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2y, proxyD, false, "114", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2n, proxyD, false, "115", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3yP, proxyD, false, "116", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3y, proxyD, false, "117", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3n, proxyD, false, "118", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<UAInvoice>(branchS1yP, proxyD, true, "128", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS1y, proxyD, true, "129", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS1n, proxyD, true, "130", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2yP, proxyD, true, "31", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2y, proxyD, true, "132", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS2n, proxyD, true, "133", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3yP, proxyD, true, "134", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3y, proxyD, true, "135", ZGuid.Empty, ZString.Empty, 1);
				helper.CreateInvoice<UAInvoice>(branchS3n, proxyD, true, "136", ZGuid.Empty, ZString.Empty, 1);

				helper.CreateInvoice<UAInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, true, "137", ZGuid.Empty, ZString.Empty, 1);
				validUAInvoices.Add(helper.CreateInvoice<UAInvoice>(GlbBranch.CurrentBranch, TestObjectCreator.AALSHI, false, "139", ZGuid.Empty, ZString.Empty, 1));

				Factory.Save();

				ZQuery tranQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				tranQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Invoice);
				tranQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				AssertEquals("Precondition: AP invoices must exist", 20, Factory.Load<APInvoice>(tranQuery).Length);

				tranQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				tranQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				AssertEquals("Precondition: transactions must exist", 25, Factory.Load<TransactionHeader>(tranQuery).Length);

				tranQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions);
				tranQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.UAInvoice);
				tranQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				AssertEquals("Precondition: UA invoices must exist", 20, Factory.Load<UAInvoice>(tranQuery).Length);

				var unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();

				AssertEquals("There should be 1 item in the collection", 1, unapprovedTransactionCandidateCollection.Count);
				foreach (UAInvoice invoice in validUAInvoices)
				{
					Assert(string.Format("UA Invoice no. {0} must be in the collection", invoice.AH_TransactionNum), unapprovedTransactionCandidateCollection.Contains(invoice));
				}

				// Adding org proxy to destination company's branch
				branchD.GB_OH_OrgProxy = proxyD.PK;
				Factory.Save();

				unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();

				AssertEquals("Count of items in the collection because Debtor is Login Branch Org Proxy", validARTransactions.Count + validUAInvoices.Count, unapprovedTransactionCandidateCollection.Count);
				foreach (TransactionHeader transaction in validARTransactions)
				{
					Assert(string.Format("Transaction no. {0} must be in the collection, for its Debtor is Login Branch Org Proxy", transaction.AH_TransactionNum), unapprovedTransactionCandidateCollection.Contains(transaction));
				}
				foreach (UAInvoice invoice in validUAInvoices)
				{
					Assert(string.Format("UA Invoice no. {0} must be in the collection", invoice.AH_TransactionNum), unapprovedTransactionCandidateCollection.Contains(invoice));
				}

				helper.TargetCompany.GC_OH_OrgProxy = branchD.GB_OH_OrgProxy;
				branchD.GB_OH_OrgProxy = ZGuid.Empty;
				Factory.Save();

				unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();

				AssertEquals("Count of items in the collection because Debtor is Login Company Org Proxy", validARTransactions.Count + validUAInvoices.Count, unapprovedTransactionCandidateCollection.Count);
				foreach (TransactionHeader transaction in validARTransactions)
				{
					Assert(string.Format("Transaction no. {0} must be in the collection, for its Debtor is Login Company Org Proxy", transaction.AH_TransactionNum), unapprovedTransactionCandidateCollection.Contains(transaction));
				}
				foreach (UAInvoice invoice in validUAInvoices)
				{
					Assert(string.Format("UA Invoice no. {0} must be in the collection, for its Debtor is Login Company Org Proxy", invoice.AH_TransactionNum), unapprovedTransactionCandidateCollection.Contains(invoice));
				}
			}
		}

		public void TestValidation()
		{
			GlbCompany companyDestination = TestObjectCreator.CreateNewCompany("D");
			GlbCompany companySource = TestObjectCreator.CreateNewCompany("S");
			GlbBranch branchD = TestObjectCreator.CreateNewBranch(companyDestination, "D");
			GlbBranch branchS = TestObjectCreator.CreateNewBranch(companySource, "S");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				OrgHeader proxyD = TestObjectCreator.CreateOrgHeader("BD", true, true);
				proxyD.GetCompanyDataForGlbCompany(companyDestination).OB_IsDebtor = true;
				proxyD.GetCompanyDataForGlbCompany(companyDestination).OB_IsCreditor = true;
				proxyD.GetCompanyDataForGlbCompany(companySource).OB_IsDebtor = true;
				proxyD.GetCompanyDataForGlbCompany(companySource).OB_IsCreditor = true;
				OrgHeader proxyS = TestObjectCreator.CreateOrgHeader("BS", true, true);
				proxyS.GetCompanyDataForGlbCompany(companyDestination).OB_IsDebtor = true;
				proxyS.GetCompanyDataForGlbCompany(companyDestination).OB_IsCreditor = true;
				proxyS.GetCompanyDataForGlbCompany(companySource).OB_IsDebtor = true;
				proxyS.GetCompanyDataForGlbCompany(companySource).OB_IsCreditor = true;
				branchD.GB_OH_OrgProxy = proxyD.PK;
				branchS.GB_OH_OrgProxy = proxyS.PK;
				Factory.Save();

				ZGuid transactionGroup1 = ZGuid.NewZGuid();
				ZGuid transactionGroup2 = ZGuid.NewZGuid();

				ARInvoice invoice101 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
				ARInvoice invoice102 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "102", ZGuid.Empty, @"S00001011\A", 10, isManuallySetTransactionNumber: true);
				ARInvoice invoice103 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "103", ZGuid.Empty, @"S00001011\B", 30, isManuallySetTransactionNumber: true);
				ARInvoice invoice108 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, true, "105", ZGuid.Empty, "S00001012", 100);
				InvoicingBaseReversing reverser = new InvoicingBaseReversing(invoice108);
				reverser.Reverse();
				ARCreditNote creditNote109 = (ARCreditNote)reverser.ReverseTransaction;

				ARInvoice invoice112 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "112", ZGuid.Empty, "S00001014", 90);
				reverser = new InvoicingBaseReversing(invoice112);
				reverser.Reverse();
				ARCreditNote creditNote113 = (ARCreditNote)reverser.ReverseTransaction;

				ARCreditNote creditNote114 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, true, "114", ZGuid.Empty, "S00001015", -20);
				ARCreditNote creditNote115 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, false, "115", ZGuid.Empty, "S00001016", -30, isManuallySetTransactionNumber: true);
				ARCreditNote creditNote116 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, false, "116", ZGuid.Empty, "S00001017", -40);
				Factory.Save();

				APInvoice invoice102D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "102", ZGuid.Empty, ZString.Empty, 10);
				APInvoice invoice103D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "103", ZGuid.Empty, "103", 30);
				APInvoice invoice201D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "201", ZGuid.Empty, "S00001012", 100);
				APInvoice invoice204D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "204", ZGuid.Empty, "109", 90);
				APCreditNote creditNote205D = helper.CreateInvoice<APCreditNote>(branchD, proxyS, false, "205", ZGuid.Empty, "S00001015", -20);
				APCreditNote creditNote206D = helper.CreateInvoice<APCreditNote>(branchD, proxyS, false, "115", ZGuid.Empty, "S00001016", -30);

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(newFactory);
				headers.Load("SELECT COUNT(*) AS Count FROM dbo.AccTransactionHeader");
				AssertEquals("Precondition: transactions must exist", 16, (ZInt)headers[0]["Count"]);

				UnapprovedTransactionCandidateCollection unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();

				AssertEquals("Wrong number of items in the collection", 6, unapprovedTransactionCandidateCollection.Count);

				Assert("invoice101 should be included", unapprovedTransactionCandidateCollection.Contains(invoice101));
				invoice101.MarkAsNeedingValidation();
				invoice101.Validation.ValidateAH_OH();
				Assert("invoice101 shouldn't have any warnings", !invoice101.HasWarnings);

				Assert("invoice102 should be included", unapprovedTransactionCandidateCollection.Contains(invoice102));
				Assert("invoice102 should have 1 warning", invoice102.HasWarnings);
				AssertEquals("invoice102 should have 1 warning", "Warning - Accounts Receivable Invoice: AP Invoice with the same Transaction Number already exists", invoice102.Notifications.GetWarnings().GetFirstMessage());

				Assert("invoice103 should be included", unapprovedTransactionCandidateCollection.Contains(invoice103));
				Assert("invoice103 should have 1 warning", invoice103.HasWarnings);
				AssertEquals("invoice103 should have 1 warning", "Warning - Accounts Receivable Invoice: AP Invoice with the same Transaction Number already exists", invoice103.Notifications.GetWarnings().GetFirstMessage());

				Assert("Reversing creditNote109 should be included", unapprovedTransactionCandidateCollection.Contains(creditNote109));
				Assert("creditNote109 shouldn't have any warnings", !creditNote109.HasWarnings);

				Assert("invoice112 should not be included", !unapprovedTransactionCandidateCollection.Contains(invoice112));
				Assert("creditNote113 should not be included", !unapprovedTransactionCandidateCollection.Contains(creditNote113));

				Assert("creditNote115 should be included", unapprovedTransactionCandidateCollection.Contains(creditNote115));
				Assert("creditNote115 should have 1 warning", creditNote115.HasWarnings);
				AssertEquals("creditNote115 should have 1 warning", "Warning - Accounts Receivable Credit Note: AP Credit Note with the same Transaction Number already exists", creditNote115.Notifications.GetWarnings().GetFirstMessage());

				Assert("creditNote116 should be included", unapprovedTransactionCandidateCollection.Contains(creditNote116));
				creditNote116.MarkAsNeedingValidation();
				creditNote116.Validation.ValidateAH_OH();
				Assert("creditNote116 shouldn't have any warnings", !creditNote116.HasWarnings);

				const string invoiceNumPrefix = "SZZ";
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(companySource.PK.ToGuid(), Guid.Empty, Guid.Empty, invoiceNumPrefix);
				invoice102D.AH_TransactionNum = invoiceNumPrefix + invoice102D.AH_TransactionNum;
				Factory.Save();
				invoice102.ClearAllNotifications();
				Assert("Precondition: invoice102 should have no warnings", !invoice102.HasWarnings);
				unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();
				Assert("invoice102 should be included", unapprovedTransactionCandidateCollection.Contains(invoice102));
				Assert("invoice102 should have 1 warning", invoice102.HasWarnings);
				AssertEquals("invoice102 should have 1 warning", "Warning - Accounts Receivable Invoice: AP Invoice with the same Transaction Number already exists", invoice102.Notifications.GetWarnings().GetFirstMessage());
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Inner.DeleteValue(companySource.PK.ToGuid(), Guid.Empty, Guid.Empty);

				branchD.GB_OH_OrgProxy = Guid.Empty;
				Factory.Save();
				unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();
				AssertEquals("There should no invoices in the collection because the company's branch org proxy was removed", 0, unapprovedTransactionCandidateCollection.Count);

				branchD.GB_OH_OrgProxy = proxyD.PK;
				Factory.Save();
				Assert("Precondition: invoice103 should have no warnings", !invoice103.HasWarnings);
				unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.Load();
				Assert("invoice103 should be included", unapprovedTransactionCandidateCollection.Contains(invoice103));
				Assert("invoice103 should have 1 warning because there is now a company org proxy for the sister company", invoice103.HasWarnings);
				AssertEquals("invoice103 should have 1 warning", "Warning - Accounts Receivable Invoice: AP Invoice with the same Transaction Number already exists", invoice103.Notifications.GetWarnings().GetFirstMessage());
			}
		}

		[ExpectNoExceptions()]
		public void TestValidationIsSuspended()
		{
			GlbCompany companyDestination = TestObjectCreator.CreateNewCompany("D");
			GlbCompany companySource = TestObjectCreator.CreateNewCompany("S");
			GlbBranch branchD = TestObjectCreator.CreateNewBranch(companyDestination, "D");
			GlbBranch branchS = TestObjectCreator.CreateNewBranch(companySource, "S");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				OrgHeader proxyD = TestObjectCreator.CreateOrgHeader("BD", true, true);
				OrgHeader proxyS = TestObjectCreator.CreateOrgHeader("BS", true, true);
				branchD.GB_OH_OrgProxy = proxyD.PK;
				branchS.GB_OH_OrgProxy = proxyS.PK;
				Factory.Save();

				ZGuid transactionGroup1 = ZGuid.NewZGuid();
				ZGuid transactionGroup2 = ZGuid.NewZGuid();

				APInvoice invoice201D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "201", ZGuid.Empty, "S00001012", 100);
				APInvoice invoice202D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "202", ZGuid.Empty, @"S00001011\A", 10);
				APInvoice invoice203D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "203", ZGuid.Empty, "103", 30);
				APInvoice invoice204D = helper.CreateInvoice<APInvoice>(branchD, proxyS, false, "204", ZGuid.Empty, "109", 90);
				APCreditNote creditNote205D = helper.CreateInvoice<APCreditNote>(branchD, proxyS, false, "205", ZGuid.Empty, "S00001015", -20);
				APCreditNote creditNote206D = helper.CreateInvoice<APCreditNote>(branchD, proxyS, false, "206", ZGuid.Empty, "S00001016", -30);

				ARInvoice invoice101 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
				ARInvoice invoice102 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "102", ZGuid.Empty, @"S00001011\A", 10);
				ARInvoice invoice103 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "103", ZGuid.Empty, @"S00001011\B", 30);
				ARInvoice invoice108 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, true, "105", ZGuid.Empty, "S00001012", 100);
				InvoicingBaseReversing reverser = new InvoicingBaseReversing(invoice108);
				reverser.Reverse();
				ARCreditNote creditNote109 = (ARCreditNote)reverser.ReverseTransaction;

				ARInvoice invoice112 = helper.CreateInvoice<ARInvoice>(branchS, proxyD, false, "112", ZGuid.Empty, "S00001014", 90);
				reverser = new InvoicingBaseReversing(invoice112);
				reverser.Reverse();
				ARCreditNote creditNote113 = (ARCreditNote)reverser.ReverseTransaction;

				ARCreditNote creditNote114 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, true, "114", ZGuid.Empty, "S00001015", -20);
				ARCreditNote creditNote115 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, false, "115", ZGuid.Empty, "S00001016", -30);
				ARCreditNote creditNote116 = helper.CreateInvoice<ARCreditNote>(branchS, proxyD, false, "116", ZGuid.Empty, "S00001017", -40);
				Factory.Save();

				DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(Factory);
				headers.Load("SELECT COUNT(*) AS Count FROM dbo.AccTransactionHeader");
				AssertEquals("Precondition: transactions must exist", 16, (ZInt)headers[0]["Count"]);

				UnapprovedTransactionCandidateCollection unapprovedTransactionCandidateCollection = new UnapprovedTransactionCandidateCollection(Factory);
				unapprovedTransactionCandidateCollection.SuspendValidation();
				try
				{
					unapprovedTransactionCandidateCollection.Load();
				}
				finally
				{
					unapprovedTransactionCandidateCollection.ResumeValidation();
				}
			}
		}

		void TestSingleTransactionPostAndConvert(PostType postType)
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				helper.SetupSource();
				helper.ChangeCurrentCompanyViaBranch(helper.SourceCompany, helper.SourceCompanyBranch);

				helper.CreateJobAndPost(postType, "1");

				helper.ChangeCurrentCompanyViaBranch(helper.TargetCompany, helper.TargetBranch);
				helper.EnsureCC1ChargeCodeInDBForCurrentCompany();

				UnapprovedTransactionCandidateCollection collection = new UnapprovedTransactionCandidateCollection(Factory);

				collection.Load();
				AssertEquals("Collection Should contain 1 item", 1, collection.Count);

				UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
				InvoicingBase aRInvoice = (InvoicingBase)collection[0];

				aRInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
				aRInvoice.Lines[0].AL_AT = ((ILineMatching)aRInvoice.Lines[0]).Charge.JR_AT_SellGSTRate;

				AssertNotNull("Generic charge should be set for validation to work however this is done outside poster", aRInvoice.Lines[0].GenericCharge);
				aRInvoice.RunPreSaveValidation();
				AssertNoRowErrors(aRInvoice);
				AssertNoErrors(aRInvoice);
				InvoicingBase aPInvoice = converter.ConvertToAP(aRInvoice, false);
				aPInvoice.Factory.Save();

				Factory.Save();
				collection.Load();
				AssertEquals("Collection Should not contain anything because invoice has been converted", 0, collection.Count);
			}
			finally
			{
				helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
			}
		}

		void TestSingleTransactionWithReversal(PostType postType)
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentBranch = GlbBranch.CurrentBranch;

			try
			{
				helper.ChangeCurrentCompanyViaBranch(helper.SourceCompany, helper.SourceCompanyBranch);
				helper.SetupSourceDebtor();
				AssertEquals("Precondition: Source Debtor is org proxy for for target company", helper.SourceDebtor.PK, helper.TargetCompany.OrgProxy.PK);
				Assert("Precondition: Source Creditor is AR in source company", helper.SourceDebtor.CompanyData.OB_IsDebtor);

				helper.SetupSourceProxy();
				helper.SourceProxy.CompanyDataCollection.Load(new ZQuery(OrgCompanyDataSchema.OB_GC, helper.TargetCompany.PK));
				OrgCompanyData sourceProxyTargetCompanyData = helper.SourceProxy.CompanyDataCollection[0];
				Assert("Precondition: SourceCompany Org proxy must be AP in target company", sourceProxyTargetCompanyData.OB_IsCreditor);

				helper.CreateJobAndPost(postType, "1");

				helper.ChangeCurrentCompanyViaBranch(helper.TargetCompany, helper.TargetBranch);
				helper.EnsureCC1ChargeCodeInDBForCurrentCompany();

				UnapprovedTransactionCandidateCollection collection = new UnapprovedTransactionCandidateCollection(Factory);
				AccChargeCode code = TestObjectCreator.CC1;
				collection.Load();
				AssertEquals("Collection Should contain 1 item", 1, collection.Count);
				Assert("Code needs ot be in DB for generic charge to work properly", code.IsInDatabase);

				InvoicingBase aRInvoice = (InvoicingBase)collection[0];
				AssertNotNull("Generic charge should be set for validation to work however this is done outside poster", aRInvoice.Lines[0].GenericCharge);
				InvoicingBaseReversing reverser = new InvoicingBaseReversing(aRInvoice);
				reverser.Reverse();
				Factory.Save();

				collection.Load();
				AssertEquals("Collection Should not contain anything because invoice has been reversed", 0, collection.Count);
			}
			finally
			{
				helper.ChangeCurrentCompanyViaBranch(currentCompany, currentBranch);
			}
		}

		#region BaseTestCaseOverrides

		UnapprovedTransactionTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(DateTime.Now.Year, 1, 1));
			helper = new UnapprovedTransactionTestHelper(Factory, TestObjectCreator);
			helper.SourceProxy.OH_IsActive = true;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			UnapprovedTransactionCandidateCollection collection = new UnapprovedTransactionCandidateCollection(Factory);
			collection.Load();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARInvoice));
		}

		#endregion

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;
	}
}
