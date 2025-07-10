using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.JobInvoicing.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	public abstract class BaseBatchInvoicingPostManagerGUIWrapperTest : PostManagerGUIWrapperTest
	{
		[TestDate(2005, 9, 10)]
		public void TestPostingChecksLevelAuthorization()
		{
			SetupDataForAuthorizationApproval();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wrapper = GetGuiWrapperWithOneObjectToPost("");
			wrapper.ShowLoginFormForTest = true;
			wrapper.Post();

			AssertEquals("Posting should be cancelled as user does not have authorization", true, wrapper.PostManager_ForTestOnly.CancelPosting);
			Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.IsInDatabaseIncludingChildren);
			AssertNull("Should not prompt any form", ZFormModaliser.LastFormShownDialogForTest);

			AssertContains(wrapper.PostedObjectName + " posting failed.", wrapper.CurrentObjectMessageStack_ForTestOnly);
			AssertContains("You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.", wrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		[TestDate(2005, 9, 10)]
		public void TestPost()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			try
			{
				JobA.Charges.RemoveAndDeleteAll();
				Factory.Save();

				ZDateTime now = ZDateTime.Now;
				Wrapper.Post();

				Assert("Should have an invoice", Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count > 0);
				AssertEquals("Date should be current date", now.Date, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_InvoiceDate);
				Assert("The object should be posted successfully", Wrapper.ObjectPosted);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestPost2Jobs()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			try
			{
				Wrapper.Post();

				AssertEquals("Should have 1 Invoice", 1, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Invoice Date should be current date", ZDateTime.Now.Date, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_InvoiceDate.Date);
				Assert("The object should be posted succesfully", Wrapper.ObjectPosted);

				var query = new ZQuery(AccTransactionHeaderSchema.AH_JH, JobA.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobA.JH_GC);
				ARInvoice[] jobAInvoices = Factory.Load<ARInvoice>(query);
				AssertEquals("There should be one Invoice created for JobA", 1, jobAInvoices.Length);

				query = new ZQuery(AccTransactionHeaderSchema.AH_JH, JobB.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobB.JH_GC);
				ARInvoice[] jobBInvoices = Factory.Load<ARInvoice>(query);
				AssertEquals("There should be one Invoice created for JobB", 1, jobBInvoices.Length);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		[TestDate(2005, 9, 10)]
		public void TestPostAndAutoAllocate2Jobs()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_BackDateInvoicesFalse);
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				SetupJobData_AutoAllocationTest();

				Wrapper.Post();

				Assert("The object should be posted succesfully", Wrapper.ObjectPosted);

				Factory.Save();

				AccTransactionHeaderCollection postedTransactions = new AccTransactionHeaderCollection(Factory);
				postedTransactions.Load(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should have created 2 payments", 2, postedTransactions.Count);

				AccTransactionHeader payment1 = GetPaymentFromCollection(postedTransactions, "5");
				AssertNotNull("Payment should exist", payment1);
				AccTransactionHeader payment2 = GetPaymentFromCollection(postedTransactions, "3");
				AssertNotNull("Payment should exist", payment2);

				ChargeA.Reload();
				ChargeB.Reload();
				AssertEquals("Charge should have its cheque No set", "5", ChargeA.JR_ChequeNo);
				AssertEquals("Charge2 should have its cheque No set", "3", ChargeB.JR_ChequeNo);

				PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator test_AllocatorOfPayment1;
				PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator test_AllocatorOfPayment2;
				if (Wrapper.Test_AllocatorsList_ForTestOnly[0].CollectionsPassedForPrinting[0].First().AH_ChequeOrReference == "5")
				{
					test_AllocatorOfPayment1 = Wrapper.Test_AllocatorsList_ForTestOnly[0];
					test_AllocatorOfPayment2 = Wrapper.Test_AllocatorsList_ForTestOnly[1];
				}
				else
				{
					test_AllocatorOfPayment2 = Wrapper.Test_AllocatorsList_ForTestOnly[0];
					test_AllocatorOfPayment1 = Wrapper.Test_AllocatorsList_ForTestOnly[1];
				}

				Assert("Auto printing should be performed", test_AllocatorOfPayment1.ChequeWasAutoPrinted);
				Assert("Auto printing should be performed", test_AllocatorOfPayment2.ChequeWasAutoPrinted);
				AssertEquals("Printing called only once", 1, test_AllocatorOfPayment1.PrintingCalled_Counter);
				AssertEquals("Printing called only once", 1, test_AllocatorOfPayment2.PrintingCalled_Counter);

				AssertEquals("There should be 1 printer passed for printing", 1, test_AllocatorOfPayment1.PrintersPassedForAutoPrinting.Count);
				Assert("Printer #1", test_AllocatorOfPayment1.PrintersPassedForAutoPrinting.Contains(AutoAllocateChequeBook.AK_SQ));
				AssertEquals("There should be 1 printer passed for printing", 1, test_AllocatorOfPayment2.PrintersPassedForAutoPrinting.Count);
				Assert("Printer #2", test_AllocatorOfPayment2.PrintersPassedForAutoPrinting.Contains(AutoAllocateChequeBook2.AK_SQ));
				AssertEquals("There should be 1 collection of payments passed for autoprinting", 1, test_AllocatorOfPayment1.CollectionsPassedForPrinting.Count);
				AssertEquals("There should be only 1 payment in collection 1", 1, test_AllocatorOfPayment1.CollectionsPassedForPrinting[0].Count());
				Assert("Collection should contain the payment1", test_AllocatorOfPayment1.CollectionsPassedForPrinting[0].Any(x => x.PK == payment1.PK));
				AssertEquals("There should be 1 collection of payments passed for autoprinting", 1, test_AllocatorOfPayment2.CollectionsPassedForPrinting.Count);
				AssertEquals("There should be only 1 payment in collection 1", 1, test_AllocatorOfPayment2.CollectionsPassedForPrinting[0].Count());
				Assert("Collection should contain the payment2", test_AllocatorOfPayment2.CollectionsPassedForPrinting[0].Any(x => x.PK == payment2.PK));

				AutoAllocateChequeBook.Reload();
				AutoAllocateChequeBook2.Reload();
				AssertEquals("CurrentNo should change on AutoAllocateChequeBook", 6m, AutoAllocateChequeBook.AK_CurrentNo);
				Assert("AutoAllocateChequeBook should become to be inactive", !AutoAllocateChequeBook.AK_IsActive);
				AssertEquals("CurrentNo should change on AutoAllocateChequeBook2", 4m, AutoAllocateChequeBook2.AK_CurrentNo);
				Assert("AutoAllocateChequeBook is still active", AutoAllocateChequeBook2.AK_IsActive);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		#region BackDateARInvoices

		[TestDate(2005, 9, 10)]
		public void TestBackDateARInvoicesBatchWithRegistryEnabledForOverridePostDate()
		{
			BackDateInvoicesConfiguration oldAllowBackDating = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			try
			{
				ZDateTime postDate = ZDateTime.Today.AddDays(-2);
				AssertEquals("There should be 2 objects to be posted", 2, Wrapper.NumberOfObjectsToPost);
				InitializeBackDateARInvoiceWithBackDateDialogResultYesToAll(Wrapper, ZDateTime.Empty, postDate);

				Wrapper.Post();

				AssertEquals("Should have 1 Invoice", 1, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.Count);
				AssertEquals("Post Date should be as expected", postDate, Wrapper.PostManager_ForTestOnly.Poster.PostedInvoices[0].AH_PostDate.Date);
				Assert("The object should be posted succesfully", Wrapper.ObjectPosted);

				var query = new ZQuery(AccTransactionHeaderSchema.AH_JH, JobA.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobA.JH_GC);
				ARInvoice[] jobAInvoices = Factory.Load<ARInvoice>(query);
				AssertEquals("There should be one Invoice created for JobA", 1, jobAInvoices.Length);
				AssertEquals("Post Date should be as expected", postDate, jobAInvoices[0].AH_PostDate.Date);

				query = new ZQuery(AccTransactionHeaderSchema.AH_JH, JobB.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, JobB.JH_GC);
				ARInvoice[] jobBInvoices = Factory.Load<ARInvoice>(query);
				AssertEquals("There should be one Invoice created for JobB", 1, jobBInvoices.Length);
				AssertEquals("Post Date should be as expected", postDate, jobBInvoices[0].AH_PostDate.Date);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAllowBackDating);
			}
		}

		class NotificationSubscriberGuiHelperForTest : ChangeTransactionDatesNotificationSubscriberGuiHelper
		{
			public NotificationSubscriberGuiHelperForTest(YesNoYesAllNoAllMessageBoxResult shouldBackDateDialogResult)
				: base()
			{
				this.ShouldBackDateDialogResult = shouldBackDateDialogResult;
			}

			protected override ChangeTransactionDatesMessageBox GetNewMessageBox()
			{
				if (!InvoiceDateForTest.IsEmpty &&
					ChangeTransactionDateBusinessObject != null &&
					!ChangeTransactionDateBusinessObject.InvoiceDateInfo.ReadOnly)
				{
					ChangeTransactionDateBusinessObject.InvoiceDate = InvoiceDateForTest;
				}

				if (!PostDateForTest.IsEmpty &&
					ChangeTransactionDateBusinessObject != null &&
					!ChangeTransactionDateBusinessObject.PostDateInfo.ReadOnly)
				{
					ChangeTransactionDateBusinessObject.PostDate = PostDateForTest;
				}

				ChangeTransactionDatesMessageBox messageBox = base.GetNewMessageBox();
				messageBox.YesNoAllResult = ShouldBackDateDialogResult;
				return messageBox;
			}

			readonly YesNoYesAllNoAllMessageBoxResult ShouldBackDateDialogResult;
			internal ZDateTime InvoiceDateForTest;
			internal ZDateTime PostDateForTest;
		}

		protected override void InitializeBackDateARInvoiceWithBackDateDialogResultYes()
		{
			NotificationSubscriberGuiHelperForTest guiHelper = new NotificationSubscriberGuiHelperForTest(YesNoYesAllNoAllMessageBoxResult.YesToAll);
			((BaseBatchInvoicingPostManagerGUIWrapper)GUIWrapper).NotificationHelper_ForTestOnly = guiHelper;
		}

		protected void InitializeBackDateARInvoiceWithBackDateDialogResultYesToAll(BaseBatchInvoicingPostManagerGUIWrapper wrapper, ZDateTime invoiceDate, ZDateTime postDate)
		{
			NotificationSubscriberGuiHelperForTest guiHelper = new NotificationSubscriberGuiHelperForTest(YesNoYesAllNoAllMessageBoxResult.YesToAll);
			if (!invoiceDate.IsEmpty)
			{
				guiHelper.InvoiceDateForTest = invoiceDate;
			}

			if (!postDate.IsEmpty)
			{
				guiHelper.PostDateForTest = postDate;
			}
			wrapper.NotificationHelper_ForTestOnly = guiHelper;
		}

		protected override void InitializeBackDateARInvoiceWithBackDateDialogResultNo()
		{
			NotificationSubscriberGuiHelperForTest guiHelper = new NotificationSubscriberGuiHelperForTest(YesNoYesAllNoAllMessageBoxResult.NoToAll);
			((BaseBatchInvoicingPostManagerGUIWrapper)GUIWrapper).NotificationHelper_ForTestOnly = guiHelper;
		}

		public new void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringCancel()
		{
			Assert("Batch Posting can't be canceled .", true);
		}

		protected override void SetupJobDataForBackDateARInvoiceTests()
		{
			//don't need any special setup
		}

		protected override void SetupJobDataForAPInvoiceTests()
		{
			//don't need any special setup
		}

		protected override SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.BulkModifyTransactionDate); }
		}

		protected override SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.BulkModifyPostDate); }
		}

		protected override SecurityCheckpoint OverrideRequisitionDetailsSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.OverrideRequisitionDetails); }
		}

		protected override void SetShipmentDate(ZDateTime date)
		{
			((ForwardingShipment)JobA.PlugInData).JS_E_ARV = date;
		}

		protected override void SetInvoiceTerms(ZString term, ZByte termDays)
		{
			TestObjectCreator.ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = term;
			TestObjectCreator.ABIGAS.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = termDays;
		}

		#endregion

		public void TestAppendMessageToStack()
		{
			ZString testMessage = "This is a test message";
			Wrapper.AppendMessageToStack_ForTestOnly(testMessage);
			AssertContains("Message should be added to CurrentObjectMessageStack_ForTestOnly", testMessage, Wrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		#region NothingPostedHandler

		protected override PostManagerGUIWrapper PrepareTestDataForNothingPostedHandler()
		{
			Action<Charge> clearInvoiceFields = x =>
			{
				x.JR_OH_CostAccount = ZGuid.Empty;
				x.JR_OH_SellAccount = ZGuid.Empty;
				x.JR_APInvoiceNum = "";
				x.JR_APInvoiceDate = ZDateTime.Empty;
				x.JR_PaymentDate = ZDateTime.Empty;
			};
			JobA.Charges.Cast<Charge>().ToList().ForEach(clearInvoiceFields);
			JobB.Charges.Cast<Charge>().ToList().ForEach(clearInvoiceFields);
			Factory.Save();

			return Wrapper;
		}

		protected override void AssertNothingPostedHandlerCore(PostManagerGUIWrapper wrapper)
		{
			var batchWrapper = wrapper as BaseBatchInvoicingPostManagerGUIWrapper;
			AssertNotNull("Wrapper should be BaseBatchInvoicingPostManagerGUIWrapper", batchWrapper);

			batchWrapper.Post();

			AssertEquals("Current Object Posted", false, batchWrapper.CurrentObjectPosted_ForTestOnly);
			AssertContains(batchWrapper.PostedObjectName + " posting failed.", batchWrapper.CurrentObjectMessageStack_ForTestOnly);
			var expectedMessage = GetExpectedMessageForNothingPosted();
			AssertContains("Default Message for nothing posted event", expectedMessage, batchWrapper.GetNothingPostedMessage_ForTestOnly());
			AssertContains("The message should be set correctly", expectedMessage, batchWrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		#endregion

		public void TestGetJobOnHoldMessage()
		{
			JobA.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			JobB.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Factory.Save();

			Wrapper.Post();

			var expectedMessage = GetExpectedMessageForJobOnHold();
			AssertContains("The message should be set correctly", expectedMessage, Wrapper.CurrentObjectMessageStack_ForTestOnly);
			AssertEquals("CurrentObjectPosted_ForTestOnly should be set to False", false, Wrapper.CurrentObjectPosted_ForTestOnly);
		}

		protected abstract ZString GetExpectedMessageForJobOnHold();

		public void TestOnPrepostValidationFailed()
		{
			JobA.Charges.RemoveAndDeleteAll();
			JobB.Charges.RemoveAndDeleteAll();
			Factory.Save();

			Wrapper.Post();
			AssertContains("Validation failed:", Wrapper.PostedObjectName + " posting failed.", Wrapper.CurrentObjectMessageStack_ForTestOnly);
			AssertEquals("CurrentObjectPosted_ForTestOnly should be set to False", false, Wrapper.CurrentObjectPosted_ForTestOnly);
		}

		public void TestJobsNotPostedWhenDebtorIsNotValid()
		{
			var debtor = TestObjectCreator.CreateOrgHeader("TESTORG", false, true);

			Consol1 = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001003");
			Consol2 = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001004");

			Shipment1 = TestObjectCreator.CreateShipment("S00001003", Consol1);
			Shipment2 = TestObjectCreator.CreateShipment("S00001004", Consol2);

			var jobA = CreateJob(Shipment1, debtor);
			var jobB = CreateJob(Shipment2, debtor);

			debtor.OH_IsDebtor = false;

			var chargeA = CreateCharge(jobA, TestObjectCreator.CC1, "Description", AUD, 100m, TestObjectCreator.AALSHI, AUD, 150m, debtor);
			chargeA.JR_APInvoiceNum = "INV12";
			chargeA.JR_APInvoiceDate = ZDateTime.Today;
			chargeA.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var chargeB = CreateCharge(jobB, TestObjectCreator.CC1, "Description", AUD, 200m, TestObjectCreator.AALSHI, AUD, 250m, debtor);
			chargeB.JR_APInvoiceNum = "INV23";
			chargeB.JR_APInvoiceDate = ZDateTime.Today;
			chargeB.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var jobCollection = new JobCollection(Factory);
			jobCollection.AddRange(jobA, jobB);

			Factory.Save();

			var wrapper = GetGuiWrapperWithTwoObjectsToPost("Shipments", debtor.OH_IsDebtor);
			wrapper.Post();
			AssertContains("Validation failed:", wrapper.PostedObjectName + " posting failed.", wrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		public void TestMessageAddedOnCreditLimitExceeded()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0100001", TestObjectCreator.AUD, 1m, 50m, 0m, 50m, 0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditLimit = 5m;
			OrgARTerms term = TestObjectCreator.ABIGAS.CompanyData.ARTerms[0];
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.AddRange(JobA, JobB);
			Factory.Save();
			Wrapper.Post();
			AssertContains("which is over the credit limit.", Wrapper.CurrentObjectMessageStack_ForTestOnly);
		}

		public void TestSetParentFormForPrinting()
		{
			using (ZForm testForm = new ZForm())
			{
				Wrapper.ParentForm = testForm;
				AssertEquals("Wrapper.ParentForm should be the same as TestForm", testForm, Wrapper.ParentForm);
				Assert("Wrapper.ParentForm should be the same as TestForm", object.ReferenceEquals(testForm, Wrapper.ParentForm));
			}
		}

		public void TestNumberOfObjectsToPost()
		{
			Wrapper = GetGuiWrapperWithOneObjectToPost("");
			AssertEquals("There should be 1 object to be posted", 1, Wrapper.NumberOfObjectsToPost);

			Wrapper = GetGuiWrapperWithTwoObjectsToPost("");
			AssertEquals("There should be 2 objects to be posted", 2, Wrapper.NumberOfObjectsToPost);
		}

		protected abstract BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithTwoObjectsToPost(string postedBusinessObjectName, bool isDebtorValid = true);

		protected abstract BaseBatchInvoicingPostManagerGUIWrapper GetGuiWrapperWithOneObjectToPost(string postedBusinessObjectName);

		public abstract void TestConstructor();

		public abstract void TestCurrentObjectCode();

		public abstract void TestDefaultPostedObjectName();

		public override void TestGetParentForPostingAction()
		{
			SetupDataForAuthorizationApproval();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var wrapper = GetGuiWrapperWithOneObjectToPost("");
			wrapper.ShowLoginFormForTest = true;
			wrapper.Post();

			AssertEquals("Posting should be cancelled", true, wrapper.PostManager_ForTestOnly.CancelPosting);
			Assert("Should not create any invoices", !wrapper.PostManager_ForTestOnly.Poster.PostedInvoices.IsInDatabaseIncludingChildren);

			var parent = wrapper.GetParentInfoForPostingAction_ForTestOnly();
			var parentId = parent.Id;
			var parentTableCode = parent.TableCode;

			Assert("Parent id is set", !parentId.IsEmpty);
			AssertNotNull("Parent table code is not null", parentTableCode);
			AssertParentIDIsSetToParentJob("Parent id is set to correct job", parentId);
			AssertParentTableCode("Parent table code is set to appropriate value", parentTableCode);
		}

		protected virtual void AssertParentIDIsSetToParentJob(ZString description, ZGuid parentId)
		{
		}

		protected virtual void AssertParentTableCode(ZString description, ZString actualTableCode)
		{
		}

		void SetupDataForAuthorizationApproval()
		{
			Enterprise.Environment.Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Enterprise.Environment.Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200M;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200M;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);

			JobA.Charges.RemoveAndDeleteAll();
			Charge charge = JobA.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC3.PK;
			charge.JR_OSSellAmt = -200M;
			charge.JR_OSCostAmt = 0;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			JobB.Charges.RemoveAndDeleteAll();
			charge = JobB.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC3.PK;
			charge.JR_OSSellAmt = -200M;
			charge.JR_OSCostAmt = 0;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(2005);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(2006);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(2007);

			SetupTestData();
			Wrapper = GetGuiWrapperWithTwoObjectsToPost("Shipments");
		}

		protected BaseBatchInvoicingPostManagerGUIWrapper Wrapper;

		protected override PostManagerGUIWrapper GUIWrapper
		{
			get { return GUIWrapper_innerValue ?? (GUIWrapper_innerValue = GetGuiWrapperWithOneObjectToPost("Shipments")); }
		}
		PostManagerGUIWrapper GUIWrapper_innerValue;

		protected ForwardingConsol ConsolA;
		protected ForwardingConsol ConsolB;
		protected ForwardingShipment ShipmentA;
		protected ForwardingShipment ShipmentB;
		protected Job JobA;
		protected Job JobB;
		protected Charge ChargeA;
		protected Charge ChargeB;
		protected AccChequeBook AutoAllocateChequeBook;
		protected AccChequeBook AutoAllocateChequeBook2;

		protected ForwardingConsol Consol1;
		protected ForwardingConsol Consol2;
		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;

		protected void SetupTestData()
		{
			SetupInvoiceRollupForTest(TestObjectCreator.ABIGAS);

			ConsolA = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001001");
			ConsolB = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001002");

			ShipmentA = TestObjectCreator.CreateShipment("S00001001", "", "", ConsolA);
			ShipmentB = TestObjectCreator.CreateShipment("S00001002", "", "", ConsolB);

			JobA = CreateJob(ShipmentA, TestObjectCreator.ABIGAS);
			JobB = CreateJob(ShipmentB, TestObjectCreator.ABIGAS);

			ChargeA = CreateCharge(JobA, TestObjectCreator.CC1, "Description", AUD, 100m, TestObjectCreator.AALSHI, AUD, 150m, TestObjectCreator.ABIGAS);
			ChargeA.JR_APInvoiceNum = "INV11";
			ChargeA.JR_APInvoiceDate = ZDateTime.Today;
			ChargeA.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			ChargeB = CreateCharge(JobB, TestObjectCreator.CC1, "Description", AUD, 200m, TestObjectCreator.AALSHI, AUD, 250m, TestObjectCreator.ABIGAS);
			ChargeB.JR_APInvoiceNum = "INV22";
			ChargeB.JR_APInvoiceDate = ZDateTime.Today;
			ChargeB.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();
		}

		protected void SetupJobData_AutoAllocationTest_Core()
		{
			JobA.AgentCollectPK = TestObjectCreator.Agent.PK;
			JobA.JH_LocalChargesCFX = 1M;
			JobA.JH_AgentChargesCFX = 1M;

			JobB.AgentCollectPK = TestObjectCreator.Agent.PK;
			JobB.JH_LocalChargesCFX = 1M;
			JobB.JH_AgentChargesCFX = 1M;

			ChargeA.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			ChargeB.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;

			SetAPInvoiceInfo(ChargeA, "1", ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(20));
			SetAPPaymentInfo(ChargeA, ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, "", AutoAllocateChequeBook);

			SetAPInvoiceInfo(ChargeB, "2", ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(20));
			SetAPPaymentInfo(ChargeB, ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, "", AutoAllocateChequeBook2);

			Factory.Save();
		}

		protected new void SetupJobData_AutoAllocationTest(string chequeBookDesc = null)
		{
			AutoAllocateChequeBook = GetAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, 1, 5, 5, "Chk1");
			if (!string.IsNullOrEmpty(chequeBookDesc))
			{
				AutoAllocateChequeBook.AK_Desc = chequeBookDesc;
			}

			AutoAllocateChequeBook2 = GetAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, 1, 4, 3, "Chk2");
			ChargeA.JR_AK = AutoAllocateChequeBook.PK;
			ChargeB.JR_AK = AutoAllocateChequeBook2.PK;
			Factory.Save();
			SetupJobData_AutoAllocationTest_Core();
		}

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bank, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO, ZString aK_Code)
		{
			bank.AB_ChequeNumDigits = 1;
			bank.AB_SO_ChequeTemplate = Enterprise.Accounting.Business.TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bank.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			chequeBook.AK_Code = aK_Code;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		AccTransactionHeader GetPaymentFromCollection(AccTransactionHeaderCollection collection, ZString chequeNo)
		{
			foreach (AccTransactionHeader header in collection)
			{
				if (header.AH_ChequeOrReference == chequeNo)
				{
					return header;
				}
			}
			return null;
		}

		Job CreateJob(ForwardingShipment shipment, OrgHeader localClient)
		{
			Job newJob = TestObjectCreator.CreateJob(shipment);
			newJob.JH_GB = GlbBranch.CurrentBranch.PK;
			newJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			newJob.LocalChargesPK = localClient != null ? localClient.PK : ZGuid.Empty;
			return newJob;
		}

		void SetupInvoiceRollupForTest(OrgHeader organisation)
		{
			organisation.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = organisation.CompanyData.InvoiceRollupOrGroups.AddNew();

			group.PG_JobType = (ZString)OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = (ZString)OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = (ZString)OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
		}

		#endregion
	}
}
