using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobInvoicingReverserTest : TestCaseWithFactory
	{
		public void TestReverseInvoiceWithoutShowReversalDatesForm_WhenPostDateHasErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.India))
			using (AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				Factory.Save();

				var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				originalInvoice.AH_PostDate = new ZDate(2022, 3, 31);
				originalInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalInvoice.PK));
				Factory.Save();

				var securityHelper = new JobInvoicingSecurityHelper(job.PlugInData.InvoicingSupporter.JobInvoicingSecurity);
				securityHelper.GetInvSecurity(SecurityCore.ModifyTransactionDate).IsAllowed = false;
				securityHelper.GetInvSecurity(SecurityCore.ModifyPostDate).IsAllowed = false;
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				var reverser = new JobInvoicingReverser(job);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				AssertEquals("Continue With Save", false, reverser.ContinueWithSave);
				AssertContains(@"Error - AH_PostDate: You cannot reverse this transaction as period to credit GST amounts has lapsed. You can create an amending credit note without GST.

If you must reverse this transaction, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Allow Crediting India GST Eight months after Financial Year End", reverser.Errors.ToString());
			}
		}

		public void TestReverseInvoiceWithoutShowReversalDatesForm_WhenPostDateHasErrors_AndShouldAssignComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.India))
			using (AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				Factory.Save();

				var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				originalInvoice.AH_PostDate = new ZDate(2022, 3, 31);
				originalInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalInvoice.PK));
				Factory.Save();

				var securityHelper = new JobInvoicingSecurityHelper(job.PlugInData.InvoicingSupporter.JobInvoicingSecurity);
				securityHelper.GetInvSecurity(SecurityCore.ModifyTransactionDate).IsAllowed = false;
				securityHelper.GetInvSecurity(SecurityCore.ModifyPostDate).IsAllowed = false;
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				var reverser = new JobInvoicingReverser(job);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				AssertEquals("Continue With Save", false, reverser.ContinueWithSave);
				AssertContains(@"00001000:	Please check your Compliance Invoice Book Setups. 
 A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.", reverser.Errors.ToString());

				TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, IndiaComplianceInfo.ComplianceSubTypeCodes.TXC);
				reverser = new JobInvoicingReverser(job);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				AssertEquals("Continue With Save", false, reverser.ContinueWithSave);
				AssertContains(@"Error - AH_PostDate: You cannot reverse this transaction as period to credit GST amounts has lapsed. You can create an amending credit note without GST.

If you must reverse this transaction, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Allow Crediting India GST Eight months after Financial Year End", reverser.Errors.ToString());
			}
		}

		#region Reverse Invoice with CreditAdjustmentNotePostingApprovalLevels

		public void TestReverseInvoiceWhenUserHasCreditAdjustmentNotePostingApprovalLevelsSecurity()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, null);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(reverser.ContinueWithApprovalFactorySave);
				Assert(reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndAllExisitngApprovalRequestsAreAlreadyApproved()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, null);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(reverser.ContinueWithApprovalFactorySave);
				Assert(reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndProvidesSpotOnAuthorisation()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, x => true);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(reverser.ContinueWithApprovalFactorySave);
				Assert(reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndCancelsWithoutProvidingSpotOnAuthorisation()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider> { CallBase = true };
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest> { CallBase = true };
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false); //user cancels out
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, x => false);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreUnapprovedApprovalRequests_UserSelectsToCancelExistingRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), new ZString(AccTransactionHeaderSchema.Constants.Prefix)));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.SetupSequence(m => m.ShowMessage(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
				It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()))
				.Returns(ZDialogResult.Yes) //user wants to cancel exisitng requests and create new ones
				.Returns(ZDialogResult.OK); //exit message

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, x => false);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				guiWrapper.Verify();
			}

			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreUnapprovedApprovalRequests_UserDoesNotWantToCancelExistingRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider> { CallBase = true };
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest> { CallBase = true };
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.No);//user does not want to cancel exisitng requests
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, x => false);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndThereAreNoUnapprovedApprovalRequests()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider> { CallBase = true };
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);
			guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, x => false);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
				Assert(reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		public void TestReverseInvoiceWhenNoARInvoicesAvailable()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("1234", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "", job, TestObjectCreator.CC1, 10m, ZDateTime.Today, false);
			TestObjectCreator.CreateJobCharge(creditNote.Lines[0], job, TestObjectCreator.CC1);
			Factory.Save();

			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Verify(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);//should not call PerformReversalAuthorization method

			var reverser = new JobInvoicingReverser(job, null, guiWrapper.Object);
			Assert(!reverser.ContinueWithApprovalFactorySave);
			Assert(!reverser.ContinueWithSave);
			reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
			guiWrapper.Verify();
			Assert(!reverser.ContinueWithApprovalFactorySave);
			Assert(reverser.ContinueWithSave);
		}

		[ExpectNoExceptions]
		public void TestReverseInvoiceWhenApprovalGUIDProviderIsNull()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var reverser = new JobInvoicingReverser(job, null, null);
			Assert(!reverser.ContinueWithApprovalFactorySave);
			Assert(!reverser.ContinueWithSave);
			reverser.ReverseAllInvoices("Test Invoice Reversal", "TST");
			Assert(!reverser.ContinueWithApprovalFactorySave);
			Assert(reverser.ContinueWithSave);
		}

		public void TestReverseInvoiceWhenUserDoesNotHaveCreditAdjustmentNotePostingApprovalLevelsSecurityAndAllExisitngApprovalRequestsAreAlreadyApproved_WhenUserStopReversingProcessAtShowReveralDatesForm()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));
			Factory.Save();

			var request1 = CreateApprovalRequest(invoice1, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			var request2 = CreateApprovalRequest(invoice2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 2;
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var reverser = new JobInvoicingReverserForInvoiceReversal(job, null, guiWrapper.Object, null);
				ShowReversalDatesFormResult = null; //stops reversing process
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				reverser.ReverseAllInvoices("Test Invoice Reversal", "TST", ShowReversalDatesForm);
				Assert(!reverser.ContinueWithApprovalFactorySave);
				Assert(!reverser.ContinueWithSave);
				guiWrapper.Verify();
			}
		}

		AuthorizationModeAndSettings GetAuthorisationConfigSetting()
		{
			var result = new AuthorizationModeAndSettings();
			var collection = result.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			return result;
		}

		ARCreditNoteApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
			approvalRequest.XP_ReasonDescription = "Invoice Reverse Testing";
			return approvalRequest;
		}

		class JobInvoicingReverserForInvoiceReversal : JobInvoicingReverser
		{
			public JobInvoicingReverserForInvoiceReversal(Job jobToReverse, SetSecurityProviderDelegate securityProviderSetter, IPostingJobTransactionsApprovalGUIProvider approvalGUIProvider, Func<InvoicingBase, bool> checkLevelSecurityRights) : base(jobToReverse, securityProviderSetter, approvalGUIProvider)
			{
				CheckLevelSecurityRights = checkLevelSecurityRights;
			}

			readonly Func<InvoicingBase, bool> CheckLevelSecurityRights;

			protected override Tuple<bool, bool> PerformReversalAuthorization(string reversingReason, IEnumerable<InvoicingBase> invoicesToBeReversed)
			{
				var helper = new ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(ApprovalGUIProvider);
				helper.CheckLevelSecurityRights_ForTestOnly = CheckLevelSecurityRights; //Can not mock security certificates
				return helper.PerformLevelAuthorizationForReversing(invoicesToBeReversed.ToArray());
			}
		}

		#endregion

		public void TestIsReversingInProcess()
		{
			Job testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);

			AssertEquals(false, testJob.IsReversingInProcess);
			JobInvoicingReverser reverser = new JobInvoicingReverser(testJob);
			reverser.ReverseAllInvoices("test", "tst");
			AssertEquals(false, testJob.IsReversingInProcess);
		}

		public void TestReopenClosedJobDenied()
		{
			Job testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Closed.Code);
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Env.Security.ReopenJob.IsAllowed = false;
				JobInvoicingReverser reverser = new JobInvoicingReverser(testJob);
				reverser.ReverseAllInvoices("test", "tst");
				AssertEquals("ReopenJob.IsAllowed = false, Should remain Closed", JobHeaderStatus.Closed.Code, reverser.JobToReverse.JH_Status);
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = isAllowed;
			}
		}

		public void TestReopenClosedJobAllowed()
		{
			Job testJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			testJob = Factory.Load<Job>(testJob.PK);
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 1000m, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 1000m, TestObjectCreator.LocalClient);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			testJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			bool isAllowed = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Env.Security.ReopenJob.IsAllowed = true;
				JobInvoicingReverser reverser = new JobInvoicingReverser(testJob);
				reverser.ReverseAllInvoices("test", "tst");
				AssertEquals("ReopenJob.IsAllowed = true, Should Reopen", JobHeaderStatus.Working.Code, reverser.JobToReverse.JH_Status);
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = isAllowed;
			}
		}

		public void TestCannotReverseJobWithInvoicingOnHold()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader localClient = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(10), true, true, true, true, true, true);
			OrgHeader agent = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(10), true, true, true, true, true, true);

			Job testJob = creator.CreateJob(localClient, 5M, agent, 10M);
			Factory.Save();

			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			try
			{
				Env.Security.ReopenJob.IsAllowed = true;

				JobInvoicingReverser reverser = new JobInvoicingReverser(testJob);

				AssertEquals("", reverser.IsValidToReverseAllInvoices());

				testJob.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				Factory.Save();

				AssertEquals(string.Format("Cannot reverse Invoices because the job has status '{0}'.", JobHeaderStatus.WorkOnHold.Description), reverser.IsValidToReverseAllInvoices());

				testJob.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
				Factory.Save();

				AssertEquals(string.Format("Cannot reverse Invoices because the job has status '{0}'.", JobHeaderStatus.InvoiceOnHold.Description), reverser.IsValidToReverseAllInvoices());
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = isAllowed;
			}
		}

		[TestDate(2021, 5, 1)]
		public void TestCannotReverseJobWithComplianceErrors_InvalidBook_PST()
		{
			AssertCannotReverseJobWithComplianceErrors_InvalidBook(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 5, 1)]
		public void TestCannotReverseJobWithComplianceErrors_InvalidBook_INV()
		{
			AssertCannotReverseJobWithComplianceErrors_InvalidBook(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCannotReverseJobWithComplianceErrors_InvalidBook(string dateOption)
		{
			var today = ZDate.Today;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.EUR, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.EUR, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				SetInvoiceDate(invoice1, dateOption, ZDateTime.Today, ZDateTime.Today.AddMonths(-2));
				invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.EUR, 1m, TestObjectCreator.ABIGAS);
				SetInvoiceDate(invoice2, dateOption, ZDateTime.Today, ZDateTime.Today.AddMonths(-2));
				invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				const string subTypeRec = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;

				var fullSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subTypeRec, "ARI.21_", 1, 100, 101);
				fullSeq.XD_StartDate = today.AddMonths(-1);
				fullSeq.XD_ExpiryDate = today.AddMonths(1).AddDays(-1);
				fullSeq.XD_IsActive = true;
				Factory.Save();

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					var reverser = new JobInvoicingReverser(job);
					reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
					Assert(!reverser.ContinueWithSave);
					AssertContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, reverser.Errors.ToString());

					reverser = new JobInvoicingReverser(job);
					reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", showReversalDatesForm);
					Assert(!reverser.ContinueWithSave);
					AssertContains(ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, reverser.Errors.ToString());
				}

				bool? showReversalDatesForm(TransactionHeaderCollection reversedInvoices)
				{
					foreach (TransactionHeader transaction in reversedInvoices)
					{
						transaction.AH_ComplianceSubType = subTypeRec;
					}
					return true;
				}
			}
		}

		void SetInvoiceDate(TransactionHeader invoice, string dateOption, ZDateTime allocationDate, ZDateTime otherDate)
		{
			if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_PostDate = otherDate;
				invoice.AH_InvoiceDate = allocationDate;
			}
			else
			{
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = otherDate;
			}
		}

		[TestDate(2021, 1, 15)]
		public void TestCannotReverseJobWithComplianceErrors_NotAllocatable_PST()
		{
			AssertCannotReverseJobWithComplianceErrors_NotAllocatable(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestCannotReverseJobWithComplianceErrors_NotAllocatable_INV()
		{
			AssertCannotReverseJobWithComplianceErrors_NotAllocatable(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCannotReverseJobWithComplianceErrors_NotAllocatable(string dateOption)
		{
			var today = ZDate.Today;
			var dateOutsideSequencePeriod = new ZDateTime(2020, 10, 15);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.EUR, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.EUR, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.ABIGAS);

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				SetInvoiceDate(invoice1, dateOption, ZDateTime.Today, dateOutsideSequencePeriod);
				invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.EUR, 1m, TestObjectCreator.ABIGAS);
				SetInvoiceDate(invoice2, dateOption, ZDateTime.Today, dateOutsideSequencePeriod);
				invoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice2.PK));

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				const string subTypeRec = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;

				var complSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subTypeRec, "TXE.21-", 1, 100, 3);
				complSeq.XD_StartDate = new ZDate(today.Year, today.Month, 1);
				complSeq.XD_ExpiryDate = today.AddMonths(1).AddDays(-1);
				complSeq.XD_IsActive = true;

				var lastDateUsed = today;

				var inv1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1);
				inv1.AH_TransactionNum = "INV1";
				SetInvoiceDate(inv1, dateOption, complSeq.XD_StartDate, dateOutsideSequencePeriod);
				inv1.AH_ComplianceSubType = subTypeRec;
				inv1.AH_XD_ComplianceBook = complSeq.PK;
				inv1.AH_TransactionReference = "TXE.21-0001";

				var inv2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1);
				inv2.AH_TransactionNum = "INV2";
				SetInvoiceDate(inv2, dateOption, today.AddDays(-5), dateOutsideSequencePeriod);
				inv2.AH_ComplianceSubType = subTypeRec;
				inv2.AH_XD_ComplianceBook = complSeq.PK;

				var inv3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1);
				inv3.AH_TransactionNum = "INV3";
				SetInvoiceDate(inv3, dateOption, lastDateUsed, dateOutsideSequencePeriod);
				inv3.AH_ComplianceSubType = subTypeRec;
				inv3.AH_XD_ComplianceBook = complSeq.PK;
				inv3.AH_TransactionReference = "TXE.21-0002";

				Factory.Save();

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				ZDate allocationDate = ZDate.Empty;

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					var reverser = new JobInvoicingReverser(job);
					allocationDate = today.AddDays(-10);
					reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", showReversalDatesForm);
					Assert(!reverser.ContinueWithSave);
					var expMsgErr = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
						subTypeRec, lastDateUsed.ToShortDateString());
					AssertContains(expMsgErr, reverser.Errors.ToString());

					reverser = new JobInvoicingReverser(job);
					allocationDate = lastDateUsed;
					reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", showReversalDatesForm);
					Assert(!reverser.ContinueWithSave);
					expMsgErr = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
						subTypeRec, lastDateUsed.ToShortDateString());
					AssertContains(expMsgErr, reverser.Errors.ToString());
				}

				bool? showReversalDatesForm(TransactionHeaderCollection reversedInvoices)
				{
					foreach (TransactionHeader transaction in reversedInvoices)
					{
						transaction.AH_ComplianceSubType = subTypeRec;
						SetInvoiceDate(transaction, dateOption, allocationDate, dateOutsideSequencePeriod);
					}
					return true;
				}
			}
		}

		public void TestCanReverseJobWithUnpostedApportionment()
		{
			ForwardingConsol exportConsol = Factory.New<ForwardingConsol>();
			Factory.Save();

			TestObjectCreator creator = new TestObjectCreator(Factory);

			exportConsol.JK_TransportMode = "AIR";
			exportConsol.JK_RL_NKLoadPort = "AUSYD";
			exportConsol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = exportConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.AALSHI.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, exportConsol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.63m;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			AssertEquals("An apportioned charge exist.", 1, shipmentJob.Charges.Count);

			Charge shipmentJobCharge = shipmentJob.Charges[0];
			shipmentJobCharge.JR_OSSellAmt = 200m;
			shipmentJobCharge.JR_OH_SellAccount = creator.ABIGAS.PK;

			Factory.Save();

			ChargePoster poster = new ChargePoster(Factory);
			AssertNotNull(poster.Post(shipmentJobCharge));

			Factory.Save();

			JobInvoicingReverser reverser = new JobInvoicingReverser(shipmentJob);
			AssertEquals("", reverser.IsValidToReverseAllInvoices());
		}

		public void TestReverseJobAndDontResetUnpostedApportionment()
		{
			ForwardingConsol exportConsol = Factory.New<ForwardingConsol>();
			Factory.Save();

			TestObjectCreator creator = new TestObjectCreator(Factory);

			exportConsol.JK_TransportMode = "AIR";
			exportConsol.JK_RL_NKLoadPort = "AUSYD";
			exportConsol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = exportConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.AALSHI.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, exportConsol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.63m;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			AssertEquals("An apportioned charge exist.", 1, shipmentJob.Charges.Count);

			Charge shipmentJobCharge = shipmentJob.Charges[0];
			shipmentJobCharge.JR_OSSellAmt = 200m;
			shipmentJobCharge.JR_OH_SellAccount = creator.ABIGAS.PK;

			Factory.Save();

			ChargePoster poster = new ChargePoster(Factory);
			AssertNotNull(poster.Post(shipmentJobCharge));

			Factory.Save();

			AssertEquals("Charge is apportioned.", true, shipmentJobCharge.JR_IsApportioned);

			JobInvoicingReverser reverser = new JobInvoicingReverser(shipmentJob);
			reverser.ReverseAllInvoices("test", "tst");
			Factory.Save();
			AssertEquals("Job should have a charge.", 1, shipmentJob.Charges.Count);
			AssertEquals("Charge is still apportioned after Reversing.", true, shipmentJob.Charges[0].JR_IsApportioned);
		}

		public void TestReverseInvoicesWithRelatedPaidItemsWithSecurityRights()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol exportConsol = Factory.New<ForwardingConsol>();
			exportConsol.JK_TransportMode = "AIR";
			exportConsol.JK_RL_NKLoadPort = "AUSYD";
			exportConsol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = exportConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.AALSHI.PK;

			Charge charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OH_CostAccount = creator.Creditor1.PK;
			charge.JR_OSCostAmt = 1000m;
			charge.JR_APInvoiceNum = "ABCD1234";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_AB = creator.AUDBankAccount.PK;
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(shipmentJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			AssertEquals("Precondition: AR should be posted", true, charge.IsRevenuePosted);
			AssertEquals("Precondition: AP should be posted", true, charge.IsCostPosted);
			AssertEquals("Precondition: AP should be paid", 0m, charge.APLine.TransactionHeader.AH_OutstandingAmount);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;

			JobInvoicingReverser reverser = new JobInvoicingReverser(shipmentJob,
				delegate(InvoicingBase[] transactions)
				{
					foreach (var transaction in transactions)
					{
						// Provide credentials for someone who has security rights that we don't have
						NonInteractiveSecurityOverrideProvider securityProvider =
							new NonInteractiveSecurityOverrideProvider();
						securityProvider.OverrideLogin = User.SupportUserName;
						securityProvider.OverridePassword = CWSupportLoginToken.TokenForTest;
						transaction.SecurityOverrideProvider = securityProvider;
					}
				}, null);
			reverser.ReverseAllInvoices("test", "tst");
			AssertEquals("Continue With Save", true, reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();
			Factory.Save();

			AssertEquals("AR should NOT be posted", false, charge.IsRevenuePosted);
			AssertEquals("AP should still be posted", true, charge.IsCostPosted);
		}

		public void TestReverseInvoicesWithoutRelatedPaidItemsWithSecurityRights()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol exportConsol = Factory.New<ForwardingConsol>();
			exportConsol.JK_TransportMode = "AIR";
			exportConsol.JK_RL_NKLoadPort = "AUSYD";
			exportConsol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = exportConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.ABIGAS.PK;

			Charge charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OH_CostAccount = creator.Creditor1.PK;
			charge.JR_OSCostAmt = 1000m;
			charge.JR_APInvoiceNum = "ABCD1234";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_AB = creator.AUDBankAccount.PK;
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(shipmentJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			AssertEquals("Precondition: AR should be posted", true, charge.IsRevenuePosted);
			AssertEquals("Precondition: AP should be posted", true, charge.IsCostPosted);
			AssertEquals("Precondition: AP should be paid", 0m, charge.APLine.TransactionHeader.AH_OutstandingAmount);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;

			JobInvoicingReverser reverser = new JobInvoicingReverser(shipmentJob,
				delegate(InvoicingBase[] transactions)
				{
					foreach (var transaction in transactions)
					{
						// Simulate Cancel Button or pressing 'Esc' by not providing username & password
						transaction.SecurityOverrideProvider = new NonInteractiveSecurityOverrideProvider();
					}
				}, null);
			reverser.ReverseAllInvoices("test", "tst");
			AssertEquals("Continue With Save", false, reverser.ContinueWithSave);

			charge = reverser.ReversingFactory.Load<Charge>(charge.PK);
			AssertNotNull("Charge AR Line shouldn't be cleared", charge.ARLine);
			AssertEquals("AR should still be posted", TransactionLineTypes.Revenue, charge.ARLine.AL_LineType);
			AssertEquals("AP should still be posted", TransactionLineTypes.Cost, charge.APLine.AL_LineType);
			AssertEquals("Invoice should not be cancelled", false, charge.ARLine.TransactionHeader.AH_IsCancelled);
		}

		public void TestReverseMultipleInvoicesWithRelatedPaidItemsWithoutSecurityRights()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol exportConsol = Factory.New<ForwardingConsol>();
			exportConsol.JK_TransportMode = "AIR";
			exportConsol.JK_RL_NKLoadPort = "AUSYD";
			exportConsol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = exportConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.LocalChargesPK = creator.AALSHI.PK;

			creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Charge charge1 = CreateChargeWithCostAndSellDetails(shipmentJob, creator.CC1, creator.AALSHI, creator.Creditor1, creator.AUDBankAccount);
			Charge charge2 = CreateChargeWithCostAndSellDetails(shipmentJob, creator.CC2, creator.ABIGAS, creator.Creditor2, creator.AUDBankAccount);
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(shipmentJob);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			AssertEquals("Precondition: Charge 1 AR should be posted", true, charge2.IsRevenuePosted);
			AssertEquals("Precondition: Charge 1 AP should be posted", true, charge2.IsCostPosted);
			AssertEquals("Precondition: Charge 1 AP should be paid", 0m, charge2.APLine.TransactionHeader.AH_OutstandingAmount);

			AssertEquals("Precondition: Charge 2 AR should be posted", true, charge2.IsRevenuePosted);
			AssertEquals("Precondition: Charge 2 AP should be posted", true, charge2.IsCostPosted);
			AssertEquals("Precondition: Charge 2 AP should be paid", 0m, charge2.APLine.TransactionHeader.AH_OutstandingAmount);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;

			JobInvoicingReverser reverser = new JobInvoicingReverser(shipmentJob,
				delegate(InvoicingBase[] transactions)
				{
					foreach (var transaction in transactions)
					{
						// Provide credentials for someone who has security rights that we don't haveo
						NonInteractiveSecurityOverrideProvider securityProvider =
							new NonInteractiveSecurityOverrideProvider();
						// We want to provide credentials for only one Invoice
						// We are simulating pressing the 'Cancel' button for the second Invoice

						if (transaction.PK == charge1.ARLine.AL_AH)
						{
							securityProvider.OverrideLogin = User.SupportUserName;
							securityProvider.OverridePassword = CWSupportLoginToken.TokenForTest;
							transaction.SecurityOverrideProvider = securityProvider;
						}
					}
				}, null);

			reverser.ReverseAllInvoices("test", "tst");
			charge1 = reverser.ReversingFactory.Load<Charge>(charge1.PK);
			charge2 = reverser.ReversingFactory.Load<Charge>(charge2.PK);

			AssertEquals("Continue With Save", false, reverser.ContinueWithSave);
		}

		Charge CreateChargeWithCostAndSellDetails(Job job, AccChargeCode chargeCode, OrgHeader sellAccount, OrgHeader costAccount, AccBankAccount bankAccount)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = sellAccount.PK;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OH_CostAccount = costAccount.PK;
			charge.JR_OSCostAmt = 1000m;
			charge.JR_APInvoiceNum = "ABCD1234";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			charge.JR_PaymentType = ReceiptTypes.Cash;
			charge.JR_AB = bankAccount.PK;
			return charge;
		}

		[TestDate(2012, 03, 01)]
		public void TestReverseAllInvoices_ShowReveralDatesForm_True()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			ChargePoster chargePoster = new ChargePoster(Factory);
			InvoicingBase invoice = chargePoster.Post(charge);
			Factory.Save();

			JobInvoicingReverser reverser = new JobInvoicingReverser(job);
			ShowReversalDatesFormResult = true;
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", ShowReversalDatesForm);
			Assert("ContinueWithSave", reverser.ContinueWithSave);

			ARCreditNote[] creditNote = reverser.ReversingFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Count", 1, creditNote.Length);
			AssertEquals("AH_InvoiceDate", ZDateTime.Today.AddDays(5), creditNote[0].AH_InvoiceDate);
			AssertEquals("AH_PostDate", ZDateTime.Today.AddDays(5), creditNote[0].AH_PostDate);
		}

		[TestDate(2012, 03, 01)]
		public void TestReverseAllInvoices_ShowReveralDatesForm_False()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			ChargePoster chargePoster = new ChargePoster(Factory);
			InvoicingBase invoice = chargePoster.Post(charge);
			Factory.Save();

			JobInvoicingReverser reverser = new JobInvoicingReverser(job);
			ShowReversalDatesFormResult = false;
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", ShowReversalDatesForm);
			Assert("ContinueWithSave", reverser.ContinueWithSave);

			ARCreditNote[] creditNote = reverser.ReversingFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Count", 1, creditNote.Length);
			AssertEquals("AH_InvoiceDate", ZDateTime.Today, creditNote[0].AH_InvoiceDate);
			AssertEquals("AH_PostDate", ZDateTime.Today, creditNote[0].AH_PostDate);
		}

		[TestDate(2012, 03, 01)]
		public void TestReverseAllInvoices_ShowReveralDatesForm_Null()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			ChargePoster chargePoster = new ChargePoster(Factory);
			InvoicingBase invoice = chargePoster.Post(charge);
			Factory.Save();

			JobInvoicingReverser reverser = new JobInvoicingReverser(job);
			ShowReversalDatesFormResult = null;
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", ShowReversalDatesForm);
			Assert("ContinueWithSave", !reverser.ContinueWithSave);
		}

		[TestDate(2015, 01, 01)]
		public void TestReverseAllInvoices_GetDefaultARInvoiceDate()
		{
			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as not current year/month
			var configDateTime = ZDateTime.Now.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);

			var shipment = TestObjectCreator.CreateShipment("S00001000");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			var chargePoster = new ChargePoster(Factory);
			var invoice = chargePoster.Post(charge);
			Factory.Save();

			var reverser = new JobInvoicingReverser(job);
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("ContinueWithSave", reverser.ContinueWithSave);

			ARCreditNote[] creditNote = reverser.ReversingFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Count", 1, creditNote.Length);

			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, invoice.AH_InvoiceDate);

			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, invoice.AH_PostDate);

			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, creditNote[0].AH_InvoiceDate);

			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, creditNote[0].AH_PostDate);
		}

		[TestDate(2015, 02, 16)]
		public void TestReverseAllInvoices_DeleteInvoice()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			var chargePoster = new ChargePoster(Factory);
			var invoice = chargePoster.Post(charge);
			Factory.Save();

			var reverser = new JobInvoicingReverser(job);
			ShowReversalDatesFormResult = true;
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE", ShowReversalDatesFormDeleteInvoice);
			Assert("Should not ContinueWithSave", !reverser.ContinueWithSave);
		}

		[TestDate(2022, 02, 25)]
		public void TestReverseAllInvoices_DoNotResetSellSideIfCashAdvanceExist()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var creator = new TestObjectCreator(Factory);

				var exportConsol = Factory.New<ForwardingConsol>();
				exportConsol.JK_TransportMode = "AIR";
				exportConsol.JK_RL_NKLoadPort = "AUSYD";
				exportConsol.JK_RL_NKDischargePort = "USLAX";

				var shipment = exportConsol.Shipments.AddNew();
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = creator.ZECTRA.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = creator.AALSHI.PK;

				var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.LocalChargesPK = creator.ABIGAS.PK;

				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = creator.CC1.PK;
				charge.JR_OH_SellAccount = creator.ABIGAS.PK;
				charge.JR_OSSellAmt = 1050m;
				charge.JR_OH_CostAccount = creator.Creditor1.PK;
				charge.JR_OSCostAmt = 1000m;
				charge.JR_APInvoiceNum = "ABCD1234";
				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_IsARCashAdvance = true;
				Factory.Save();

				var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsReceivable, 1050M, 1050M, "AUD");
				var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, 1050M, 1050M);
				charge.JR_CAL_ARLine = line.PK;
				header.MarkAsPaid();
				Factory.Save();

				var postManager = new InvoicingPostManager(shipmentJob);
				postManager.CreateTransactions(JobInvoicingPostingOption.All);
				Factory.Save();

				var invoice = charge.ARLine.TransactionHeader;
				AssertEquals("Precondition: AR should be posted", true, charge.IsRevenuePosted);
				AssertEquals("Precondition: AP should be posted", true, charge.IsCostPosted);
				AssertEquals("Line status should be Invoiced", CashAdvanceStatusCodes.RequestLine.Invoiced, line.CAL_Status);

				AssertEquals("Should not be cancelled", false, invoice.AH_IsCancelled);
				AssertEquals("Outstanding amount", 0M, invoice.AH_OutstandingAmount);
				AssertEquals("Fully paid date", TestDateAttribute.Date, invoice.AH_FullyPaidDate.ToDateTime());

				var reverser = new JobInvoicingReverser(shipmentJob);
				reverser.ReverseAllInvoices("test", "tst");
				reverser.ReversingFactory.Save();

				invoice = reverser.ReversingFactory.Load<ARInvoice>(invoice.PK);
				charge = reverser.ReversingFactory.Load<Charge>(charge.PK);

				AssertEquals("Charge sell side should not be cleared", 1050M, charge.JR_OSSellAmt);
				AssertEquals("Charge sell side should not be cleared", 1050M, charge.JR_OSSellAmt);
				AssertEquals("Charge sell side should not be cleared", 1050M, charge.JR_LocalSellAmt);
				AssertEquals("Charge sell side should not be cleared", "AUD", charge.JR_RX_NKSellCurrency);
				AssertEquals("Line status should be Paid", CashAdvanceStatusCodes.RequestLine.Paid, line.CAL_Status);

				AssertEquals("Should be cancelled", true, invoice.AH_IsCancelled);
				AssertEquals("Outstanding amount", 0M, invoice.AH_OutstandingAmount);
				AssertEquals("Fully paid date", TestDateAttribute.Date, invoice.AH_FullyPaidDate.ToDateTime());
			}
		}

		[TestDate(2015, 02, 16)]
		public void TestReverseAllInvoices_AddsApprovingUserInfoFromOriginalToReverseTransaction()
		{
			SecurityTestObject.CreateTestUser(true, "", "tst", "testUser", "password");
			var shipment = TestObjectCreator.CreateShipment("S00001000");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc",
				TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI,
				TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			var chargePoster = new ChargePoster(Factory);
			var invoice = chargePoster.Post(charge);
			Factory.Save();

			var reverser = new JobInvoicingReverser(job);
			var invoiceReversingFactory = reverser.ReversingFactory.Load<ARInvoice>(invoice.PK);
			invoiceReversingFactory.ApprovingUserPK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "tst")).PK;
			invoiceReversingFactory.ApprovalDate = new ZDateTime(2019, 1, 1);

			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("ContinueWithSave", reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();

			var creditNotes = reverser.ReversingFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Count", 1, creditNotes.Length);
			var reversedNoteATHLogs = creditNotes.First(x => x.IsReversalTransaction).Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.Authorised.Code).Cast<StmALog>();
			AssertEquals("Should contain authorisation log when creating a reverse credit note", 1, reversedNoteATHLogs.Count());
			AssertContains("Should contain log for User1", "testUser", reversedNoteATHLogs.First().SL_Reference);
			AssertEquals("Should use date from invoice", new ZDateTime(2019, 1, 1), reversedNoteATHLogs.First().SL_EventTime);
		}

		bool? ShowReversalDatesFormResult;

		bool? ShowReversalDatesForm(TransactionHeaderCollection reversedInvoices)
		{
			if (ShowReversalDatesFormResult.HasValue && ShowReversalDatesFormResult.Value)
			{
				foreach (TransactionHeader transaction in reversedInvoices)
				{
					transaction.AH_InvoiceDate = ZDateTime.Today.AddDays(5);
					transaction.AH_PostDate = transaction.AH_InvoiceDate;
				}
			}
			return ShowReversalDatesFormResult;
		}

		bool? ShowReversalDatesFormDeleteInvoice(TransactionHeaderCollection reversedInvoices)
		{
			if (reversedInvoices.Count > 0)
			{
				var invoiceToBeDeleted = reversedInvoices[0];
				reversedInvoices.RemoveAndDelete(invoiceToBeDeleted);
			}
			return ShowReversalDatesForm(reversedInvoices);
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
