using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CreditNoteForm))]
	public class ARCreditNoteFormTest : CreditNoteFormTest
	{
		public override void TestOriginalInvoiceReferenceNumberForAmendVisibility()
		{
			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData()))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, true, true);
			}
		}

		public void TestShowJobChargesForImportEvent_EnableNegativeAccrualBehaviors()
		{
			using (CreditNoteForm form = (CreditNoteForm)GetFormToBashCore())
			{
				form.Show();

				InvoicingBase creditNote = form.Invoice_ForTestOnly;

				if (creditNote is APCreditNote)
				{
					var creator = new TestObjectCreator(Factory);

					var line = (InvoicingLineBase)creditNote.Lines.AddNew();
					bool showJobChargesForImportEventWasRaised = false;
					line.ShowJobChargesForImportEvent += delegate
					{ showJobChargesForImportEventWasRaised = true; };

					AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					line.AL_JH = creator.Job1.PK;
					Assert("ShowJobChargesForImportEvent should not be raised", !showJobChargesForImportEventWasRaised);

					AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					line.AL_JH = creator.Job2.PK;
					Assert("ShowJobChargesForImportEvent should be raised", showJobChargesForImportEventWasRaised);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestWhenComplianceSeuqenceFailedToAssign()
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
			item.SubType = "TCR";
			item.LedgerType = "AR";
			item.InvoiceType = "CRD";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			ARCreditNote creditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote1.AH_OH = creator.ABIGAS.PK;
			ARCreditNoteLine line = (ARCreditNoteLine)creditNote1.Lines.AddNew();
			var chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;
			line.AL_GovtChargeCode = "AAA";

			creditNote1.RunPreSaveValidation();
			AssertNoErrors("Precondition", creditNote1);

			using (CreditNoteForm form = new CreditNoteForm(creditNote1))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
					"Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = creator.CreateNewComplianceSequence(ZGuid.Empty, "TCR", 2, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			ARCreditNote creditNote2 = creator.CreateARCreditNote("ARCrd2", creator.AALSHI, creator.AUD, 1m, "desc");
			line = (ARCreditNoteLine)creditNote2.Lines.AddNew();
			chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;

			creditNote2.RunPreSaveValidation();
			AssertNoErrors("Precondition", creditNote1);

			using (CreditNoteForm form = new CreditNoteForm(creditNote2))
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

		[TestDate(2018, 01, 10)]
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
			item.SubType = "TCR";
			item.LedgerType = "AR";
			item.InvoiceType = "CRD";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var sequence = creator.CreateNewComplianceSequence(ZGuid.Empty, "TCR", 1, 100, 1);
			sequence.XD_Prefix = "0102";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var creditNote1 = creator.CreateARCreditNote("ARCrd1", creator.Debtor, creator.AUD, 1m);
			creditNote1.AH_OriginalReferenceStartDate = ZDate.Today;
			creditNote1.AH_OriginalReferenceEndDate = ZDate.Today;
			creditNote1.ReasonCode = "TXT";
			var line = (ARCreditNoteLine)creditNote1.Lines.AddNew();
			var chargeList = line.ChargeList;
			chargeList.Load();
			line.GenericCharge = chargeList[0].PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = creator.GSTFREE1.PK;

			using (var form = new CreditNoteForm(creditNote1))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence."));
				AssertEquals("Email should NOT be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			// simulate the case previous invoice cannot be found
			creditNote1.AH_TransactionReference = ZString.Empty;
			Factory.Save();

			var creditNote2 = creator.CreateARCreditNote("ARCrd2", creator.Debtor, creator.AUD, 1m);
			creditNote2.AH_PostDate = creditNote2.AH_PostDate.AddHours(1);
			creditNote2.AH_OriginalReferenceStartDate = ZDate.Today;
			creditNote2.AH_OriginalReferenceEndDate = ZDate.Today;
			creditNote2.ReasonCode = "TXT";
			var line2 = (ARCreditNoteLine)creditNote2.Lines.AddNew();
			chargeList = line2.ChargeList;
			chargeList.Load();
			line2.GenericCharge = chargeList[0].PK;
			line2.AL_OSExTaxAmount = 10m;
			line2.AL_AT = creator.GSTFREE1.PK;

			using (var form = new CreditNoteForm(creditNote2))
			{
				form.Show();
				form.ValidateAndSave_ForTestOnly();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence."));
				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestTransactionReferenceForAR()
		{
			ARCreditNote aRCreditNote = Factory.New<ARCreditNote>();

			using (CreditNoteForm testForm = new CreditNoteForm(aRCreditNote))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();
				Assert(testForm.InvoiceDetails.TransactionGuidFindBox.Visible);
				Assert(testForm.InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible);
			}
		}

		public void TestTransactionReferenceIfReversing()
		{
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.SetIsReversing(true);

			using (CreditNoteForm testForm = new CreditNoteForm(aRCreditNote))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();
				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible);
				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.Visible);
			}
		}

		public void TestResetIsPostingCanceled_ARCreditNoteForAmendingApprovalGUIProvider()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			SetReceivableAuthorizationLevelSettings();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ARInv", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			Factory.Save();

			var amendingTransaction = ((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			var amendingTransactionAsInvoicingBase = amendingTransaction as InvoicingBase;

			amendingTransactionAsInvoicingBase.RunPreSaveValidation();
			AssertNoErrors("Precondition", amendingTransactionAsInvoicingBase);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Instance.Registry.ShowSaveProgressBox = false;
			using (var form = new CreditNoteForm(amendingTransactionAsInvoicingBase))
			{
				form.Show();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((formShown) =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = formShown.GetType() == typeof(LoginFormWithRequest) ? DialogResult.No : DialogResult.OK;
				});

				form.FireSaveButton();

				AssertEquals("No error should be reported", 0, ExceptionReporterTestListener.Instance.Count);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((formShown) =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = formShown.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK;
				});

				form.FireSaveButton();

				AssertEquals("No error should be reported", 0, ExceptionReporterTestListener.Instance.Count);

				var newFactory = new BusinessObjectFactory();
				var amendingARCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should save the amending AR credit note successfully", 1, amendingARCreditNotes.Length);
			}
		}

		public void TestPromptToPrintAmendingWithCreditNote()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			SetReceivableAuthorizationLevelSettings();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			Factory.Save();

			Env.Instance.Registry.ShowSaveProgressBox = false;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((formShown) =>
			{
				ZFormModaliser.ResultToReturnFromShowDialog = formShown.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK;
			});

			var creditNoteWithApproval = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			using (var form = new CreditNoteForm(creditNoteWithApproval))
			{
				form.Show();
				Assert("Not saved in db", !creditNoteWithApproval.IsInDatabase);
				form.FireSaveButton();
				AssertNotEquals("Attempt to print document for unsaved transaction", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(creditNoteWithApproval.TransactionRelatedApprovalRequest);
			}

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			var creditNoteWithoutApproval = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR002", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var creditNoteWithoutApprovalLine = TestObjectCreator.CreateInvoiceLine(creditNoteWithoutApproval, TestObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			creditNoteWithoutApprovalLine.AL_Desc = "Line";
			using (var form = new CreditNoteForm(creditNoteWithoutApproval))
			{
				form.Show();
				Assert("Not saved in db", !creditNoteWithoutApproval.IsInDatabase);
				form.FireSaveButton();
				Assert("There should be a prompt for user to print the credit note", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("The last message should prompt user to print the credit note", string.Format("Do you want to print credit note {0}?", creditNoteWithoutApproval.AH_TransactionNum), UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			using (var form = new CreditNoteForm(creditNoteWithoutApproval))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert("Saved in db", creditNoteWithoutApproval.IsInDatabase);
				form.FireSaveButton();
				AssertNotEquals("Attempt to print document for unsaved transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptToPrintReversingCreditNote()
		{
			RunTestPromptToPrintReversingCreditNote(true);
		}

		public void TestPromptToPrintReversingCreditNote_UnsuccessfulSave()
		{
			RunTestPromptToPrintReversingCreditNote(false);
		}

		void RunTestPromptToPrintReversingCreditNote(bool testSuccessfulSave)
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OSExTaxAmount = 10m;

			ARInvoiceLine aRInvLine = (ARInvoiceLine)aRInv.Lines.AddNew();
			aRInvLine.AL_AC = testObjectCreator.CC1.PK;
			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.FillWithValidTestData();
			aRInv.AH_OH = organisation.PK;
			aRInvLine.AL_OSExTaxAmount = 10m;

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowDeleteForm(aRInv))
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
					Assert("There should be a prompt for user to print the credit note", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("The last message should prompt user to print the credit note", string.Format("Do you want to print credit note {0}?", form.Invoice_ForTestOnly.AH_TransactionNum), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals("Mutex error shown", "Test Exception", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPromptToPrintReversingUACreditNote()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			UACreditNote uACrd = Factory.NewWithValidTestData<UACreditNote>();
			uACrd.AH_OSExTaxAmount = 10m;

			UACreditNoteLine uACrdLine = (UACreditNoteLine)uACrd.Lines.AddNew();
			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsDebtor = true;
			organisation.CompanyData.FillWithValidTestData();
			uACrd.AH_OH = organisation.PK;
			uACrdLine.AL_OSExTaxAmount = 10m;

			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.UACreditNote);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowDeleteForm(uACrd))
			{
				AssertEquals("IsPostOnly", true, form.IsPostOnly);
				form.FReversingReason_ForTestOnly = "Because we want to reverse";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Delete_ForTestOnly();
				Assert("There should not be a prompt for user to print the credit note", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestCheckLevelSecurityRightsWithFullSecurityRightsAndRequireTwoApprover()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;
			var setting = new AuthorizationModeAndSettings { AuthorizationMode = AuthorizationMode.Codes.TwoApprovers };
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1000.Range = RangeCodes.UpTo;
			var upTo2000 = valuesForTest.AddNew();
			upTo2000.Amount = 2000;
			upTo2000.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2000.Range = RangeCodes.UpTo;
			var above2000 = valuesForTest.AddNew();
			above2000.Amount = 2000;
			above2000.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above2000.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			OrgHeader organisation = testObjectCreator.ABIGAS;
			ZController controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				ARCreditNote aRCrd = (ARCreditNote)form.BusinessEntity;
				aRCrd.FillWithValidTestData();
				aRCrd.AH_OH = organisation.PK;
				ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)aRCrd.Lines.AddNew();
				var chargeList = aRCrdLine.ChargeList;
				chargeList.Load();
				aRCrdLine.GenericCharge = chargeList[0].PK;
				aRCrdLine.AL_OSExTaxAmount = 5000m;

				var request = Factory.New<ARCreditNoteApprovalRequest>();
				request.Initialize(new[] { aRCrd }, aRCrd.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);

				aRCrd.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(showApprovalRequestButton: false, alwaysCreateApprovalRequest: false, keepLoginFormResultAfterFirstUserAnswer: false, supportMultipleApprover: true, new[] { request });

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();
				AssertEquals("Should prompt 2 credential login form even if user has first and second level security rights", typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestPromptToPrintNewCreditNote()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.ABIGAS;

			ZController controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				ARCreditNote aRCrd = (ARCreditNote)form.BusinessEntity;
				aRCrd.FillWithValidTestData();
				aRCrd.AH_OH = organisation.PK;
				ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)aRCrd.Lines.AddNew();
				var chargeList = aRCrdLine.ChargeList;
				chargeList.Load();
				aRCrdLine.GenericCharge = chargeList[0].PK;
				aRCrdLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", string.Format("Do you want to print credit note {0}?", aRCrd.AH_TransactionNum), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAH_ComplianceSubTypeDropEdit_Readonly()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var aRCreditNote = Factory.New<ARCreditNote>();
			aRCreditNote.AH_TransactionBelongsToGroup = invoice.PK;
			aRCreditNote.AH_ComplianceSubType = "TXI";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (CreditNoteForm form = new CreditNoteForm(aRCreditNote))
			{
				form.Show();
				Application.DoEvents();

				var details = form.InvoiceDetails;

				var subType = details.AH_ComplianceSubTypeDropEdit;
				Assert("AH_ComplianceSubTypeDropEdit.Visible", subType.Visible);
				Assert("AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() visible", subType.GetExtension<LabelCaptionRenderer>().Visible);
				Assert("AH_ComplianceSubTypeDropEdit.ReadOnly", subType.ReadOnly);
			}
		}

		public override void TestIsReversingMode()
		{
			base.TestIsReversingMode();

			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.IsAmendInFull = true;

			using (var testForm = new CreditNoteForm(aRCreditNote))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				Assert(testForm.IsReversingMode_ForTestOnly);
			}
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithValidLedgerWhenInvoiceLedgerIsValid()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Credit Note", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceCompany()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Credit Note", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceFactory()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Credit Note", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsAllowed_WhenNoTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Credit Note", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsNotAllowed_WhenTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as Auto-Allocate Discrepancy has never been tested for AR Credit Note", true);
		}

		#region Implementation

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return invoice is Invoice ? new InvoiceForm(invoice) { ControllerID = ControllerIDs.ARInvoice } :
														new CreditNoteForm(invoice) { ControllerID = ControllerIDs.ARCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<ARCreditNote>() : Factory.New<ARCreditNote>();
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

		protected override bool ShouldTestCashInvoiceOnCheckBoxControl => false;

		public override void TestPromptToPrintComplianceDocumentWhenEnablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(true, AssertForPromptToPrintComplianceDocument);
		}

		public override void TestPromptToPrintComplianceDocumentWhenDisablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(false, AssertForNotPromptToPrintComplianceDocument);
		}

		public override void TestExtendDropEdit_AH_Calc_AmendStatusCode()
		{
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var dummyArInvoice = Factory.NewWithValidTestData<ARInvoice>();

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, true);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, false);
				AssertExtendDropEdit_Default(ZGuid.Empty, true);
				AssertExtendDropEdit_Default(ZGuid.Empty, false);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, true);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, false);
				AssertExtendDropEdit_Default(ZGuid.Empty, true);
				AssertExtendDropEdit_Default(ZGuid.Empty, false);
			}

			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var dummyArInvoice = Factory.NewWithValidTestData<ARInvoice>();

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, true);
				AssertExtendDropEdit_Default(dummyArInvoice.PK, false);
				AssertExtendDropEdit_Default(ZGuid.Empty, true);
				AssertExtendDropEdit_Default(ZGuid.Empty, false);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertExtendDropEdit_EnableStatusCode(dummyArInvoice.PK, true);
				AssertExtendDropEdit_EnableStatusCode(dummyArInvoice.PK, false);
				AssertExtendDropEdit_EnableStatusCode(ZGuid.Empty, true);
				AssertExtendDropEdit_EnableStatusCode(ZGuid.Empty, false);
			}

			void AssertExtendDropEdit_Default(ZGuid originalTransactionReference, bool flagAsCreatedAmending)
			{
				var arCredit = GetInvoiceWithValidTestData() as ARCreditNote;

				if (flagAsCreatedAmending)
				{
					((IAmending)arCredit).FlagAsCreatedAmending();
				}

				arCredit.OriginalTransactionReference = originalTransactionReference;
				using (var form = GetFormByInvoice(arCredit))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();

					if (flagAsCreatedAmending && !arCredit.OriginalTransactionReference.IsEmpty)
					{
						AssertEquals("Pre Condition,when flagAsCreatedAmending is true and OriginalTransactionReference is not empty, it is simulated as amend credit note.", true, form.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
					}
					else
					{
						AssertEquals("Pre Condition,when flagAsCreatedAmending is false or OriginalTransactionReference is empty, it is simulated as new AR credit note.", false, form.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
					}

					AssertExtendDropEditDefault(form);
				}
			}

			void AssertExtendDropEdit_EnableStatusCode(ZGuid originalTransactionReference, bool flagAsCreatedAmending)
			{
				var arCredit = GetInvoiceWithValidTestData() as ARCreditNote;

				if (flagAsCreatedAmending)
				{
					((IAmending)arCredit).FlagAsCreatedAmending();
				}

				using (var form = GetFormByInvoice(arCredit))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();

					AssertExtendDropEditStatusCode(form);
					var extendDropEdit = form.InvoiceDetails.ExtendDropEdit_ForTestOnly;
					arCredit.OriginalTransactionReference = originalTransactionReference;

					if (flagAsCreatedAmending && !arCredit.OriginalTransactionReference.IsEmpty)
					{
						AssertEquals("Pre Condition,when flagAsCreatedAmending is true and OriginalTransactionReference is not empty, it is simulated as amend credit note.", true, form.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
					}
					else
					{
						AssertEquals("Pre Condition,when flagAsCreatedAmending is false or OriginalTransactionReference is empty, it is simulated as new AR credit note.", false, form.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
					}

					if (!arCredit.OriginalTransactionReference.IsEmpty)
					{
						AssertEquals("Pre Condition", true, arCredit.IsAmendingTransaction);
						AssertEquals("Status Code selected index", -1, extendDropEdit.SelectedIndex);

						form.Invoice_ForTestOnly.AH_Calc_AmendStatusCode = "01";
						AssertEquals("Status Code should be synchronized", "01", extendDropEdit.Text);

						form.Invoice_ForTestOnly.OriginalTransactionReference = ZGuid.Empty;
						AssertEquals("Status Code should be refreshed to empty when OriginalTransactionReference is cleared", ZString.Empty, form.InvoiceDetails.ExtendDropEdit_ForTestOnly.Text);
					}
					else
					{
						AssertEquals("Pre Condition", false, arCredit.IsAmendingTransaction);
						AssertEquals("Status Code should be empty when AR credit note not being linked to AR invoice.", ZString.Empty, extendDropEdit.Text);
						AssertEquals("Status Code should be read only when AR credit note not being linked to AR invoice.", true, extendDropEdit.ReadOnly);
					}
				}
			}
		}

		#endregion
	}
}
