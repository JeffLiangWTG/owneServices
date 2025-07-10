using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class LVXJobsConsolidateRunnerTest : TestCaseWithFactory
	{
		public void TestBatchMergeAtFinalProcess()
		{
			int mergeCount = 0;
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			declaration1.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration1.LVXInvoiceHeader.InvoiceLines.AddNew();
			declaration2.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration2.LVXInvoiceHeader.InvoiceLines.AddNew();
			declaration3.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration3.LVXInvoiceHeader.InvoiceLines.AddNew();
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "VAR", 2015, 4, importer1.PK, branch1.PK, "PA", "A");
			Factory.Save();
			lvs1.MergedSuccessfully += () => { mergeCount++; };
			var log = new NotificationsLogWrapper(new Notifications());

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				var runner = new LVXJobsConsolidateRunner(log, Factory);
				runner.ConsolidateLVXJobs(new JobDeclaration[] { declaration1, declaration2, declaration3 }, true);
				AssertEquals("LVS job total merge count", 1, mergeCount);
			}
		}

		#region TestLoadLVXJobs

		public void TestNotLoadLVXJobsWithInvalidEntryAuthorisation()
		{
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var bizObj3 = declaration3 as BusinessObject;
			bizObj3[JobDeclarationSchema.JE_EntryAuthorisationDate] = null;
			declaration1.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration2.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration3.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			Factory.Save();

			AssertEquals(true, declaration1.JE_EntryAuthorisationDate.IsValid);
			AssertEquals(true, declaration2.JE_EntryAuthorisationDate.IsValid);
			AssertEquals(false, declaration3.JE_EntryAuthorisationDate.IsValid);

			var log = new NotificationsLogWrapper(new Notifications());
			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				var runner = new LVXJobsConsolidateRunner(log, Factory);
				var declarationPKs = LVXJobsConsolidateRunner.SelectLVXJobPKsReadyForConsolidation().Select(x => x.DeclarationPK);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
			}
		}

		public void TestLoadLVXJobs()
		{
			var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 3, importer2.PK, branch1.PK, "1111", "A");
			var declaration4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 3, importer1.PK, branch2.PK, "1111", "A");
			var declaration5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 3, importer1.PK, branch1.PK, "2222", "A");
			var declaration6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer1.PK, branch1.PK, "1111", "B");
			var declaration7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var declaration8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 3, importer1.PK, branch3.PK, "1111", "A");
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration7.LVXInvoiceHeader, lvs1);
			Factory.Save();
			var log = new NotificationsLogWrapper(new Notifications());

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				var selectionCriteria = new LVXSelectionCriteriaBO(Factory);
				var runner = new LVXJobsConsolidateRunner(log, Factory);
				selectionCriteria.PeriodYear = 2015;
				selectionCriteria.PeriodMonth = 3;
				var declarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration8.PK);

				selectionCriteria.OH_Importer = importer1.PK;
				declarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration8.PK);

				selectionCriteria.Branch = branch1.PK;
				declarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration8.PK);

				selectionCriteria.ProvinceOfClearance = "PA";
				declarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration8.PK);

				selectionCriteria.GS_NKBroker = "A";
				declarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration8.PK);
			}
		}

		public void TestLoadLVXJobsReadyForConsolidation()
		{
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var declaration4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 4, importer2.PK, branch2.PK, "1111", "A");
			var declaration5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 3, importer1.PK, branch2.PK, "1111", "A");
			var declaration6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 4, importer2.PK, branch3.PK, "1111", "A");
			var declaration7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 3, importer1.PK, branch3.PK, "1111", "A");
			declaration1.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration2.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration3.LVXInvoiceHeader.CA_ReadyForConsolidation = false;
			declaration4.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			declaration6.LVXInvoiceHeader.CA_ReadyForConsolidation = false;
			declaration7.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			Factory.Save();
			var log = new NotificationsLogWrapper(new Notifications());

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				var runner = new LVXJobsConsolidateRunner(log, Factory);
				var declarationPKs = LVXJobsConsolidateRunner.SelectLVXJobPKsReadyForConsolidation().Select(x => x.DeclarationPK);
				AssertCollectionContains(declarationPKs, p => p == declaration1.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration2.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration3.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration4.PK);
				AssertCollectionContains(declarationPKs, p => p == declaration5.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration6.PK);
				AssertCollectionNotContains(declarationPKs, p => p == declaration7.PK);
			}
		}

		#endregion

		#region TestConsolidateLVXJobs

		public void TestConsolidateLVXJobs_MatchLVSExists()
		{
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvx2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 3, importer2.PK, branch1.PK, "1111", "A");
			var lvx3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 3, importer1.PK, branch2.PK, "2222", "B");
			var lvx4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 3, importer2.PK, branch1.PK, "2222", "B");
			var lvx5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 3, importer2.PK, branch2.PK, "1111", "B");
			var lvx6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer2.PK, branch2.PK, "2222", "A");
			var lvx7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer2.PK, branch2.PK, "2222", "B");
			var lvx8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var lvx9 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000009", 2015, 3, ZGuid.Empty, branch1.PK, "", "");
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			var lvs2 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008002", "VAR", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			var lvs3 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008003", "MSI", 2015, 3, importer1.PK, branch3.PK, "PA", "A");
			var lvx10 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000010", 2015, 3, ZGuid.Empty, branch1.PK, "", "");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx10.LVXInvoiceHeader, lvs1);
			lvx10.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				var runner = new LVXJobsConsolidateRunnerForTesting(Factory);
				runner.ConsolidationStrategyDict.Add(lvx1, new ConsolidationStrategyForTesting(true, true, true, true));
				runner.ConsolidationStrategyDict.Add(lvx2, new ConsolidationStrategyForTesting(false, true, true, true));
				runner.ConsolidationStrategyDict.Add(lvx3, new ConsolidationStrategyForTesting(true, false, false, false));
				runner.ConsolidationStrategyDict.Add(lvx4, new ConsolidationStrategyForTesting(false, true, false, false));
				runner.ConsolidationStrategyDict.Add(lvx5, new ConsolidationStrategyForTesting(false, false, true, false));
				runner.ConsolidationStrategyDict.Add(lvx6, new ConsolidationStrategyForTesting(false, false, false, true));
				runner.ConsolidationStrategyDict.Add(lvx7, new ConsolidationStrategyForTesting(false, false, false, false));
				runner.ConsolidationStrategyDict.Add(lvx8, new ConsolidationStrategyForTesting(false, false, false, false));
				runner.ConsolidationStrategyDict.Add(lvx9, new ConsolidationStrategyForTesting(true, true, true, true));
				runner.ConsolidationStrategyDict.Add(lvx10, new ConsolidationStrategyForTesting(true, false, false, false));

				runner.ConsolidateLVXJobs(new JobDeclaration[] { lvx1, lvx2, lvx3, lvx4, lvx5, lvx6, lvx7, lvx8, lvx9, lvx10 }, true);
				AssertLVXJobsAttachedToLVSJob(lvs1, lvx1, lvx3, lvx9);
				AssertLVXJobsAttachedToLVSJob(lvs2, lvx2, lvx4, lvx5, lvx6, lvx7);
				AssertLVXJobsNotAttachedToLVSJob(lvs1, lvx2, lvx4, lvx5, lvx6, lvx7, lvx8);
				AssertLVXJobsNotAttachedToLVSJob(lvs2, lvx1, lvx3, lvx9);
				AssertLVXJobsNotAttachedToLVSJob(lvs3, lvx1, lvx2, lvx3, lvx4, lvx5, lvx6, lvx7, lvx8, lvx9, lvx10);

				AssertCollectionContains("lvx1", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx1), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx2", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx2), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx3", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx3), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx4", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx4), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx5", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx5), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx6", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx6), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx7", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx7), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx9", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx9), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
				AssertCollectionContains("lvx10", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetLVXHasAdditionalDeclarationLog, GetDeclarationIdLink(lvx10), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
			}
		}

		[ExpectNoExceptions]
		public void TestConsolidateLVXJobs_MatchLVSExists_MergeMessage()
		{
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			Factory.Save();

			var invoice2 = lvx1.LVXInvoiceHeader;
			invoice2.JobComInvoiceLines.AddNew().JI_CL = ZGuid.NewZGuid();
			invoice2.JobComInvoiceLines.AddNew().JI_CL = ZGuid.NewZGuid();
			invoice2.JZ_InvoiceNumber = "INV111";

			var runner = new LVXJobsConsolidateRunnerForTesting(Factory);
			runner.ConsolidationStrategyDict.Add(lvx1, new ConsolidationStrategyForTesting(true, true, true, true));

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			using (var mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + lvs1.PK))
			{
				Assert("Locked OK", mutex.Lock());
				runner.ConsolidateLVXJobs(new[] { lvx1 }, true);
				Assert("contains merge message", runner.ExposedLog.messages[0].Contains("is attached to Consolidated LVS Declaration"));
				Assert("contains merge message", runner.ExposedLog.messages[1].Contains("is in the process of merging this job; system cannot merge this data as it will result in a different entry details.\r\nPlease retry merging when the other user has finished."));
			}
		}

		public void TestConsolidateLVXJobs_MatchLVSNotExists()
		{
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvx2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 3, importer2.PK, branch1.PK, "1111", "A");
			var lvx3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 3, importer1.PK, branch2.PK, "2222", "B");
			var lvx4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 3, importer2.PK, branch1.PK, "2222", "B");
			var lvx5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 3, importer2.PK, branch2.PK, "1111", "B");
			var lvx6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer2.PK, branch2.PK, "2222", "A");
			var lvx7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer2.PK, branch2.PK, "2222", "B");
			var lvx8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
			var lvx9 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000009", 2015, 4, ZGuid.Empty, branch1.PK, "", "");
			var lvx10 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000010", 2015, 4, ZGuid.Empty, branch1.PK, "", "");
			lvx10.LVXInvoiceHeader.Delete();
			var lvx11 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000011", 2015, 4, ZGuid.Empty, branch1.PK, "", "");
			lvx11.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();

			var runner = new LVXJobsConsolidateRunnerForTesting(Factory);
			runner.ConsolidationStrategyDict.Add(lvx1, new ConsolidationStrategyForTesting(true, true, true, true));
			runner.ConsolidationStrategyDict.Add(lvx2, new ConsolidationStrategyForTesting(false, true, true, true));
			runner.ConsolidationStrategyDict.Add(lvx3, new ConsolidationStrategyForTesting(true, false, false, false));
			runner.ConsolidationStrategyDict.Add(lvx4, new ConsolidationStrategyForTesting(false, true, false, false));
			runner.ConsolidationStrategyDict.Add(lvx5, new ConsolidationStrategyForTesting(false, false, true, false));
			runner.ConsolidationStrategyDict.Add(lvx6, new ConsolidationStrategyForTesting(false, false, false, true));
			runner.ConsolidationStrategyDict.Add(lvx7, new ConsolidationStrategyForTesting(false, false, false, false));
			runner.ConsolidationStrategyDict.Add(lvx8, new ConsolidationStrategyForTesting(true, true, true, true));
			runner.ConsolidationStrategyDict.Add(lvx9, new ConsolidationStrategyForTesting(true, true, true, true));
			runner.ConsolidateLVXJobs(new JobDeclaration[] { lvx1, lvx2, lvx3, lvx4, lvx5, lvx6, lvx7, lvx8, lvx9, lvx10, lvx11 }, true);

			var lvs1 = lvx1.LVXInvoiceHeader.FirstAdditionalDeclaration;
			AssertNotNull("A LVS Declaration should be created", lvs1);
			AssertLVXJobsAttachedToLVSJob(lvs1, lvx1, lvx3);
			AssertEquals("MessageType", "LVS", lvs1.JE_MessageType);
			AssertEquals("MessageSubType", "MSI", lvs1.JE_MessageSubType);
			AssertEquals("Period", new ZDateTime(2015, 3, 1), lvs1.JE_EntryAuthorisationDate);
			AssertEquals("Importer", importer1.PK, lvs1.JE_OH_Importer);
			AssertEquals("Branch", branch1.PK, lvs1.JE_GB);
			AssertEquals("ProvinceOfClearance", "PA", lvs1.CA_ProvinceOfClearance);
			AssertEquals("Broker", "A", lvs1.JE_GS_NKCusAgent);

			var lvs2 = lvx2.LVXInvoiceHeader.FirstAdditionalDeclaration;
			AssertNotNull("A LVS Declaration should be created", lvs2);
			AssertLVXJobsAttachedToLVSJob(lvs2, lvx2, lvx4, lvx5, lvx6, lvx7);
			AssertEquals("MessageType", "LVS", lvs2.JE_MessageType);
			AssertEquals("MessageSubType", "VAR", lvs2.JE_MessageSubType);
			AssertEquals("Period", new ZDateTime(2015, 3, 1), lvs2.JE_EntryAuthorisationDate);
			AssertEquals("Importer", ZGuid.Empty, lvs2.JE_OH_Importer);
			AssertEquals("Branch", branch1.PK, lvs2.JE_GB);
			AssertEquals("ProvinceOfClearance", "PA", lvs2.CA_ProvinceOfClearance);
			AssertEquals("Broker", "A", lvs2.JE_GS_NKCusAgent);

			var lvs3 = lvx8.LVXInvoiceHeader.FirstAdditionalDeclaration;
			AssertNotNull("A LVS Declaration should be created", lvs3);
			AssertLVXJobsAttachedToLVSJob(lvs3, lvx8, lvx9);
			AssertEquals("MessageType", "LVS", lvs3.JE_MessageType);
			AssertEquals("MessageSubType", "MSI", lvs3.JE_MessageSubType);
			AssertEquals("Period", new ZDateTime(2015, 4, 1), lvs3.JE_EntryAuthorisationDate);
			AssertEquals("Importer", importer1.PK, lvs3.JE_OH_Importer);
			AssertEquals("Branch", branch1.PK, lvs3.JE_GB);
			AssertEquals("ProvinceOfClearance", "PA", lvs3.CA_ProvinceOfClearance);
			AssertEquals("Broker", "A", lvs3.JE_GS_NKCusAgent);

			AssertCollectionContains("lvx1", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx1), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx2", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx2), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx3", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx3), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx4", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx4), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx5", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx5), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx6", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx6), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx7", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx7), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx9", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXAttachedToConsolidatedLVSLog, GetDeclarationIdLink(lvx9), GetDeclarationIdLink(lvs3)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx10", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetDeclarationHasNoInvoicesLog, GetDeclarationIdLink(lvx10)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx11", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetDeclarationMergeErrorLog, GetDeclarationIdLink(lvx11), Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines), runner.ExposedLog.messages);
		}

		#endregion

		#region TestDeConsolidateLVXJobs

		public void TestDeConsolidateLVXJobs()
		{
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx1.LVXInvoiceHeader, lvs1);
			var lvx2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvs2 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008002", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx2.LVXInvoiceHeader, lvs2);
			var lvx3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvx4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			lvx4.LVXInvoiceHeader.Delete();
			var entryHeader = lvs2.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			Factory.Save();

			var runner = new LVXJobsConsolidateRunnerForTesting(Factory);
			runner.DeConsolidateLVXJobs(new JobDeclaration[] { lvx1, lvx2, lvx3, lvx4 });

			AssertNull(lvx1.LVXInvoiceHeader.FirstAdditionalDeclaration);
			AssertLVXJobsAttachedToLVSJob(lvs2, lvx2);

			AssertCollectionContains("lvx1", "INFO: " + string.Format(LVXJobsConsolidateHelper.GetLVXDetachedFromConsolidatedLVSLog, GetDeclarationIdLink(lvx1), GetDeclarationIdLink(lvs1)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx2", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetLVXHasAdditionalDeclarationWithB3AcceptedLog, GetDeclarationIdLink(lvx2), GetDeclarationIdLink(lvs2)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx3", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetLVXHasNoAdditionalDeclarationLog, GetDeclarationIdLink(lvx3)), runner.ExposedLog.messages);
			AssertCollectionContains("lvx4", "WARNING: " + string.Format(LVXJobsConsolidateHelper.GetDeclarationHasNoInvoicesLog, GetDeclarationIdLink(lvx4)), runner.ExposedLog.messages);
		}

		[ExpectNoExceptions]
		public void TestDeConsolidateLVXJobs_MergeMessage()
		{
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx1.LVXInvoiceHeader, lvs1);
			Factory.Save();

			var mergedLine1 = lvs1.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var mergedLine2 = lvs1.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			lvs1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = mergedLine1.PK;
			lvs1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = mergedLine2.PK;

			var runner = new LVXJobsConsolidateRunnerForTesting(Factory);
			using (var mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + lvs1.PK))
			{
				Assert("Locked OK", mutex.Lock());
				runner.DeConsolidateLVXJobs(new[] { lvx1 });
				Assert("contains merge message", runner.ExposedLog.messages[0].Contains("is in the process of merging this job; system cannot merge this data as it will result in a different entry details.\r\nPlease retry merging when the other user has finished."));
			}
		}

		#endregion

		#region Implementation

		string GetDeclarationIdLink(JobDeclaration declaration)
		{
			return string.Format("[HL {0}]", declaration.JE_DeclarationReference);
		}

		void AssertLVXJobsAttachedToLVSJob(JobDeclaration lvsJob, params JobDeclaration[] lvxJobs)
		{
			foreach (var lvxJob in lvxJobs)
			{
				AssertEquals(string.Format("{0} should be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.PK, lvxJob.LVXInvoiceHeader.FirstAdditionalDeclaration.PK);
				AssertNotNull(string.Format("{0}'s Invoice should be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.Invoices.FindByPK(lvxJob.LVXInvoiceHeader.PK));
				AssertNotNull(string.Format("{0}'s GroupHeader should be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.JobComInvoiceGroupHeaders.FindByPK(lvxJob.JobComInvoiceGroupHeaders[0].PK));
			}
		}

		void AssertLVXJobsNotAttachedToLVSJob(JobDeclaration lvsJob, params JobDeclaration[] lvxJobs)
		{
			foreach (var lvxJob in lvxJobs)
			{
				AssertNotEquals(string.Format("{0} should not be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.PK, lvxJob.LVXInvoiceHeader.FirstAdditionalDeclaration.PK);
				AssertNull(string.Format("{0}'s Invoice should not be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.Invoices.FindByPK(lvxJob.LVXInvoiceHeader.PK));
				AssertNull(string.Format("{0}'s GroupHeader should not be attached to {1}", lvxJob.HumanReadableName, lvsJob.HumanReadableName), lvsJob.JobComInvoiceGroupHeaders.FindByPK(lvxJob.JobComInvoiceGroupHeaders[0].PK));
			}
		}

		OrgHeader importer1;
		OrgHeader importer2;
		GlbBranch branch1;
		GlbBranch branch2;
		GlbBranch branch3;
		GlbCompany company1;
		GlbCompany company2;

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345");
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_FullName = "Importer2";
			company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "GBB";
			company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC2";
			branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "GBC";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "1111", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2222", "2222", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PB");

			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20002", companyPk: company2.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20002");

			Factory.Save();
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
