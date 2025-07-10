using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(InvoiceForm))]
	public class ARInvoiceFormTest : InvoiceFormTest
	{
		#region Implementation

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return invoice is Invoice ? new InvoiceForm(invoice) { ControllerID = ControllerIDs.ARInvoice } :
														new CreditNoteForm(invoice) { ControllerID = ControllerIDs.ARCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<ARInvoice>() : Factory.New<ARInvoice>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return true; }
		}

		public override void TestOriginalInvoiceReferenceNumberForAmendVisibility()
		{
			var invoice = GetInvoiceWithValidTestData();
			using (var form = GetFormByInvoice(invoice))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, false, false);
			}

			var original = invoice as IAmending;
			var amendingAR = original.GenerateAmendingTransaction(invoice.AH_TransactionType) as ARInvoice;
			using (var form = GetFormByInvoice(amendingAR))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, true, true);
			}
		}

		public override void TestPromptToPrintComplianceDocumentWhenEnablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(true, AssertForPromptToPrintComplianceDocument);
		}

		public override void TestPromptToPrintComplianceDocumentWhenDisablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(false, AssertForNotPromptToPrintComplianceDocument);
		}
		#endregion

		public void TestWhenComplianceSequenceFailedToAssign()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var creator = new TestObjectCreator(Factory);
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";
			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = creator.ABIGAS.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice1.Lines.AddNew();
			var chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;
			line.AL_GovtChargeCode = "AAA";

			invoice1.RunPreSaveValidation();
			AssertNoErrors("Precondition", invoice1);

			using (InvoiceForm form = new InvoiceForm(invoice1))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = creator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 2, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			ARInvoice invoice2 = creator.CreateARInvoice<ARInvoice>("ARInv2", creator.AUD, 1m, creator.AALSHI);
			invoice2.AH_OH = creator.ABIGAS.PK;
			line = (ARInvoiceLine)invoice2.Lines.AddNew();
			chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;
			line.AL_GovtChargeCode = "BBB";

			invoice2.RunPreSaveValidation();
			AssertNoErrors("Precondition", invoice2);

			using (InvoiceForm form = new InvoiceForm(invoice2))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Branch or Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please configure appropriate Compliance Invoice Books for your Login Company, Transaction Header Branch or Transaction Header Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"));
				AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2018, 10, 10)]
		public void TestWhenDigitalSignatureFailedToSign()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var creator = new TestObjectCreator(Factory);
			var group = Factory.New<GlbGroup>();
			var currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";

			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Portugal);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Portugal;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var sequence = creator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 1);
			sequence.XD_Prefix = "0102";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			AddPeriodTimeForUNLOCOUtcOffset();
			Factory.Save();

			var invoice1 = creator.CreateARInvoice<ARInvoice>("ARInv1", creator.AUD, 1m, creator.Debtor);
			var line = (ARInvoiceLine)invoice1.Lines.AddNew();
			var chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;

			using (var form = new InvoiceForm(invoice1))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence."));
				AssertEquals("Email should NOT be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			// simulate the case previous invoice cannot be found
			invoice1.AH_TransactionReference = ZString.Empty;
			Factory.Save();

			var invoice2 = creator.CreateARInvoice<ARInvoice>("ARInv2", creator.AUD, 1m, creator.Debtor);
			invoice2.AH_PostDate = invoice1.AH_PostDate.AddHours(1);
			var line2 = (ARInvoiceLine)invoice2.Lines.AddNew();
			chargeList = line2.ChargeList;
			chargeList.Load();
			line2.GenericCharge = chargeList[0].PK;
			line2.AL_OSExTaxAmount = 10m;
			line2.AL_AT = creator.GSTFREE1.PK;

			using (var form = new InvoiceForm(invoice2))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestSetupReceiptPaymentPanel()
		{
			using (InvoiceForm form = (InvoiceForm)GetFormToBashCore())
			{
				form.Show();
				UserIdleWorker.Flush();

				AssertEquals("Controls.Count", 1, form.ReceiptPaymentPanel_ForTestOnly.Controls.Count);
				AssertEquals("Type of Control", typeof(InvoiceReceiptUserControl), form.ReceiptPaymentPanel_ForTestOnly.Controls[0].GetType());
				InvoiceReceiptUserControl control = form.ReceiptPaymentPanel_ForTestOnly.Controls[0] as InvoiceReceiptUserControl;
				control.Parent.Visible = true; // simulate when binding will start
				AssertEquals(control.ReceiptPaymentAH_InvoiceDateEdit.CaptionResourceString.Caption, "Receipt Date");
				Assert("DataBindings.Count", control.ReceiptPaymentAH_ReceiptTypeDropEdit.DataBindings.Count > 0);
			}
		}

		public void TestPromptToPrintReversingInvoice()
		{
			RunTestPromptToPrintReversingInvoice(true);
		}

		public void TestPromptToPrintReversingInvoice_UnsuccessfulSave()
		{
			RunTestPromptToPrintReversingInvoice(false);
		}

		void RunTestPromptToPrintReversingInvoice(bool testSuccessfulSave)
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)aRCrd.Lines.AddNew();
			aRCrdLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.FillWithValidTestData();
			aRCrd.AH_OH = organisation.PK;
			aRCrdLine.AL_OSExTaxAmount = 10m;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(aRCrd))
			{
				if (!testSuccessfulSave)
				{
					form.BusinessEntity.Factory.Saving += f =>
					{
						throw new JobCreationException("Test Exception");
					};
				}

				AssertEquals("IsPostOnly", true, form.IsPostOnly);
				form.FReversingReason_ForTestOnly = "Because we want to reverse";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Delete_ForTestOnly();

				if (testSuccessfulSave)
				{
					Assert("User should be prompted to print reversing invoice", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("User should be prompted to print invoice", "Do you want to print invoice 00001000?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals("Mutex error shown", "Test Exception", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPromptToPrintReversingUAInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			UAInvoice uAInv = Factory.NewWithValidTestData<UAInvoice>();
			UAInvoiceLine uAInvLine = (UAInvoiceLine)uAInv.Lines.AddNew();
			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.FillWithValidTestData();
			uAInv.AH_OH = organisation.PK;
			uAInvLine.AL_OSExTaxAmount = 10m;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.UAInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(uAInv))
			{
				AssertEquals("IsPostOnly", true, form.IsPostOnly);
				form.FReversingReason_ForTestOnly = "Because we want to reverse";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Delete_ForTestOnly();
				Assert("User should not be prompted to print reversing invoice", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestPromptToPrintCancellingIncompleteInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SaveAsIncomplete();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APIncompleteInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(invoice))
			{
				form.CancelInsteadOfDelete = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Delete_ForTestOnly();
				Assert("User should not be prompted to print reversing invoice", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestPromptToPrintNewInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.ABIGAS;

			// new invoice should also prompt
			ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				ARInvoice aRInv = (ARInvoice)form.BusinessEntity;
				aRInv.FillWithValidTestData();
				aRInv.AH_OH = organisation.PK;
				ARInvoiceLine aRInvLine = (ARInvoiceLine)aRInv.Lines.AddNew();
				var chargeList = aRInvLine.ChargeList;
				chargeList.Load();
				aRInvLine.GenericCharge = chargeList[0].PK;
				aRInvLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print invoice", "Do you want to print invoice 00001000?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aRInv.AH_Desc += "TEST";
				form.ValidateAndSave_ForTestOnly();
				AssertEquals("User should not be prompted to print invoice when saving existing invoice", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptsToPrintCopiedInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.FillWithValidTestData();
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = organisation.PK;
			ARInvoiceLine aRInvLine = (ARInvoiceLine)aRInv.Lines.AddNew();
			var chargeList = aRInvLine.ChargeList;
			chargeList.Load();
			aRInvLine.GenericCharge = chargeList[0].PK;
			aRInvLine.AL_OSExTaxAmount = 10m;
			aRInvLine.AL_AT = testObjectCreator.GSTFREE1.PK;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowTemplateCopyForm(aRInv))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print invoice", "Do you want to print invoice 00001001?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSourceReferenceMutexIsUnlockedWhenFormIsClosed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();

				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice.SourceReference = "FTM a10";
					var invoice1Mutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice.Factory).GetSourceReferenceMutex(invoice.Company.GC_Code, invoice.SourceReference);
					invoice.RunPreSaveValidation();
					Assert(invoice1Mutex.IsLocked);
					Assert(invoice1Mutex.HasLock);

					form.Close();
					Assert(!invoice1Mutex.IsLocked);
				}
			}
		}

		public void TestEDIMessagesPluginAttached()
		{
			using (InvoiceForm form = (InvoiceForm)GetFormToBashCore())
			{
				Assert("Form should have the plugin attached", form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage) != null);
			}
		}

		void AddPeriodTimeForUNLOCOUtcOffset()
		{
			var createUtcTime = ZDateTime.Now;
			Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES ('{ZGuid.NewZGuid()}', 
				'{GlbBranch.CurrentBranch.GB_RL_NKHomePort}', '{createUtcTime.AddMonths(-3)}', '{createUtcTime.AddMonths(3)}', 60)");
		}

		public override void TestExtendDropEdit_AH_Calc_AmendStatusCode()
		{
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertNormalAR();
				AssertAmendingAR_Default();

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertNormalAR();
				AssertAmendingAR_Default();
			}

			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertNormalAR();
				AssertAmendingAR_Default();

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertNormalAR();
				AssertAmendingAR_EnableStatusCode();
			}

			void AssertNormalAR()
			{
				var invoice = GetInvoiceWithValidTestData();
				AssertEquals("PreCondition", false, invoice.IsAmendingTransaction);
				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();
					AssertExtendDropEditDefault(form);
				}
			}

			void AssertAmendingAR_Default()
			{
				using (var form = GetFormByInvoice(CreateAmendAR()))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();

					AssertExtendDropEditDefault(form);
				}
			}

			void AssertAmendingAR_EnableStatusCode()
			{
				using (var form = GetFormByInvoice(CreateAmendAR()))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();

					AssertExtendDropEditStatusCode(form);
					var extendDropEdit = form.InvoiceDetails.ExtendDropEdit_ForTestOnly;
					AssertEquals("Status Code selected index", -1, extendDropEdit.SelectedIndex);

					form.Invoice_ForTestOnly.AH_Calc_AmendStatusCode = "01";
					AssertEquals("Status Code should be synchronized", "01", extendDropEdit.Text);
				}
			}

			ARInvoice CreateAmendAR()
			{
				var invoice = GetInvoiceWithValidTestData();
				var original = invoice as IAmending;
				var amendingAR = original.GenerateAmendingTransaction(invoice.AH_TransactionType) as ARInvoice;
				AssertEquals("PreCondition", true, amendingAR.IsAmendingTransaction);

				return amendingAR;
			}
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithValidLedgerWhenInvoiceLedgerIsValid()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Invoice", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceCompany()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Invoice", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceFactory()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Invoice", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsAllowed_WhenNoTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Invoice", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsNotAllowed_WhenTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Invoice", true);
		}
	}
}
