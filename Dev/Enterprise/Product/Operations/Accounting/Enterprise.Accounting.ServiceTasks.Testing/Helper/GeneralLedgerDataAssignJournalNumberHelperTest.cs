using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class GeneralLedgerDataAssignJournalNumberHelperTest : TestCaseWithFactory
	{
		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberForTransactionType_CTR_NumberRule_ALL()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			TestAssignJournalNumberForTransactionType_CTR();
		}

		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberForTransactionType_CTR_NumberRule_GRP()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var groupCollection = new JournalEntriesClassificationGroupCollection(fallbackLevel);
			var group = groupCollection.AddNew();
			group.GroupCode = "TST";
			group.Ledger = "AP";
			group.TransactionType = "CTR";
			var group1 = groupCollection.AddNew();
			group1.GroupCode = "TST";
			group1.Ledger = "AR";
			group1.TransactionType = "CTR";
			var groupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var groupCode = groupCodeCollection.AddNew();
			groupCode.Code = "TST";
			groupCode.Description = "Test Description";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, groupCodeCollection);
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, groupCollection);

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			TestAssignJournalNumberForTransactionType_CTR();
		}

		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberForTransactionType_CTR_NumberRule_TRN()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.TRN
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			TestAssignJournalNumberForTransactionType_CTR();
		}

		void TestAssignJournalNumberForTransactionType_CTR()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionNum = "1100";
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Contra;
			header.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);

			var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header1.AH_TransactionNum = "1100";
			header1.AH_Ledger = LedgerTypes.AccountsReceivable;
			header1.AH_TransactionType = TransactionTypes.Contra;
			header1.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);

			var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header2.AH_TransactionNum = "1101";
			header2.AH_Ledger = LedgerTypes.AccountsPayable;
			header2.AH_TransactionType = TransactionTypes.Contra;
			header2.AH_PostDate = new ZDateTime(2024, 9, 2, 15, 0, 0);

			var header3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header3.AH_TransactionNum = "1101";
			header3.AH_Ledger = LedgerTypes.AccountsReceivable;
			header3.AH_TransactionType = TransactionTypes.Contra;
			header3.AH_PostDate = new ZDateTime(2024, 9, 2, 15, 0, 0);

			CreateGeneralLedgerData(header.PK, ZGuid.Empty, ZGuid.Empty, header.AH_PostDate);
			CreateGeneralLedgerData(header1.PK, ZGuid.Empty, ZGuid.Empty, header1.AH_PostDate);
			CreateGeneralLedgerData(header2.PK, ZGuid.Empty, ZGuid.Empty, header2.AH_PostDate);
			CreateGeneralLedgerData(header3.PK, ZGuid.Empty, ZGuid.Empty, header3.AH_PostDate);

			Factory.Save();

			GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
			Factory.ReloadAll<AccGeneralLedgerData>();

			var gldLine = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, header.PK));
			AssertEquals("gldLine's GLD_JournalEntriesNumber:", "0000000001", gldLine.GLD_JournalEntriesNumber);

			var gldLine1 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, header1.PK));
			AssertEquals("gldLine1's GLD_JournalEntriesNumber:", "0000000001", gldLine1.GLD_JournalEntriesNumber);

			var gldLine2 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, header2.PK));
			AssertEquals("gldLine2's GLD_JournalEntriesNumber:", "0000000002", gldLine2.GLD_JournalEntriesNumber);

			var gldLine3 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, header3.PK));
			AssertEquals("gldLine3's GLD_JournalEntriesNumber:", "0000000002", gldLine3.GLD_JournalEntriesNumber);
		}

		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberForGLD()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			AssertJournalNumberWhenTransactionHeaderIsNotNull();
			AssertJournalNumberWhenTransactionLinesIsNotNull();
			AssertJournalNumberWhenTaxGLMovementIsNotNull();

			void AssertJournalNumberWhenTransactionHeaderIsNotNull()
			{
				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
				arInvoice1.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1m, 100m);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1m, 200m);

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1101", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
				arInvoice2.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);

				var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
				apInvoice1.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);
				var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1101", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
				apInvoice2.AH_PostDate = new ZDateTime(2024, 9, 2, 15, 0, 0);

				Factory.Save();

				var lines = new List<AccTransactionLines>
				{
						arInvoice1.Lines[0],
						arInvoice1.Lines[1],
						arInvoice2.Lines[0],
						apInvoice1.Lines[0],
						apInvoice2.Lines[0]
				};
				lines.ForEach(x => CreateGeneralLedgerData(x.AL_AH, x.PK, ZGuid.Empty, x.AL_PostDate));
				Factory.Save();

				GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
				Factory.ReloadAll<AccGeneralLedgerData>();

				var gldLine1 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, apInvoice1.Lines[0].PK));
				AssertEquals("gldLine1's GLD_JournalEntriesNumber:", "0000000001", gldLine1.GLD_JournalEntriesNumber);

				var gldLine2 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, arInvoice1.Lines[0].PK));
				var gldLine3 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, arInvoice1.Lines[1].PK));
				AssertEquals("gldLine2's GLD_JournalEntriesNumber:", "0000000002", gldLine2.GLD_JournalEntriesNumber);
				AssertEquals("gldLine3's GLD_JournalEntriesNumber:", "0000000002", gldLine3.GLD_JournalEntriesNumber);

				var gldLine4 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, arInvoice2.Lines[0].PK));
				AssertEquals("gldLine4's GLD_JournalEntriesNumber:", "0000000003", gldLine4.GLD_JournalEntriesNumber);

				var gldLine5 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, apInvoice2.Lines[0].PK));
				AssertEquals("gldLine5's GLD_JournalEntriesNumber:", "0000000004", gldLine5.GLD_JournalEntriesNumber);

				CreateGeneralLedgerData(arInvoice1.Lines[2].AL_AH, arInvoice1.Lines[2].PK, ZGuid.Empty, arInvoice1.Lines[2].AL_PostDate);
				Factory.Save();

				GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
				Factory.ReloadAll<AccGeneralLedgerData>();

				var gldLine6 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, arInvoice1.Lines[2].PK));
				AssertEquals("gldLine6's GLD_JournalEntriesNumber:", "0000000002", gldLine6.GLD_JournalEntriesNumber);
			}

			void AssertJournalNumberWhenTransactionLinesIsNotNull()
			{
				var job1 = TestObjectCreator.CreateJob("S001", null, 0M, null, 0M);
				var job2 = TestObjectCreator.CreateJob("S002", null, 0M, null, 0M);
				var acr = TestObjectCreator.CreateAccrual(job1);
				var wip = TestObjectCreator.CreateWIP(job2);
				Factory.Save();

				var lines = new List<AccTransactionLines>
				{
						acr,
						wip
				};
				lines.ForEach(x => CreateGeneralLedgerData(x.AL_AH, x.PK, ZGuid.Empty, x.AL_PostDate));
				Factory.Save();

				GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
				Factory.ReloadAll<AccGeneralLedgerData>();

				var acrGLD = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, acr.PK));
				var wipGLD = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, wip.PK));
				AssertEquals("arc's GLD_JournalEntriesNumber:", "0000000005", acrGLD.GLD_JournalEntriesNumber);
				AssertEquals("wip's GLD_JournalEntriesNumber:", "0000000006", wipGLD.GLD_JournalEntriesNumber);
			}

			void AssertJournalNumberWhenTaxGLMovementIsNotNull()
			{
				var postDate = ZDateTime.UtcNow;

				var glMovement1 = CreateAccTaxGLMovement("Tx0001", postDate);
				var glMovement2 = CreateAccTaxGLMovement("Tx0002", postDate);
				CreateGeneralLedgerData(ZGuid.Empty, ZGuid.Empty, glMovement1.PK, postDate);
				CreateGeneralLedgerData(ZGuid.Empty, ZGuid.Empty, glMovement2.PK, postDate);
				Factory.Save();

				GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
				Factory.ReloadAll<AccGeneralLedgerData>();

				var glMovementGLD1 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, glMovement1.PK));
				var glMovementGLD2 = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, glMovement2.PK));
				AssertEquals("glMovementGLD1's GLD_JournalEntriesNumber:", "0000000007", glMovementGLD1.GLD_JournalEntriesNumber);
				AssertEquals("glMovementGLD2's GLD_JournalEntriesNumber:", "0000000008", glMovementGLD2.GLD_JournalEntriesNumber);
			}
		}

		void CreateGeneralLedgerData(ZGuid headerPk, ZGuid linePk, ZGuid taxGLMovementPk, ZDateTime postDate)
		{
			var gld = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			gld.GLD_GC_Company = Env.CurrentCompanyPK;
			gld.GLD_GB_Branch = Env.CurrentBranchPK;
			gld.GLD_AH_TransactionHeader = headerPk;
			gld.GLD_AL_TransactionLine = linePk;
			gld.GLD_ATM_TaxGLMovement = taxGLMovementPk;
			gld.GLD_PostDate = postDate;
			gld.GLD_PostPeriod = PeriodCalculator.GetPeriodFromDate(postDate.ToDateTime());
			gld.GLD_Type = AccountingConstants.GLDTypeCodes.Recognition;
			gld.GLD_GLAccountType = GLDAccountTypes.ARControlAccountForGST;
		}

		AccTaxGLMovement CreateAccTaxGLMovement(string transactionNum, ZDateTime postDate)
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionNum = transactionNum;
			header.AH_PostDate = postDate;

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = header.PK;
			taxTransaction.ATT_PostDate = (ZDate)postDate;
			taxTransaction.ATT_GC = Env.CurrentCompanyPK;
			taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();

			return Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK)).FirstOrDefault();
		}

		public void TestNotContainsError_WhenCompanyHasNoBranch()
		{
			var company = TestObjectCreator.CreateNewCompany("TST", TestObjectCreator.AALSHI);
			Factory.Save();

			Assert(!Factory.Exists(typeof(GlbBranch),new ZQuery(GlbBranchSchema.GB_GC, company.PK)));
			Assert(company.GC_IsActive);

			GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(company);

			AssertNotContains("Cannot find an active branch for company with code", ErrorReporter.LastMessageReported);
		}

		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberWithChineseCharacters()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};

			var journalEntriesNumberCustomisationCollection = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>();
			var customElement = journalEntriesNumberCustomisationCollection.FirstOrDefault(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.CustomElement1);
			customElement.Code = "记账凭证";
			customElement.Include = true;
			customElement.Fountain = false;
			customElement.Order = 1;
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			arInvoice.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);

			CreateGeneralLedgerData(arInvoice.PK, ZGuid.Empty, ZGuid.Empty, arInvoice.AH_PostDate);

			Factory.Save();

			GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
			Factory.ReloadAll<AccGeneralLedgerData>();

			var gldLine = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, arInvoice.PK));
			AssertEquals("gldLine's GLD_JournalEntriesNumber:", "记账凭证0000000001", gldLine.GLD_JournalEntriesNumber);
		}

		[TestDate(2024, 10, 01)]
		public void TestAssignJournalNumberWithGroupCodePrefix()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var groupCollection = new JournalEntriesClassificationGroupCollection(fallbackLevel);
			var group = groupCollection.AddNew();
			group.GroupCode = "ZZZ";
			group.Ledger = LedgerTypes.AccountsReceivable;
			group.TransactionType = TransactionTypes.Invoice;
			var groupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			var groupCode = groupCodeCollection.AddNew();
			groupCode.Code = "ZZZ";
			groupCode.Description = "转账类";
			groupCode.Prefix = "转";
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, groupCodeCollection);
			AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, groupCollection);

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL,
			};

			var journalEntriesNumberCustomisationCollection = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>();
			var prefixCustomisation = journalEntriesNumberCustomisationCollection.FirstOrDefault(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix);
			prefixCustomisation.Include = true;
			prefixCustomisation.Fountain = false;
			prefixCustomisation.Order = 1;
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			arInvoice.AH_PostDate = new ZDateTime(2024, 9, 1, 15, 0, 0);

			CreateGeneralLedgerData(arInvoice.PK, ZGuid.Empty, ZGuid.Empty, arInvoice.AH_PostDate);

			Factory.Save();

			GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(GlbCompany.CurrentCompany);
			Factory.ReloadAll<AccGeneralLedgerData>();

			var gldLine = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, arInvoice.PK));
			AssertEquals("gldLine's GLD_JournalEntriesNumber:", "转0000000001", gldLine.GLD_JournalEntriesNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			TestObjectCreator.CreateTestPeriodsForEntireYear(TestObjectCreator.NonCurrentCompany, ZDateTime.Today.Year);
			TestObjectCreator.SetTemporaryControlAccounts();
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);
		}

		GeneralLedgerDataAssignJournalNumberHelper GeneralLedgerDataAssignJournalNumberHelper => generalLedgerDataAssignJournalNumberHelper ?? (generalLedgerDataAssignJournalNumberHelper = new GeneralLedgerDataAssignJournalNumberHelper());
		GeneralLedgerDataAssignJournalNumberHelper generalLedgerDataAssignJournalNumberHelper;

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;

		PeriodCalculator PeriodCalculator => periodCalculator ?? (periodCalculator = new PeriodCalculator("445", new DateTime(2024, 1, 1), "SUN"));
		PeriodCalculator periodCalculator;
	}
}
