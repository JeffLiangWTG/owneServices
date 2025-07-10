using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ARCreditNoteApprovalRequest = Enterprise.Accounting.Business.ARAP.Invoicing.ARCreditNoteApprovalRequest;
using ZGuid = CargoWise.Types.ZGuid;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	[TestedType(typeof(ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest))]
	class ARCreditNoteForReversalLevelAuthorizationWithApprovalRequestTest : ARCreditNoteLevelAuthorizationWithApprovalRequestTest
	{
		public override void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			Assert("Currently Preview mode is not available and so it's not applicable here.", true);
		}

		public override void TestPerformTransactionLevelAuthorization_NoTransactionsAndPreview()
		{
			Assert("At least one transaction should be here always. Preview mode is not applicable here.", true);
		}

		public override void TestPerformTransactionLevelAuthorization_PostWithoutRight()
		{
			Assert("This class is for invoice reversal, and is tested by different test cases", true);
		}

		public override void TestPerformTransactionLevelAuthorization_PostWithRight()
		{
			Assert("This class is for invoice reversal, and is tested by different test cases", true);
		}

		public void TestUserHasSufficientApprovalLevels()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, null);
				Assert("Should continue reversing logic", result.Item1);
				Assert("Should save the approval factory", result.Item2); //Eventhough we do not create new approval requests, continueProcessing variable is TRUE
				guiWrapper.Verify();
				AssertEquals("Should not set user when no approval occurs", 0, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should not set user when no approval occurs", 0, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndAllExisitngApprovalRequestsAreAlreadyApproved()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(),
					It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, null);
				Assert("Should continue reversing logic", result.Item1);
				Assert("Should save the approval factory", result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should set user from approvalrequests", 1, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should set user from approvalrequests", ApprovingUser.PK, invoice1.ApprovingUserPK);
				AssertEquals("Should set user from approvalrequests", 1, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should set user from approvalrequests", ApprovingUser.PK, invoice2.ApprovingUserPK);
				AssertEquals("Should set approval date from request", TestApprovalDate, invoice1.ApprovalDate);
				AssertEquals("Should set approval date from request", TestApprovalDate, invoice2.ApprovalDate);
			}
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndProvidesSpotOnAuthorisation()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock
				.Setup(m => m.UserSecurityOverride)
				.Returns(new SecurityCore(null, ApprovingUser, Guid.Empty, Guid.Empty, Guid.Empty));
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => true; //on the spot authorization
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights);
			}

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				Assert("Should continue reversing logic", result.Item1);
				Assert("Should save the approval factory", result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should set user from approvalrequests", 1, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should set user from approvalrequests", ApprovingUser.PK, invoice1.ApprovingUserPK);
				AssertEquals("Should set user from approvalrequests", 1, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should set user from approvalrequests", ApprovingUser.PK, invoice2.ApprovingUserPK);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndCancelsWithoutProvidingSpotOnAuthorisation()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false); //user cancels out
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			guiWrapper
				.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()))
				.Returns(ZDialogResult.OK);//exit message)

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights);
			}

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				Assert("Should not continue reversing logic", !result.Item1);
				Assert("Should not save the approval factory", !result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should not set user when no approval occurs", 0, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should not set user when no approval occurs", 0, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndThereAreUnapprovedApprovalRequests_UserSelectsToCancelExistingRequests()
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
			guiWrapper.Setup(m => m.ShowCreditNoteReversalReasonForm(It.IsAny<string>())).Returns(Tuple.Create(new ZString("IOB"), new ZString("Incorrect Organisation Billed")));

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights, true);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				Assert("Should not continue reversing logic", !result.Item1);
				Assert("Should save the approval factory", result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should not set user when no approval occurs", 0, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should not set user when no approval occurs", 0, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}

			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndThereAreUnapprovedApprovalRequests_UserDoesNotWantToCancelExistingRequests()
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
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request)
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
				It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.No);//user does not want to cancel existing requests
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				Assert("Should not continue reversing logic", !result.Item1);
				Assert("Should not save the approval factory", !result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should not set user when no approval occurs", 0, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should not set user when no approval occurs", 0, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}
		}

		public void TestUserDoesNotHaveSufficientApprovalLevelsAndThereAreNoUnapprovedApprovalRequests()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never);
			guiWrapper
				.Setup(m => m.ShowCreditNoteReversalReasonForm(It.IsAny<string>()))
				.Returns(Tuple.Create(new ZString("IOB"), new ZString("Incorrect Organisation Billed")));
			guiWrapper
				.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
				It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()))
				.Returns(ZDialogResult.OK); //exit message

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights, true);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				Assert("Should not continue reversing logic", !result.Item1);
				Assert("Should save the approval factory", result.Item2);
				guiWrapper.Verify();
				AssertEquals("Should not set user when no approval occurs", 0, invoice1.ApprovingUserPKList.Count);
				AssertEquals("Should not set user when no approval occurs", 0, invoice2.ApprovingUserPKList.Count);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice1.ApprovalDate);
				AssertEquals("Should not set any approval date", ZDateTime.Empty, invoice2.ApprovalDate);
			}
		}

		public void TestCreditNoteReversalReasonFormPopUp_ForMutlipleTransactionsShowCreditNoteReversalReasonForm()
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
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(
				Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request)
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(),
					It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);

			guiWrapper
				.Setup(m => m.ShowCreditNoteReversalReasonForm(It.IsAny<string>()))
				.Returns(Tuple.Create(new ZString("IOB"), new ZString("Incorrect Organisation Billed")));
			guiWrapper.Verify(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>()), Times.Never);
			guiWrapper
				.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
				It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()))
				.Returns(ZDialogResult.OK);//exit message

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1, invoice2 }, checkLevelSecurityRights, true);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				guiWrapper.Verify();
			}
		}

		public void TestCreditNoteReversalReasonFormPopUp_ForSingleTransactionsShowApprovalRequestForm()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			Factory.Save();

			var approvalFactory = new BusinessObjectFactory();
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(approvalFactory);
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), AccTransactionHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.Revenue);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true); //user wish to create approval request)
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(),
					It.IsAny<ARCreditNoteApprovalRequest[]>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Verify(m => m.ShowCreditNoteReversalReasonForm(It.IsAny<string>()), Times.Never);
			guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.OK);
			guiWrapper
				.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
				It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()))
				.Returns(ZDialogResult.OK);//exit message

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			Tuple<bool, bool> performTransactionLevelAuthorization()
			{
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => false;
				return PerformLevelAuthorizationForReversing(guiWrapper.Object, new[] { invoice1 }, checkLevelSecurityRights, true);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var result = performTransactionLevelAuthorization();
				guiWrapper.Verify();
			}
		}

		AuthorizationModeAndSettings GetAuthorisationConfigSetting()
		{
			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			return setting;
		}

		ARCreditNoteApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
			approvalRequest.UpdateApprovalUserAndStatus(ApprovingUser.GS_Code, approvalStatus);
			approvalRequest.XP_ApprovalDate = TestApprovalDate;
			approvalRequest.XP_ReasonDescription = "Invoice Reverse Testing";
			return approvalRequest;
		}

		ZDateTime TestApprovalDate => new ZDateTime(2019, 2, 2);

		GlbStaff ApprovingUser
		{
			get
			{
				if (fApprovingUser == null)
				{
					SecurityTestObject.CreateTestUser(true, "", "tst", "testUser", "password");
					fApprovingUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "tst"));
				}
				return fApprovingUser;
			}
		}
		GlbStaff fApprovingUser;

		protected override ZString GetDefaultXP_ParentTableCode() => AccTransactionHeaderSchema.Constants.Prefix;

		Tuple<bool, bool> PerformLevelAuthorizationForReversing(IPostingJobTransactionsApprovalGUIProvider postingGUIProvider, InvoicingBase[] transactions, Func<InvoicingBase, bool> checkLevelSecurityRights, bool isTestingCreditNoteReversalReasonForm = false)
		{
			var helper = new ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(postingGUIProvider);
			helper.IsTestingCreditNoteReversalReasonForm = isTestingCreditNoteReversalReasonForm;
			helper.CheckLevelSecurityRights_ForTestOnly = checkLevelSecurityRights; //Can not mock security certificates
			return helper.PerformLevelAuthorizationForReversing(transactions);
		}
	}
}
