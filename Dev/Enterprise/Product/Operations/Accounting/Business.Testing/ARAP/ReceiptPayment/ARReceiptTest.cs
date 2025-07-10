using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARReceipt))]
	public class ARReceiptTest : ReceiptTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARReceipt>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override string ExceptedWorkflowType => WorkflowDescriptors.ARReceiptWorkflowDescriptorCode;

		public override void TestDefaultDescription()
		{
			AssertEquals("Default description should be AR RECEIPT", "AR RECEIPT", ReceiptPaymentBase.AH_Desc);
		}

		public override void TestDefaultBankAccount()
		{
			DefaultBankAccountARTest();
		}

		public override void TestOrgHeaders()
		{
			OrgHeader aPOrg = Factory.NewWithValidTestData<OrgHeader>();
			aPOrg.OH_IsCreditor = true;
			aPOrg.OH_IsDebtor = false;

			OrgHeader aROrg = Factory.NewWithValidTestData<OrgHeader>();
			aROrg.OH_IsCreditor = false;
			aROrg.OH_IsDebtor = true;

			Factory.Save();

			ReceiptPaymentBase.Lookups.Headers.Load();
			Assert("Should contain the AROrg", ReceiptPaymentBase.Lookups.Headers.Contains(aROrg));
			Assert("Should not contain the APOrg", !ReceiptPaymentBase.Lookups.Headers.Contains(aPOrg));
		}

		public void TestOn_DisplayCollectionNotesForm()
		{
			TestARReceipt.On_DisplayCollectionNotesForm += new DisplayCollectionNotesFormEventHandler(TestARReceipt_On_DisplayCollectionNotesForm);

			OrgHeader organisationWithActiveCollectionNote = Factory.NewWithValidTestData<OrgHeader>();
			OrgCollectionNote activeCollectionNote = organisationWithActiveCollectionNote.CollectionNotes.AddNew();
			activeCollectionNote.PN_Status = CollectionNoteStatusList.Codes.Working;
			OrgCollectionNote closedCollectionNote = organisationWithActiveCollectionNote.CollectionNotes.AddNew();
			closedCollectionNote.PN_Status = CollectionNoteStatusList.Codes.Closed;
			Factory.Save();

			TestARReceipt.AH_OH = organisationWithActiveCollectionNote.PK;
			Assert("The form with OrgCollectionNotes should be opened", EventWasCalled);
			EventWasCalled = ZBool.False;

			OrgHeader organisationWithAllClosedCollectionNotes = Factory.NewWithValidTestData<OrgHeader>();
			OrgCollectionNote closedCollectionNote2 = organisationWithAllClosedCollectionNotes.CollectionNotes.AddNew();
			closedCollectionNote2.PN_Status = CollectionNoteStatusList.Codes.Closed;
			OrgCollectionNote closedCollectionNote3 = organisationWithAllClosedCollectionNotes.CollectionNotes.AddNew();
			closedCollectionNote3.PN_Status = CollectionNoteStatusList.Codes.Closed;
			Factory.Save();
			TestARReceipt.AH_OH = organisationWithAllClosedCollectionNotes.PK;
			Assert("The form with OrgCollectionNotes should not be opened", !EventWasCalled);
			EventWasCalled = ZBool.False;

			OrgHeader organisationWithoutAnyCollectionNotes = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			TestARReceipt.AH_OH = organisationWithoutAnyCollectionNotes.PK;
			Assert("The form with OrgCollectionNotes should not be opened", !EventWasCalled);
			EventWasCalled = ZBool.False;

			TestARReceipt.AH_OH = ZGuid.Empty;
			Assert("The form with OrgCollectionNotes should not be opened", !EventWasCalled);
			AssertNull("There should be none exceptions reported", ErrorReporter.LastExceptionReported);
		}

		public void TestOn_DisplayCollectionNotesFormIsNull()
		{
			OrgHeader organisationWithActiveCollectionNote = Factory.NewWithValidTestData<OrgHeader>();
			OrgCollectionNote activeCollectionNote = organisationWithActiveCollectionNote.CollectionNotes.AddNew();
			activeCollectionNote.PN_Status = CollectionNoteStatusList.Codes.Open;
			OrgCollectionNote closedCollectionNote = organisationWithActiveCollectionNote.CollectionNotes.AddNew();
			closedCollectionNote.PN_Status = CollectionNoteStatusList.Codes.Closed;
			Factory.Save();
			EventWasCalled = false;
			TestARReceipt.AH_OH = organisationWithActiveCollectionNote.PK;
			Assert("The form with OrgCollectionNotes should not be opened", !EventWasCalled);
			AssertNull("There should be none exceptions reported", ErrorReporter.LastExceptionReported);
		}

		public void TestSuspendSettingDefaultBankForOrganisation()
		{
			AccBankAccount newBank = Factory.NewWithValidTestData<AccBankAccount>();
			newBank.AB_IsDefaultReceiptBankAccount = ZBool.True;
			newBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestARReceipt.AH_OH = newOrganisation.PK;
			AssertEquals("Bank account should be defaulted", newBank.PK, TestARReceipt.AH_AB);

			TestARReceipt.AH_OH = ZGuid.Empty;
			TestARReceipt.AH_AB = ZGuid.Empty;
			TestARReceipt.SuspendSettingDefaultBankForOrganisation(ZBool.True);
			TestARReceipt.AH_OH = newOrganisation.PK;
			AssertEquals("Bank account should not be defaulted", ZGuid.Empty, TestARReceipt.AH_AB);

			TestARReceipt.AH_OH = ZGuid.Empty;
			TestARReceipt.SuspendSettingDefaultBankForOrganisation(ZBool.False);
			TestARReceipt.AH_OH = newOrganisation.PK;
			AssertEquals("Bank account should be defaulted", newBank.PK, TestARReceipt.AH_AB);
		}

		public void TestIncludeInDepositBatch_ReadOnly()
		{
			TestARReceipt.SetDefaultIncludeInDepositBatch(ZBool.True);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Property should not be read only yet", !TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Property should not be read only yet", !TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Assert("Property should not be read only yet", !TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
			TestARReceipt.SetDefaultIncludeInDepositBatch(ZBool.False);
			Assert("Property should become to be read only", TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
			TestARReceipt.SetDefaultIncludeInDepositBatch(ZBool.True);
			Assert("Property should not be read only", !TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			Assert("Property should become to be read only", TestARReceipt.IncludeInDepositBatchInfo.ReadOnly);
		}

		public void TestChangingAH_ReceiptTypeWillChangeIncludeInDepositBatch()
		{
			Assert("IncludeInDepositBatch should be False by default", !TestARReceipt.IncludeInDepositBatch);
			TestARReceipt.SetDefaultIncludeInDepositBatch(ZBool.True);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("IncludeInDepositBatch should be changed", TestARReceipt.IncludeInDepositBatch);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("IncludeInDepositBatch should be changed", !TestARReceipt.IncludeInDepositBatch);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("IncludeInDepositBatch should be changed", TestARReceipt.IncludeInDepositBatch);
			TestARReceipt.SetDefaultIncludeInDepositBatch(ZBool.False);
			TestARReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("IncludeInDepositBatch should be changed", !TestARReceipt.IncludeInDepositBatch);
		}

		public void TestParentCollection()
		{
			ARReceiptCollection testCollection = new ARReceiptCollection(Factory);
			ARReceipt testReceipt = Factory.New<ARReceipt>();
			AssertNull("There is no parent collection", testReceipt.GetParentCollection_ForTestOnly());
			testReceipt = testCollection.AddNew();
			AssertEquals("Parrent collection should be determined correctly", testCollection, testReceipt.GetParentCollection_ForTestOnly());
		}

		public void TestAH_OSExTaxAmount_SetterRaisesEventOnParent()
		{
			ARReceiptCollection testCollection = new ARReceiptCollection(Factory);
			testCollection.OnAmountOnChildChanged += new ARReceiptCollection.OnAmountOnChildChangedHandler(TestCollection_OnAmountOnChildChanged);
			ARReceipt testReceipt = testCollection.AddNew();
			Assert("Event should not be raised yet", !OnAmountOnChildChangedFired);
			testReceipt.AH_OSExTaxAmount = 100;
			Assert("Event should be raised", OnAmountOnChildChangedFired);
		}

		public void TestAH_InvoiceAmount_SetterRaisesEventOnParent()
		{
			ARReceiptCollection testCollection = new ARReceiptCollection(Factory);
			testCollection.OnAmountOnChildChanged += new ARReceiptCollection.OnAmountOnChildChangedHandler(TestCollection_OnAmountOnChildChanged);
			ARReceipt testReceipt = testCollection.AddNew();
			Assert("Event should not be raised yet", !OnAmountOnChildChangedFired);
			testReceipt.AH_InvoiceAmount = 100;
			Assert("Event should be raised", OnAmountOnChildChangedFired);
		}

		public void TestAH_ReceiptType_SetterRaisesEventOnParent()
		{
			ARReceiptCollection testCollection = new ARReceiptCollection(Factory);
			testCollection.OnReceiptTypeOnChildChanged += new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(TestCollection_OnReceiptTypeOnChildChanged);
			ARReceipt testReceipt = testCollection.AddNew();
			OnReceiptTypeOnChildChangedFired = ZBool.False;
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Event should be raised", OnReceiptTypeOnChildChangedFired);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RRC", ((IDocManagerSupport)Factory.New<ARReceipt>()).DocManagerInfo.DocManagerCode);
		}

		public void TestAH_InvoiceAmount()
		{
			var receipt = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			string GetCriticalInfo(ZGuid receiptPK) => infoCollector.GetInfo(receiptPK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved);

			AssertContains("Info should not be collected because AH_InvoiceAmount not change", "TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", GetCriticalInfo(receipt.PK));

			receipt.AH_InvoiceAmount = -200m;

			var info = GetCriticalInfo(receipt.PK);
			var expectedCallStackMsg = @"
TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved:
Invoice Amount was changed from -100 to -200.
Call stack:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.Accounting.Business.ARAP.ReceiptPayment.ARReceipt.";

			AssertContains("Invoice Amount was changed from -100 to -200.", info);
			AssertContains(expectedCallStackMsg, info);

			var link = Factory.New<AccTransactionMatchLink>();
			link.AP_MatchGroupNum = "M000111";
			link.AP_AH = receipt.PK;
			link.AP_MatchDate = new ZDateTime(2010, 3, 2);
			link.AP_Amount = -100m;

			//TODO: Hello, fellow developer! If this test is failing on Factory.Save below, uncomment the "DISABLE TRIGGER" command below and remove this TODO line.
			//HACK: Temporarily disable the trigger that prevents updates to AH_InvoiceAmount to test critical validation, another layer of protection. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");

			AssertExceptionThrown<OnSavingCriticalCheckException>(Factory.Save);
			ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedCallStackMsg);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[TestedType(typeof(ARReceipt))]
		public class ARReceiptMatchingTest : ReceiptPaymentBaseMatchingTest
		{
			protected override ReceiptPaymentBase GetNewReceiptPayment()
			{
				return Factory.New<ARReceipt>();
			}
		}

		#region Implementation

		ZBool EventWasCalled;

		void TestARReceipt_On_DisplayCollectionNotesForm()
		{
			EventWasCalled = ZBool.True;
		}

		ARReceipt TestARReceipt
		{
			get
			{
				return (ARReceipt)ReceiptPaymentBase;
			}
		}

		void TestCollection_OnAmountOnChildChanged()
		{
			OnAmountOnChildChangedFired = ZBool.True;
		}
		ZBool OnAmountOnChildChangedFired;

		void TestCollection_OnReceiptTypeOnChildChanged()
		{
			OnReceiptTypeOnChildChangedFired = ZBool.True;
		}
		ZBool OnReceiptTypeOnChildChangedFired;

		#endregion
	}
}
