using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MergeManagerWhichSupportsAutoMergeTest : Customs.Business.Testing.MergeManagerWhichSupportsAutoMergeTest
	{
		public void TestGetReasonCannotMerge()
		{
			AssertGSTInconsistancy(isInconsistentExpected: true, isMergeCancelledExpected: true, expectedAdditionalLogsCount: 1, importerGSTFlag: true, actionAnswer: false, feeType: CusEntryChargeTypeList.Codes.GSTAmount);
			AssertGSTInconsistancy(isInconsistentExpected: false, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 0, importerGSTFlag: true, actionAnswer: false, feeType: CusEntryChargeTypeList.Codes.GSTDeferred);
			AssertGSTInconsistancy(isInconsistentExpected: true, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 2, importerGSTFlag: true, actionAnswer: true, feeType: CusEntryChargeTypeList.Codes.GSTAmount);
			AssertGSTInconsistancy(isInconsistentExpected: false, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 0, importerGSTFlag: true, actionAnswer: true, feeType: CusEntryChargeTypeList.Codes.GSTDeferred);

			AssertGSTInconsistancy(isInconsistentExpected: false, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 0, importerGSTFlag: false, actionAnswer: false, feeType: CusEntryChargeTypeList.Codes.GSTAmount);
			AssertGSTInconsistancy(isInconsistentExpected: true, isMergeCancelledExpected: true, expectedAdditionalLogsCount: 1, importerGSTFlag: false, actionAnswer: false, feeType: CusEntryChargeTypeList.Codes.GSTDeferred);
			AssertGSTInconsistancy(isInconsistentExpected: false, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 0, importerGSTFlag: false, actionAnswer: true, feeType: CusEntryChargeTypeList.Codes.GSTAmount);
			AssertGSTInconsistancy(isInconsistentExpected: true, isMergeCancelledExpected: false, expectedAdditionalLogsCount: 2, importerGSTFlag: false, actionAnswer: true, feeType: CusEntryChargeTypeList.Codes.GSTDeferred);
		}

		void AssertGSTInconsistancy(bool isInconsistentExpected, bool isMergeCancelledExpected, int expectedAdditionalLogsCount, bool importerGSTFlag, bool actionAnswer, ZString feeType)
		{
			var testDec = GetMergedDeclaration();
			testDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			testDec.Importer.MiscServ.OM_IMIsGSTDeferred = importerGSTFlag;
			testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = notifier;
			notifier.AnswerToContinueWithAction = actionAnswer;
			AddFee(testDec, feeType);
			var manager = (CMRMergeManager)testDec.MergeManager;
			var initialLogsCount = new LogsForNominatedEvent(testDec.Logs, Events.EditedARecord).Count;
			var mergeResult = testDec.DoMerge();
			Assert("Merge was done", mergeResult);
			Assert(string.IsNullOrEmpty(notifier.ContinueWithActionMessage));
			AssertEquals("No new logs posted, except for merge done", initialLogsCount + 1, new LogsForNominatedEvent(testDec.Logs, Events.EditedARecord).Count);

			testDec.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AddFee(testDec, feeType);
			initialLogsCount = new LogsForNominatedEvent(testDec.Logs, Events.EditedARecord).Count;
			mergeResult = testDec.DoMerge();
			if (isMergeCancelledExpected)
			{
				Assert("Merge should not have occured", !mergeResult);
			}
			else
			{
				Assert("Merge should have occured", mergeResult);
			}

			if (isInconsistentExpected)
			{
				if (importerGSTFlag)
				{
					AssertEquals(@"The consignee tab of the importer organization of this declaration has the GST Deferred box ticked,
however GST information returned by Customs indicates that GST is not deferred.
If you choose to continue then the GST deferred status of this job will change.
This will cause the entry print to show GST deferred information that is inconsistent with Customs,
and auto-rated GST details are likely to be incorrect.
It is recommended that you do not continue, but you correct the Importer organization GST setting,
and then perform the Generate Entries (Merge) option from the brokerage menu.
Do you wish to continue with the merge?", notifier.ContinueWithActionMessage);
				}
				else
				{
					AssertEquals(@"The consignee tab of the importer organization of this declaration has the GST Deferred box NOT ticked,
however GST information returned by Customs indicates that GST is deferred.
If you choose to continue then the GST deferred status of this job will change.
This will cause the entry print to show GST deferred information that is inconsistent with Customs,
and auto-rated GST details are likely to be incorrect.
It is recommended that you do not continue, but you correct the Importer organization GST setting,
and then perform the Generate Entries (Merge) option from the brokerage menu.
Do you wish to continue with the merge?", notifier.ContinueWithActionMessage);
				}
			}
			else
			{
				Assert(string.IsNullOrEmpty(notifier.ContinueWithActionMessage));
			}

			AssertEquals("Correct number of logs posted", isMergeCancelledExpected ? expectedAdditionalLogsCount : expectedAdditionalLogsCount + 1, new LogsForNominatedEvent(testDec.Logs, Events.EditedARecord).Count - initialLogsCount);
		}

		void AddFee(BaseJobDeclaration testDec, ZString feeType)
		{
			testDec.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(feeType, 1m);
		}

		public void TestDutyInconsistancy()
		{
			var declaration = GetMergedDeclaration();
			var importer = Factory.New<OrgHeader>();

			declaration.JE_OH_Importer = importer.PK;
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = notifier;
			declaration.ActiveEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 100m);

			importer.AUIsDutyDeferred = true;
			declaration.DoMerge();
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box ticked,
however duty information returned by Customs indicates that duty is not deferred.
If you choose to continue then the duty deferred status of this job will change.
This will cause the entry print to show duty deferred information that is inconsistent with Customs,
and auto-rated duty details are likely to be incorrect.
It is recommended that you do not continue, but you correct the Importer organization duty setting,
and then perform the Generate Entries (Merge) option from the brokerage menu.
Do you wish to continue with the merge?", notifier.ContinueWithActionMessage);

			declaration.ActiveEntryHeaders[0].Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			importer.AUIsDutyDeferred = false;
			declaration.DoMerge();
			AssertEquals(@"The consignee tab of the importer organization of this declaration has the Duty Deferred box NOT ticked,
however duty information returned by Customs indicates that duty is deferred.
If you choose to continue then the duty deferred status of this job will change.
This will cause the entry print to show duty deferred information that is inconsistent with Customs,
and auto-rated duty details are likely to be incorrect.
It is recommended that you do not continue, but you correct the Importer organization duty setting,
and then perform the Generate Entries (Merge) option from the brokerage menu.
Do you wish to continue with the merge?", notifier.ContinueWithActionMessage);
		}

		public void TestRemovingCPDecQuestionWontCauseMerge()
		{
			var declaration = (JobDeclaration)GetMergedDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders[0].MergedLines[0].Questions.AddNew();
			declaration.DoMerge();
			AssertEquals("PreCondition", false, declaration.MergeManager.RequiresMerge);

			declaration.CustomsEntryHeaders[0].MergedLines[0].Questions.RemoveAndDeleteAll();
			AssertEquals(false, declaration.MergeManager.RequiresMerge);
		}

		public void TestAutoMergeWhenHouseMultiPackChanges2()
		{
			var declaration = (JobDeclaration)GetMergedDeclaration();
			declaration.JE_HouseBill = "house";
			declaration.JE_MasterBill = "master";
			declaration.DoMerge();
			AssertEquals("PreCondition", false, declaration.MergeManager.RequiresMerge);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C0987";
			declaration.DoMerge();
			AssertEquals("PreCondition", false, declaration.MergeManager.RequiresMerge);

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MBL2020";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HBL1010";
			houseBill.CU_MasterBill = "MBL2020";
			AssertEquals("housebill change does not cause merge", false, declaration.MergeManager.RequiresMerge);

			var multiPack = declaration.PackingGroups.AddNew();
			multiPack.CR_CU_HouseBill = houseBill.PK;
			multiPack.CR_CO_Container = container.PK;
			multiPack.Packages.AddNew().CW_PackQty = 10;
			AssertEquals("MultiPack change should not cause merge as LineMerge no longer assigns line number", false, declaration.MergeManager.RequiresMerge);

			container.CO_ContainerNumber = "C09872";
			AssertEquals("Changes to CusContainer should not cause merge", false, declaration.MergeManager.RequiresMerge);
		}

		public new void TestRequiresMergeIsOnlyEffectedByRelevantChangesCountrySpecific()
		{
			var declaration = GetMergedDeclaration();
			Factory.Save();

			declaration.JE_GoodsDescription = "XXX";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);

			declaration.JE_MergeBy = "NON";
			CheckRequiresMergeThenMerge(declaration);
			Factory.Save();
			declaration.JE_ExportDate = new ZDateTime(2008, 1, 18);
			CheckRequiresMergeThenMerge(declaration);
			Factory.Save();
			declaration.JE_MessageType = "IMX";
			CheckRequiresMergeThenMerge(declaration);
			Factory.Save();
			declaration.JE_MessageSubType = "SAC";
			CheckRequiresMergeThenMerge(declaration);
			Factory.Save();
			declaration.Invoices[0].JZ_PaymentNo = "ABC";
			CheckRequiresMergeThenMerge(declaration);

			Factory.Save();
			declaration.JE_RL_NKFinalDestination = "AUADL";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
		}

		public void TestDoNotCheckJobDeclarationForChanges()
		{
			var declaration = GetMergedDeclaration();
			AssertEquals("Merge not requires", false, declaration.MergeManager.RequiresMerge);
			declaration.JE_GoodsDescription = "XXXX";
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
		}

		public void TestChangesBeforeMergeManagerGetsCreatedLeadToMergeRequired()
		{
			var creator = new MergedDeclarationCreator(Factory);
			creator.InvoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			creator.InvoiceLine1.JI_LinePrice = 100;
			creator.Declaration.DoMerge();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration = factory2.Load<JobDeclaration>(creator.Declaration.PK);
			var invoiceLine = declaration.FilteredInvoiceLines[0];
			invoiceLine.AddInfo.ZA_ADJ = "100AUD";

			Assert("Should require merge", declaration.MergeManager.RequiresMerge);
		}

		public void TestMultipleInvoicesWithoutHblOverrideDoesNotLoop()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2007, 1, 1);
			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceHeader2.JZ_ValuationDateOverride = new ZDateTime(2007, 1, 2);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			var package = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CZUP3352349";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_MasterBill = "AQT1";

			var houseBillWithMasterBill1 = declaration.Bills.AddNew();
			houseBillWithMasterBill1.CU_HouseBill = "AQT1HBL";
			houseBillWithMasterBill1.CU_MasterBill = "AQT1";

			package.CW_HouseBill = houseBillWithMasterBill1.CU_BillUniqueCode;
			package.CW_PackQty = 12;
			package.CW_OuterPacks = 13;
			package.CW_InBondPackQty = 14;
			package.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			var houseBillWithMasterBill2 = declaration.Bills.AddNew();
			houseBillWithMasterBill2.CU_HouseBill = "AQT1HBL2";
			houseBillWithMasterBill2.CU_MasterBill = "AQT1";

			package2.CW_HouseBill = houseBillWithMasterBill2.CU_BillUniqueCode;
			package2.CW_PackQty = 12;
			package2.CW_OuterPacks = 13;
			package2.CW_InBondPackQty = 14;
			package2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			int i;
			for (i = 0; i < 10; i++)
			{
				declaration.DoMerge();
				if (!declaration.MergeManager.RequiresMerge)
				{
					break;
				}
			}
			Assert("Should not loop requiring merge", i < 9);
		}

		public override void TestSupportsAutoMerge()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			AssertEquals("SupportsAutoMerge", true, manager.SupportsAutoMerge);
		}

		public override void TestShouldCheckExistenceOfInvoices()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			AssertEquals(true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoices(manager));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;

			AssertEquals(false, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoices(manager));
		}

		public override void TestShouldCheckExistenceOfInvoiceLineForAllInvoices()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			AssertEquals(true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoiceLineForAllInvoices(manager));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;

			AssertEquals(false, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoiceLineForAllInvoices(manager));
		}

		public void TestChangingTariffRequiresMergeAfterLoadingExistingEntry()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			object temp = declaration2.MergeManager;
			var invoiceLine2 = declaration2.Invoices[0].JobComInvoiceLines[0];
			invoiceLine2.JI_Tariff = "TEST";
			invoiceLine2.AddInfo.ZA_ORG = "AU";
			AssertEquals("Requires merge after tariff change", true, declaration2.MergeManager.RequiresMerge);
		}

		public void TestGetTypesWhichDoNotEffectMerge()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var manager = new CMRMergeManager(declaration);
			var result = MergeManagerTestHelper.GetTypesWhichDoNotEffectMerge(manager);

			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(CMRCusEntryCPDec); }));
			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(AllEntryLineCPDecQuestion); }));
			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(CMRCusEntryCPDecCollection); }));

			//PackingGroup should not affect merge as LineNumber which is sent to Customs is no longer assiged during merge
			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(BasePackingGroup); }));
			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(BasePackingGroupCollection); }));
			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(BaseDeclarationLevelPackingGroupCollection); }));

			Assert(result.Exists((HasChangesHunterExclusionDetails details) =>
			{ return details.TypeToExclude == typeof(BaseCusContainer); }));
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var result = base.GetJobDeclaration();
			result.DisableDefaultPackingInformation = true;
			result.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		//protected override BaseJobDeclaration ImportJobDeclaration
		//{
		//	get
		//	{
		//		var result = GetJobDeclaration();

		//		result.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
		//		return result;
		//	}
		//}
	}
}
