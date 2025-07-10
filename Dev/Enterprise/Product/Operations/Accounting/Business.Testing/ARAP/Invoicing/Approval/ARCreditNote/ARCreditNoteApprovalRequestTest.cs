using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalRequest))]
	public class ARCreditNoteApprovalRequestTest : InvoicingBaseApprovalRequestTest<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public void TestAuthorisationLevelRequiredForDisplay()
		{
			var request = Factory.New<ARCreditNoteApprovalRequest>();
			request.XP_ApprovalStatus = GenApprovalRequestApprovalStatus.Requested;
			request.PostingDetails.ApprovingOption = AuthorizationMode.Codes.Default;
			request.PostingDetails.MaxAuthorisationLevelRequired = 1;
			AssertEquals("1", request.MaxAuthorisationLevelRequiredForDisplay);
			AssertEquals("1", request.NextAuthorisationLevelRequiredForDisplay);
			request.PostingDetails.MaxAuthorisationLevelRequired = 0;
			AssertEquals("", request.MaxAuthorisationLevelRequiredForDisplay);
			AssertEquals("", request.NextAuthorisationLevelRequiredForDisplay);
		}

		public void TestApprovingModeForDisplay()
		{
			var request = Factory.New<ARCreditNoteApprovalRequest>();
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SingleLogin;
			AssertEquals("DEF - Single Approval Required", request.ApprovingModeForDisplay);
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			AssertEquals("TWO - Two Approvers Required", request.ApprovingModeForDisplay);
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			AssertEquals("SEQ - Sequential Approvals", request.ApprovingModeForDisplay);
		}

		public void TestReverseInvoiceWhenTwoApproversEnforced()
		{
			var setting = GetAuthorisationConfigSetting(Enterprise.Core.Constants.AuthorizationMode.Codes.TwoApprovers);
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, TestObjectCreator.GetRandomString(5), null, 0m, null, TestObjectCreator.AUD, 15, TestObjectCreator.AALSHI);
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 15, TestObjectCreator.AALSHI);
				invoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, invoice.PK));
				invoice.IsCreatingCreditNoteForReversal = true;

				var request = Factory.New<ARCreditNoteApprovalRequest>();
				request.ChangeApprovalTypeForInvoiceReversal();
				request.Initialize(new[] { invoice }, invoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);

				AssertEquals("Two Level 1", request.ApprovingOptionForDisplay);
			}
		}

		public void TestJobNumber_JobBranch_JobDepartment_TextBoxCaptions_ForParentTableJH()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			Factory.Save();

			AssertEquals(JobHeaderSchema.Constants.Prefix, request.XP_ParentTableCode);
			AssertEquals("Job Number", request.JobNumberTextBoxResString.Caption);
			AssertEquals("Job Branch", request.JobBranchTextBoxResString.Caption);
			AssertEquals("Job Department", request.JobDepartmentTextBoxResString.Caption);
		}

		public void TestJobNumber_JobBranch_JobDepartment_TextBoxCaptions_ForParentTableAH()
		{
			var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(10m);
			Factory.Save();

			AssertEquals(AccTransactionHeaderSchema.Constants.Prefix, request.XP_ParentTableCode);
			AssertEquals("Invoice Number", request.JobNumberTextBoxResString.Caption);
			AssertEquals("Invoice Branch", request.JobBranchTextBoxResString.Caption);
			AssertEquals("Invoice Department", request.JobDepartmentTextBoxResString.Caption);
		}

		public void TestJobNumber_JobBranch_JobDepartment_TextBoxCaptions_ForParentTableXP_ParentIsLinkedToJob()
		{
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
				Factory.Save();

				var childRequest = request.ChildRequests[0];
				AssertEquals(GenApprovalRequestSchema.Constants.Prefix, childRequest.XP_ParentTableCode);
				AssertEquals("Job Number", childRequest.JobNumberTextBoxResString.Caption);
				AssertEquals("Job Branch", childRequest.JobBranchTextBoxResString.Caption);
				AssertEquals("Job Department", childRequest.JobDepartmentTextBoxResString.Caption);
			}
		}

		public void TestJobNumber_JobBranch_JobDepartment_TextBoxCaptions_ForParentTableXP_ParentIsLinkedToInvoice()
		{
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(10m);
				Factory.Save();

				var childRequest = request.ChildRequests[0];
				AssertEquals(GenApprovalRequestSchema.Constants.Prefix, childRequest.XP_ParentTableCode);
				AssertEquals("Invoice Number", childRequest.JobNumberTextBoxResString.Caption);
				AssertEquals("Invoice Branch", childRequest.JobBranchTextBoxResString.Caption);
				AssertEquals("Invoice Department", childRequest.JobDepartmentTextBoxResString.Caption);
			}
		}

		public void TestJobNumber_JobBranch_JobDepartment_TextBoxCaptions_DefaultValue()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			request.XP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals(JobConsolSchema.Constants.Prefix, request.XP_ParentTableCode);
			AssertEquals("Job/Invoice Number", request.JobNumberTextBoxResString.Caption);
			AssertEquals("Job/Invoice Branch", request.JobBranchTextBoxResString.Caption);
			AssertEquals("Job/Invoice Department", request.JobDepartmentTextBoxResString.Caption);

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
				request.XP_ParentTableCode = JobConsolSchema.Constants.Prefix;
				Factory.Save();

				AssertEquals(JobConsolSchema.Constants.Prefix, request.XP_ParentTableCode);

				var childRequest = request.ChildRequests[0];
				AssertEquals(GenApprovalRequestSchema.Constants.Prefix, childRequest.XP_ParentTableCode);

				AssertEquals("Job/Invoice Number", childRequest.JobNumberTextBoxResString.Caption);
				AssertEquals("Job/Invoice Branch", childRequest.JobBranchTextBoxResString.Caption);
				AssertEquals("Job/Invoice Department", childRequest.JobDepartmentTextBoxResString.Caption);
			}
		}

		public void TestRealtedRequestsCollection_RealtedRequestGridTitleAndDescription()
		{
			var expectedErrorMessageForReasonCode = "Please enter a Reason Code.";
			var expectedErrorMessageForDescription = "Please enter a Description.";
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.Validation.ValidateAll();

				var relatedChildRequests = TestApprovalRequest.RelatedRequests;
				AssertEquals("Parent request has one child request", 1, relatedChildRequests.Count);
				var relatedChildRequest = relatedChildRequests[0];
				AssertEquals(TestApprovalRequest.PK, relatedChildRequest.XP_ParentID);
				AssertHasError(TestApprovalRequest.XP_ReasonDescriptionInfo, expectedErrorMessageForDescription);
				AssertNoError(relatedChildRequest.XP_ReasonDescriptionInfo, expectedErrorMessageForDescription);
				AssertHasError(TestApprovalRequest.XP_ReasonCodeInfo, expectedErrorMessageForReasonCode);
				AssertNoError(relatedChildRequest.XP_ReasonCodeInfo, expectedErrorMessageForReasonCode);
				AssertEquals("Related line-level requests", TestApprovalRequest.RelatedRequestsdGridTitle.Caption);
				AssertEquals("This request can be approved once the associated line-level requests are in the Approved status", TestApprovalRequest.RelatedRequestsGridDescription);

				var childRequest = TestApprovalRequest.ChildRequests[0];
				var relatedParentRequests = childRequest.RelatedRequests;
				AssertEquals("Child request has one parent", 1, relatedParentRequests.Count);
				var relatedParentRequest = relatedParentRequests[0];
				AssertEquals(TestApprovalRequest.PK, relatedParentRequest.PK);
				AssertEquals("Related header request", childRequest.RelatedRequestsdGridTitle.Caption);
				AssertEquals("This request is associated with the following header request. Credit note can be posted once the header request is approved.", childRequest.RelatedRequestsGridDescription);
			}
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			var b = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			a.PostingDetails.MaxAmountToApprove = 10;
			a.PostingDetails.Charges.AddNew();
			b.PostingDetails.MaxAmountToApprove = 100;
			b.PostingDetails.Charges.AddNew();
			b.PostingDetails.Charges.AddNew();
			Assert(a.IsPostingActionTheSame(b));
			Assert(b.IsPostingActionTheSame(a));

			a.PostingDetails.PostingOption = "PST";
			Assert(!a.IsPostingActionTheSame(b));
			Assert(!b.IsPostingActionTheSame(a));

			b.PostingDetails.PostingOption = "PST";
			Assert(a.IsPostingActionTheSame(b));
			Assert(b.IsPostingActionTheSame(a));
		}

		public override void TestUpdateApprovalUserAndStatus()
		{
			var currentLoginSetting = new AuthorizationModeAndSettings();
			currentLoginSetting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			var valuesForTest = currentLoginSetting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 10;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK, currentLoginSetting);

			var branch = TestObjectCreator.CreateBranch("B01", GlbCompany.CurrentCompany);
			var department = TestObjectCreator.CreateDepartment("D01");
			Factory.Save();

			var otherLoginSetting = new AuthorizationModeAndSettings();
			otherLoginSetting.AuthorizationMode = Constants.AuthorizationMode.Codes.Default;
			valuesForTest = otherLoginSetting.AuthorisationSettings;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SixthApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branch.PK.ToGuid(), department.PK.ToGuid(), otherLoginSetting);

			var request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			request.PostingDetails.MaxAmountToApprove = 5;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 5;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			request.PostingDetails.MaxAmountToApprove = 100;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 100;
			request.XP_GB_JobBranch = ZGuid.Empty;
			request.XP_GE_JobDepartment = ZGuid.Empty;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 300;
			request.XP_GB_JobBranch = ZGuid.Empty;
			request.XP_GE_JobDepartment = ZGuid.Empty;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals("", request.XP_GS_NKApprovingUser3);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("", request.XP_GS_NKApprovingUser3);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad3", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("ad3", request.XP_GS_NKApprovingUser3);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 150;
			request.XP_GB_JobBranch = branch.PK;
			request.XP_GE_JobDepartment = department.PK;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 300;
			request.XP_GB_JobBranch = branch.PK;
			request.XP_GE_JobDepartment = department.PK;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals("", request.XP_GS_NKApprovingUser3);
			AssertEquals("", request.XP_GS_NKApprovingUser4);
			AssertEquals("", request.XP_GS_NKApprovingUser5);
			AssertEquals("", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("", request.XP_GS_NKApprovingUser3);
			AssertEquals("", request.XP_GS_NKApprovingUser4);
			AssertEquals("", request.XP_GS_NKApprovingUser5);
			AssertEquals("", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad3", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("ad3", request.XP_GS_NKApprovingUser3);
			AssertEquals("", request.XP_GS_NKApprovingUser4);
			AssertEquals("", request.XP_GS_NKApprovingUser5);
			AssertEquals("", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad4", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("ad3", request.XP_GS_NKApprovingUser3);
			AssertEquals("ad4", request.XP_GS_NKApprovingUser4);
			AssertEquals("", request.XP_GS_NKApprovingUser5);
			AssertEquals("", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad5", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("ad3", request.XP_GS_NKApprovingUser3);
			AssertEquals("ad4", request.XP_GS_NKApprovingUser4);
			AssertEquals("ad5", request.XP_GS_NKApprovingUser5);
			AssertEquals("", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			AssertNullOrEmpty(request.XP_ApprovalDate.ToString());

			request.UpdateApprovalUserAndStatus("ad6", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals("ad3", request.XP_GS_NKApprovingUser3);
			AssertEquals("ad4", request.XP_GS_NKApprovingUser4);
			AssertEquals("ad5", request.XP_GS_NKApprovingUser5);
			AssertEquals("ad6", request.XP_GS_NKApprovingUser6);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Rejected);
			AssertEquals("ad1", request.XP_GS_NKApprovingUser1);
			AssertEquals("", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = "E";
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Rejected);
			AssertEquals("E", request.XP_GS_NKApprovingUser1);
			AssertEquals("ad2", request.XP_GS_NKApprovingUser2);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());

			request = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			request.XP_GS_NKApprovingUser1 = "A";
			request.XP_GS_NKApprovingUser2 = "B";
			request.XP_GS_NKApprovingUser3 = "C";
			request.XP_ApprovalStatus = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			request.PostingDetails.MaxAmountToApprove = 300;
			request.XP_GB_JobBranch = branch.PK;
			request.XP_GE_JobDepartment = department.PK;
			request.UpdateApprovalUserAndStatus("D", Constants.GenApprovalRequestApprovalStatus.Rejected);
			AssertEquals("A", request.XP_GS_NKApprovingUser1);
			AssertEquals("B", request.XP_GS_NKApprovingUser2);
			AssertEquals("C", request.XP_GS_NKApprovingUser3);
			AssertEquals("D", request.XP_GS_NKApprovingUser4);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, request.XP_ApprovalStatus);
			AssertNotNullOrEmpty(request.XP_ApprovalDate.ToString());
		}

		public override void TestPostingOptionForDisplay()
		{
			foreach (JobInvoicingPostingOption x in Enum.GetValues(typeof(JobInvoicingPostingOption)))
			{
				TestApprovalRequest.PostingDetails.PostingOption = x.ToString();
				AssertEquals(AccountingUtils.ConvertPostingOptionToHumanReadableName(x, TestApprovalRequest.IsConsolRelated), TestApprovalRequest.PostingOptionForDisplay);
				AssertEquals(x, TestApprovalRequest.PostingOption);
			}
		}

		public override void TestValidationType()
		{
			AssertType(typeof(ARTransactionApprovalRequestValidation), TestApprovalRequest.Validation);
		}

		public void TestApprovingOptionForDisplay()
		{
			TestApprovalRequest.PostingDetails.ApprovingOption = "ONE";
			AssertEquals("Single Level 0", TestApprovalRequest.ApprovingOptionForDisplay);
			TestApprovalRequest.PostingDetails.ApprovingOption = "TWO";
			AssertEquals("Two Level 0", TestApprovalRequest.ApprovingOptionForDisplay);
			TestApprovalRequest.PostingDetails.ApprovingOption = "SEQ";
			AssertEquals("Seq 0", TestApprovalRequest.ApprovingOptionForDisplay);
			TestApprovalRequest.PostingDetails.ApprovingOption = "XYZ";
			AssertEquals("XYZ", TestApprovalRequest.ApprovingOptionForDisplay);
		}

		public override void TestReferenceType()
		{
			base.TestReferenceType();
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(10m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 10m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
				AssertEquals("Job", TestApprovalRequest.ReferenceType);
				AssertEquals("Approval Request", TestApprovalRequest.ChildRequests[0].ReferenceType);
			}
		}

		public void TestLineLevelApprovalWithZeroTotalAmount()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(0m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 0m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(job.TablePrefix, TestApprovalRequest.XP_ParentTableCode);
				AssertEquals(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, TestApprovalRequest.XP_ReasonCode);
				AssertEquals("reason desc 01", TestApprovalRequest.XP_ReasonDescription);
				AssertEquals(job.PK, TestApprovalRequest.XP_ParentID);
				AssertEquals(job.JH_GB, TestApprovalRequest.XP_GB_JobBranch);
				AssertEquals(job.JH_GE, TestApprovalRequest.XP_GE_JobDepartment);
				AssertEquals(1, TestApprovalRequest.PostingDetails.Charges.Count);

				AssertEquals(0, TestApprovalRequest.ChildRequests.Count);
			}
		}

		public void TestLineLevelApprovalWithSingleChildRequest()
		{
			AssertNotNull(BranchBNE);

			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(30m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 5m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);

				var creditNote2 = TestObjectCreator.CreateARCreditNote(50m, TestObjectCreator.LocalClient2.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 50m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1, creditNote2 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(job.TablePrefix, TestApprovalRequest.XP_ParentTableCode);
				AssertEquals(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, TestApprovalRequest.XP_ReasonCode);
				AssertEquals("reason desc 01", TestApprovalRequest.XP_ReasonDescription);
				AssertEquals(job.PK, TestApprovalRequest.XP_ParentID);
				AssertEquals(job.JH_GB, TestApprovalRequest.XP_GB_JobBranch);
				AssertEquals(job.JH_GE, TestApprovalRequest.XP_GE_JobDepartment);
				AssertEquals(3, TestApprovalRequest.PostingDetails.Charges.Count);

				var childRequests = TestApprovalRequest.ChildRequests;
				AssertEquals(1, childRequests.Count);
				AssertEquals("XP", childRequests[0].XP_ParentTableCode);
				AssertEquals(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, childRequests[0].XP_ReasonCode);
				AssertEquals("reason desc 01", childRequests[0].XP_ReasonDescription);
				AssertEquals(TestApprovalRequest.PK, childRequests[0].XP_ParentID);
				AssertEquals(BranchBNE.PK, childRequests[0].XP_GB_JobBranch);
				AssertEquals(TestObjectCreator.FIADepartment.PK, childRequests[0].XP_GE_JobDepartment);
				AssertEquals(3, childRequests[0].PostingDetails.Charges.Count);
				AssertEquals(TestApprovalRequest.PostingDetails.PostingOption, childRequests[0].PostingDetails.PostingOption);
				AssertEquals(TestApprovalRequest.JobNumber, childRequests[0].JobNumber);
			}
		}

		public void TestLineLevelApprovalWithMultipleChildRequest()
		{
			AssertNotNull(BranchBNE);
			AssertNotNull(BranchSYD);

			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FISDepartment.PK.ToGuid(), new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.SequentialApprovers });
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchSYD.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.Default });
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchSYD.PK.ToGuid(), TestObjectCreator.FISDepartment.PK.ToGuid(), new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(50m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 5m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 10m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchSYD.PK, TestObjectCreator.FIADepartment.PK, 15m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchSYD.PK, TestObjectCreator.FISDepartment.PK, 20m);

				var creditNote2 = TestObjectCreator.CreateARCreditNote(50m, TestObjectCreator.LocalClient2.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 5m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 10m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchSYD.PK, TestObjectCreator.FIADepartment.PK, 15m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchSYD.PK, TestObjectCreator.FISDepartment.PK, 20m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1, creditNote2 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(job.TablePrefix, TestApprovalRequest.XP_ParentTableCode);
				AssertEquals(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, TestApprovalRequest.XP_ReasonCode);
				AssertEquals("reason desc 01", TestApprovalRequest.XP_ReasonDescription);
				AssertEquals(job.PK, TestApprovalRequest.XP_ParentID);
				AssertEquals(job.JH_GB, TestApprovalRequest.XP_GB_JobBranch);
				AssertEquals(job.JH_GE, TestApprovalRequest.XP_GE_JobDepartment);
				AssertEquals(8, TestApprovalRequest.PostingDetails.Charges.Count);

				var childRequests = TestApprovalRequest.ChildRequests;
				AssertEquals(4, childRequests.Count);

				var childBNEFIA = childRequests.Where(x => x.XP_GB_JobBranch == BranchBNE.PK && x.XP_GE_JobDepartment == TestObjectCreator.FIADepartment.PK).ToArray()[0];
				var childBNEFIS = childRequests.Where(x => x.XP_GB_JobBranch == BranchBNE.PK && x.XP_GE_JobDepartment == TestObjectCreator.FISDepartment.PK).ToArray()[0];
				var childSYDFIA = childRequests.Where(x => x.XP_GB_JobBranch == BranchSYD.PK && x.XP_GE_JobDepartment == TestObjectCreator.FIADepartment.PK).ToArray()[0];
				var childSYDFIS = childRequests.Where(x => x.XP_GB_JobBranch == BranchSYD.PK && x.XP_GE_JobDepartment == TestObjectCreator.FISDepartment.PK).ToArray()[0];

				AssertChildRequestForBranchDepartmentCombination(childBNEFIA, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, -5m, "TWO");
				AssertChildRequestForBranchDepartmentCombination(childBNEFIS, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, -10m, "SEQ");
				AssertChildRequestForBranchDepartmentCombination(childSYDFIA, BranchSYD.PK, TestObjectCreator.FIADepartment.PK, -15m, "ONE");
				AssertChildRequestForBranchDepartmentCombination(childSYDFIS, BranchSYD.PK, TestObjectCreator.FISDepartment.PK, -20m, "TWO");
			}
		}

		void AssertChildRequestForBranchDepartmentCombination(ARCreditNoteApprovalRequest childRequest, ZGuid branchPK, ZGuid departmentPK, decimal chargeAmount, ZString approvingOption)
		{
			AssertEquals("XP", childRequest.XP_ParentTableCode);
			AssertEquals(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, childRequest.XP_ReasonCode);
			AssertEquals("reason desc 01", childRequest.XP_ReasonDescription);
			AssertEquals(TestApprovalRequest.PK, childRequest.XP_ParentID);
			AssertEquals(branchPK, childRequest.XP_GB_JobBranch);
			AssertEquals(departmentPK, childRequest.XP_GE_JobDepartment);
			AssertEquals(2, childRequest.PostingDetails.Charges.Count);
			Assert(childRequest.PostingDetails.Charges.Cast<ARCreditNoteApprovalRequestChargeDetails>().All(x => x.LocalSellAmount == chargeAmount));
			AssertEquals(TestApprovalRequest.PostingDetails.PostingOption, childRequest.PostingDetails.PostingOption);
			AssertEquals(TestApprovalRequest.JobNumber, childRequest.JobNumber);
			AssertEquals(approvingOption, childRequest.PostingDetails.ApprovingOption);
		}

		public void TestIsParentRequest()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				Assert(TestApprovalRequest.IsParentRequest);
				Assert(!TestApprovalRequest.ChildRequests[0].IsParentRequest);
			}
		}

		public override void TestIsAllowedToApproveOrCancel()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("TWO"));

			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				var childRequest = TestApprovalRequest.ChildRequests[0];
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childRequest.XP_ApprovalStatus);
				Assert("Parent can be canceled", TestApprovalRequest.IsAllowedToCancelRequest);
				Assert("Child cannot be canceled", !childRequest.IsAllowedToCancelRequest);
				Assert("Child can be approved directly", childRequest.IsAllowedToApproveRequest);
				Assert("Parent can be approved only when all childs are approved", !TestApprovalRequest.IsAllowedToApproveRequest);

				childRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				Assert("Parent can be approved only when all childs are approved", TestApprovalRequest.IsAllowedToApproveRequest);
			}
		}

		public override void TestIsAllowedToChangeStatus()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("TWO"));

			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				var childRequest = TestApprovalRequest.ChildRequests[0];
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childRequest.XP_ApprovalStatus);

				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Requested));
				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled));
				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Rejected));
				Assert(!TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved));
				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Posted));
				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Error));

				Assert(childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Requested));
				Assert(!childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled));
				Assert(childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Rejected));
				Assert(childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved));
				Assert(childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Posted));
				Assert(childRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Error));

				childRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				Assert(TestApprovalRequest.IsAllowedToChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved));
			}
		}

		public void TestApproveChildAndParentRequest()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);

				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
					var childRequest = TestApprovalRequest.ChildRequests[0];
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childRequest.XP_ApprovalStatus);

					Assert("No security right to approve child", !new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, childRequest).Approve());
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childRequest.XP_ApprovalStatus);

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, true);

					Assert("cannot approve parent if child is not approved", !new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, TestApprovalRequest).Approve());
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);

					Env.Security.ResetData(null, staff.PK.ToGuid(), BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), Env.CurrentCompanyPK, true);
					Assert("approve child first", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, childRequest).Approve());
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, childRequest.XP_ApprovalStatus);

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, true);
					Env.Security.ResetData(null, staff.PK.ToGuid(), currentBranch, currentDepartment, Env.CurrentCompanyPK, false);

					Assert("parent can now be approved", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, TestApprovalRequest).Approve());
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, TestApprovalRequest.XP_ApprovalStatus);
				}
			}
		}

		public void TestPrePopulateChildRequestApproverUser1WhenModeIsSEQ()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), GetAuthorisationConfigSetting("SEQ")))
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);

				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
					var childRequest = TestApprovalRequest.ChildRequests[0];
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childRequest.XP_ApprovalStatus);
					AssertEquals("", TestApprovalRequest.XP_GS_NKApprovingUser1);
					AssertEquals("", childRequest.XP_GS_NKApprovingUser1);

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, true);
					Env.Security.ResetData(null, staff.PK.ToGuid(), BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), Env.CurrentCompanyPK, true);

					new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, childRequest).Approve(true);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals("tst", childRequest.XP_GS_NKApprovingUser1);
				}
			}
		}

		public void TestPreApproveChildRequestWhenModeIsSEQ()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("SEQ", true)))
			{
				var currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				var currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, true);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FEADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, true);

				using (Env.SetTemporaryUserContext("newuser", currentBranch, currentDepartment))
				{
					var creditNote1 = TestObjectCreator.CreateARCreditNote(12m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 8m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FEADepartment.PK, 4m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					AssertEquals(2, TestApprovalRequest.ChildRequests.Count);
					var childRequestFIA = TestApprovalRequest.ChildRequests.First(x => x.XP_GE_JobDepartment == TestObjectCreator.FIADepartment.PK);
					var childRequestFEA = TestApprovalRequest.ChildRequests.First(x => x.XP_GE_JobDepartment == TestObjectCreator.FEADepartment.PK);
					AssertEquals("Seq 1,2", TestApprovalRequest.ApprovingOptionForDisplay);
					AssertEquals("Seq 1", childRequestFIA.ApprovingOptionForDisplay);
					AssertEquals("Seq 0", childRequestFEA.ApprovingOptionForDisplay);
					AssertEquals("Expect parent request in Requested Status", GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals("Expect FIA child request in Requested Status because it's SEQ 1", GenApprovalRequestApprovalStatus.Requested, childRequestFIA.XP_ApprovalStatus);
					AssertEquals("Expect FEA child request in Approved Status because it's SEQ 0", GenApprovalRequestApprovalStatus.Approved, childRequestFEA.XP_ApprovalStatus);
					Assert("Expect parent request has blank approver", TestApprovalRequest.XP_GS_NKApprovingUser1.IsEmpty);
					AssertEquals("Expect FIA child request has approver 1 set", "tst", childRequestFIA.XP_GS_NKApprovingUser1);
					AssertEquals("Expect FEA child request has approver 1 set", "tst", childRequestFEA.XP_GS_NKApprovingUser1);
				}
			}
		}

		public void TestPostParentRequest()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;

				var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 100m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
				var childRequest = TestApprovalRequest.ChildRequests[0];
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, childRequest.XP_ApprovalStatus);

				Assert("Approve the parent", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, TestApprovalRequest).Approve());
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, TestApprovalRequest.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, childRequest.XP_ApprovalStatus);

				TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, TestApprovalRequest.XP_ApprovalStatus);
				AssertEquals("childs should be also posted", Constants.GenApprovalRequestApprovalStatus.Posted, childRequest.XP_ApprovalStatus);
			}
		}

		public void TestRejectParentRequest()
		{
			AuthorizationModeAndSettings settings = GetAuthorisationConfigSetting();
			settings.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();

				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);

				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);

					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 75m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					var childRequests = TestApprovalRequest.ChildRequests;
					AssertEquals(2, childRequests.Count);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					Assert(childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested));
					childRequests[0].XP_GS_NKApprovingUser1 = "E";
					childRequests[1].XP_GS_NKApprovingUser1 = ZString.Empty;

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, true);

					Assert("Reject Parent", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, TestApprovalRequest).Reject());
					AssertEquals("Parent is rejected", Constants.GenApprovalRequestApprovalStatus.Rejected, TestApprovalRequest.XP_ApprovalStatus);
					Assert("All childs are canceled", childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled));
					AssertEquals("Parent and child have the same approval date", TestApprovalRequest.XP_ApprovalDate, childRequests[0].XP_ApprovalDate);
					AssertEquals("Childs have the same approval date", childRequests[0].XP_ApprovalDate, childRequests[1].XP_ApprovalDate);
					AssertEquals("E", childRequests[0].XP_GS_NKApprovingUser1);
					AssertEquals("tst", childRequests[0].XP_GS_NKApprovingUser2);
					AssertEquals("tst", childRequests[1].XP_GS_NKApprovingUser1);
					AssertEquals("", childRequests[1].XP_GS_NKApprovingUser2);
					AssertEquals("tst", TestApprovalRequest.XP_GS_NKApprovingUser1);
					AssertEquals("", TestApprovalRequest.XP_GS_NKApprovingUser2);
				}
			}
		}

		public void TestRejectChildRequest()
		{
			AuthorizationModeAndSettings settings = GetAuthorisationConfigSetting();
			settings.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 75m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					var childRequests = TestApprovalRequest.ChildRequests;
					AssertEquals(2, childRequests.Count);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					Assert(childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested));
					childRequests[0].XP_GS_NKApprovingUser1 = "E";
					childRequests[1].XP_GS_NKApprovingUser1 = ZString.Empty;

					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;

					Assert("Reject child 1", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, childRequests[0]).Reject());
					AssertEquals("Parent is rejected", Constants.GenApprovalRequestApprovalStatus.Rejected, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals("Other child is canceled", Constants.GenApprovalRequestApprovalStatus.Cancelled, childRequests[1].XP_ApprovalStatus);
					AssertEquals("rejected child is still rejected", Constants.GenApprovalRequestApprovalStatus.Rejected, childRequests[0].XP_ApprovalStatus);
					AssertEquals("Parent and child have the same approval date", TestApprovalRequest.XP_ApprovalDate, childRequests[0].XP_ApprovalDate);
					AssertEquals("Childs have the same approval date", childRequests[0].XP_ApprovalDate, childRequests[1].XP_ApprovalDate);
					AssertEquals("E", childRequests[0].XP_GS_NKApprovingUser1);
					AssertEquals("tst", childRequests[0].XP_GS_NKApprovingUser2);
					AssertEquals("tst", childRequests[1].XP_GS_NKApprovingUser1);
					AssertEquals("", childRequests[1].XP_GS_NKApprovingUser2);
					AssertEquals("tst", TestApprovalRequest.XP_GS_NKApprovingUser1);
					AssertEquals("", TestApprovalRequest.XP_GS_NKApprovingUser2);
				}
			}
		}

		public void TestCancelChildAndParentRequest()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);

				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 75m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					var childRequests = TestApprovalRequest.ChildRequests;
					AssertEquals(2, childRequests.Count);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					Assert(childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested));

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, true);
					Assert("Cancel child is not allowed", !new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, childRequests[0]).Cancel());
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					Assert(childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested));

					Assert("Cancel parent", new ARCreditNoteApprovalBulk(Factory, creditNote1.SecurityOverrideProvider, TestApprovalRequest).Cancel());
					AssertEquals("Parent is canceled", Constants.GenApprovalRequestApprovalStatus.Cancelled, TestApprovalRequest.XP_ApprovalStatus);
					Assert("Other childs are canceled", childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled));
				}
			}
		}

		public void TestChildRequestWithPositiveAmountShouldNotBeCreated()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

					var creditNote1 = TestObjectCreator.CreateARCreditNote(1000m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 2000m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, -1000m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					var childRequests = TestApprovalRequest.ChildRequests;
					AssertEquals("create child only for negative amounts", 1, childRequests.Count);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					Assert(childRequests.All(x => x.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested));
				}
			}
		}

		public void TestChildRequestWithSmallerAmountShouldBeApprovedIfPossible()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			var setting = new AuthorizationModeAndSettings();
			var paymentSixLevelAuthorisationSettingsCollection = setting.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = paymentSixLevelAuthorisationSettingsCollection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 1000;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = paymentSixLevelAuthorisationSettingsCollection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 1000;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
			{
				Security.Testing.SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
				using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

					var creditNote1 = TestObjectCreator.CreateARCreditNote(1600m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 500m);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 1100m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					var childRequests = TestApprovalRequest.ChildRequests;
					AssertEquals(2, childRequests.Count);
					var childOverLimit = childRequests.First(x => x.PostingDetails.Charges[0].LocalSellAmount == -1100m);
					var childUnderLimit = childRequests.First(x => x.PostingDetails.Charges[0].LocalSellAmount == -500m);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, TestApprovalRequest.XP_ApprovalStatus);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, childOverLimit.XP_ApprovalStatus);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, childUnderLimit.XP_ApprovalStatus);
				}
			}
		}

		public void TestDeleteParentShouldDeleteChilds()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FISDepartment.PK, 75m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				var childRequests = TestApprovalRequest.ChildRequests;
				AssertEquals(2, childRequests.Count);

				TestApprovalRequest.Delete();
				AssertEquals(0, childRequests.Count);
			}
		}

		public void TestCollectionDelete()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			{
				var origianlFirstLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
				var origianlSecondLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				try
				{
					var creditNote1 = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
					TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 25m);

					TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
					TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
					TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

					AssertEquals(1, TestApprovalRequest.ChildRequests.Count);

					AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

					TestApprovalRequest.Delete();

					AssertEquals("Should have no enumeration errors", 0, ExceptionReporterTestListener.Instance.Count);
				}
				finally
				{
					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = origianlFirstLevelApproval;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = origianlSecondLevelApproval;
				}
			}
		}

		public void TestIsConsolRelated()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(10m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 10m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
				Assert(!TestApprovalRequest.ChildRequests.First().IsTransactionRelated);
				Assert(!TestApprovalRequest.ChildRequests.First().IsJobRelated);
				Assert(!TestApprovalRequest.ChildRequests.First().IsConsolRelated);
				Assert(TestApprovalRequest.ChildRequests.First().IsApprovalRequestRelated);
			}
		}

		public void TestChildRequestProperties()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote1 = TestObjectCreator.CreateARCreditNote(10m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 10m);

				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote1 }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				TestApprovalRequest.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				TestApprovalRequest.XP_ReasonDescription = "reason desc 01";

				AssertEquals(1, TestApprovalRequest.ChildRequests.Count);
				AssertEquals(TestApprovalRequest.XP_ReasonCode, TestApprovalRequest.ChildRequests.First().XP_ReasonCode);
				AssertEquals(TestApprovalRequest.XP_ReasonDescription, TestApprovalRequest.ChildRequests.First().XP_ReasonDescription);
				AssertEquals(Constants.GenApprovalRequestApprovalType.ARCreditNote, TestApprovalRequest.XP_ApprovalType);
				AssertEquals(Constants.GenApprovalRequestApprovalType.ARCreditNote, TestApprovalRequest.ChildRequests.First().XP_ApprovalType);

				TestApprovalRequest.ChangeApprovalTypeForInvoiceReversal();

				AssertEquals(Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal, TestApprovalRequest.XP_ApprovalType);
				AssertEquals(Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal, TestApprovalRequest.ChildRequests.First().XP_ApprovalType);
			}
		}

		GlbBranch BranchBNE => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
		GlbBranch BranchSYD => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));

		AuthorizationModeAndSettings GetAuthorisationConfigSetting(string mode = "", bool includeLevelNone = false)
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
			if (includeLevelNone)
			{
				upToPaymentAuthorisationSettings = collection.AddNew();
				upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				upToPaymentAuthorisationSettings.Amount = 5;
				upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			}

			if (!string.IsNullOrWhiteSpace(mode))
			{
				result.AuthorizationMode = mode;
			}

			return result;
		}

		public void TestXP_ApprovalRequestData()
		{
			TestApprovalRequest.PostingDetails.PostingOption = "PostLocalClientCharges";
			TestApprovalRequest.PostingDetails.ApprovingOption = "ONE";
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 1;
			TestApprovalRequest.PostingDetails.InvoiceDate = new ZDateTime("2018-02-14");
			TestApprovalRequest.PostingDetails.PostDate = new ZDateTime("2018-02-15");
			TestApprovalRequest.PostingDetails.MaxAmountToApprove = 200.12M;
			TestApprovalRequest.PostingDetails.Description = "descAH";
			TestApprovalRequest.PostingDetails.InvoiceTerm = Constants.InvoiceTerms.FromCustomsClearanceDate;
			TestApprovalRequest.PostingDetails.InvoiceTermDays = 6;

			var charge1 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1.JobNumber = "S00001234";
			charge1.ChargeCode = "FRT";
			charge1.Branch = "SYD";
			charge1.Department = "FES";
			charge1.SellAccount = "ABIGAS";
			charge1.SellCurrency = "USD";
			charge1.OSSellAmount = 100.1M;
			charge1.LocalSellAmount = 100.1M;
			charge1.InvoiceType = "FIN";
			charge1.SupplyType = "LOC";

			var charge2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2.JobNumber = "S00001234";
			charge2.ChargeCode = "BAF";
			charge2.Branch = "SYD";
			charge2.Department = "FES";
			charge2.SellAccount = "ABIGAS";
			charge2.SellCurrency = "USD";
			charge2.OSSellAmount = 200.12M;
			charge2.LocalSellAmount = 200.12M;
			charge2.InvoiceType = "FIN";
			charge2.Description = "LineDesc";
			charge2.SupplyType = "LOA";
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			charge2.AccInvMsgPK = taxMessage.PK;
			charge2.TaxDate = new ZDate(2020, 3, 13);
			Factory.Save();

			var testApprovalRequest_inNewFactory = new BusinessObjectFactory().Load<ARCreditNoteApprovalRequest>(TestApprovalRequest.PK);
			string expectedXML = $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><ApprovingOption>ONE</ApprovingOption><InvoiceDate>14-Feb-18 00:00:00</InvoiceDate><PostDate>15-Feb-18 00:00:00</PostDate><MaxAuthorisationLevelRequired>1</MaxAuthorisationLevelRequired><MaxAmountToApprove>200.12</MaxAmountToApprove><Description>descAH</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>6</InvoiceTermDays><ArrayOfChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description /><AccInvMsgPK>00000000-0000-0000-0000-000000000000</AccInvMsgPK><TaxDate /><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType><OSTaxAmount>0</OSTaxAmount><LocalTaxAmount>0</LocalTaxAmount><ExchangeRate>0</ExchangeRate><TaxCode /><SupplyType>LOC</SupplyType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description>LineDesc</Description><AccInvMsgPK>{taxMessage.PK}</AccInvMsgPK><TaxDate>13-Mar-20</TaxDate><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType><OSTaxAmount>0</OSTaxAmount><LocalTaxAmount>0</LocalTaxAmount><ExchangeRate>0</ExchangeRate><TaxCode /><SupplyType>LOA</SupplyType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			this.AssertXMLEqualsByDiff("XML saved in XP_ApprovalRequestData", expectedXML, Encoding.Unicode.GetString(testApprovalRequest_inNewFactory.XP_ApprovalRequestData));

			string oldXML = $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><ApprovingOption>ONE</ApprovingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><Description>descAH</Description><InvoiceTerm>CUS</InvoiceTerm><InvoiceTermDays>6</InvoiceTermDays><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description /><AccInvMsgPK>00000000-0000-0000-0000-000000000000</AccInvMsgPK><TaxDate /><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType><SupplyType>LOC</SupplyType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><Description>LineDesc</Description><AccInvMsgPK>{taxMessage.PK}</AccInvMsgPK><TaxDate>13-Mar-20</TaxDate><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType><SupplyType>LOA</SupplyType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			testApprovalRequest_inNewFactory.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(oldXML);

			AssertEquals("PostingOption", "PostLocalClientCharges", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			AssertEquals("ApprovingOption", "ONE", testApprovalRequest_inNewFactory.PostingDetails.ApprovingOption);
			AssertEquals("MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			AssertEquals("Charges.Count", 2, testApprovalRequest_inNewFactory.PostingDetails.Charges.Count);

			var charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals("ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals("Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals("Department", "FES", charge_inNewFactory.Department);
			AssertEquals("SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals("SellCurrency", "USD", charge_inNewFactory.SellCurrency);
			AssertEquals("OSSellAmount", 100.1M, charge_inNewFactory.OSSellAmount);
			AssertEquals("LocalSellAmount", 100.1M, charge_inNewFactory.LocalSellAmount);
			AssertEquals("InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals("SupplyType", "LOC", charge_inNewFactory.SupplyType);
			AssertEquals(string.Empty, charge_inNewFactory.Description);
			AssertEquals(ZDate.Empty, charge_inNewFactory.TaxDate);
			AssertEquals(ZGuid.Empty, charge_inNewFactory.AccInvMsgPK);

			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals("ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals("Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals("Department", "FES", charge_inNewFactory.Department);
			AssertEquals("SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals("SellCurrency", "USD", charge_inNewFactory.SellCurrency);
			AssertEquals("OSSellAmount", 200.12M, charge_inNewFactory.OSSellAmount);
			AssertEquals("LocalSellAmount", 200.12M, charge_inNewFactory.LocalSellAmount);
			AssertEquals("InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals("SupplyType", "LOA", charge_inNewFactory.SupplyType);
			AssertEquals("LineDesc", charge_inNewFactory.Description);
			AssertEquals(new ZDate(2020, 3, 13), charge_inNewFactory.TaxDate);
			AssertEquals(taxMessage.PK, charge_inNewFactory.AccInvMsgPK);

			TestApprovalRequest.PostingDetails.PostingOption = "PostAllCharges";
			TestApprovalRequest.PostingDetails.ApprovingOption = "TWO";
			charge1.OSSellAmount = 300.05M;
			charge2.SellCurrency = "GBP";
			Factory.Save();
			var messagePrefix = "Update through data refresh bus: ";
			AssertEquals(messagePrefix + "PostingOption", "PostAllCharges", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			AssertEquals(messagePrefix + "ApprovingOption", "TWO", testApprovalRequest_inNewFactory.PostingDetails.ApprovingOption);
			AssertEquals(messagePrefix + "MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals(messagePrefix + "SellCurrency", "USD", charge_inNewFactory.SellCurrency);
			AssertEquals(messagePrefix + "OSSellAmount", 300.05M, charge_inNewFactory.OSSellAmount);
			AssertEquals(messagePrefix + "LocalSellAmount", 100.1M, charge_inNewFactory.LocalSellAmount);
			AssertEquals(messagePrefix + "InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals(messagePrefix + "SupplyType", "LOC", charge_inNewFactory.SupplyType);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals(messagePrefix + "SellCurrency", "GBP", charge_inNewFactory.SellCurrency);
			AssertEquals(messagePrefix + "OSSellAmount", 200.12M, charge_inNewFactory.OSSellAmount);
			AssertEquals(messagePrefix + "LocalSellAmount", 200.12M, charge_inNewFactory.LocalSellAmount);
			AssertEquals(messagePrefix + "InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals(messagePrefix + "SupplyType", "LOA", charge_inNewFactory.SupplyType);

			testApprovalRequest_inNewFactory.PostingDetails.PostingOption = "PostAgentCharges";
			TestApprovalRequest.PostingDetails.PostingOption = "PostLocalCharges";
			charge1.OSSellAmount = 100M;
			charge2.SellCurrency = "USD";
			Factory.Save();
			messagePrefix = "Update through data refresh bus should be done as object in current factory is changed: ";
			AssertEquals(messagePrefix + "PostingOption", "PostAgentCharges", testApprovalRequest_inNewFactory.PostingDetails.PostingOption);
			AssertEquals(messagePrefix + "MaxAmountToApprove", 200.12M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[0];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "FRT", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals(messagePrefix + "SellCurrency", "USD", charge_inNewFactory.SellCurrency);
			AssertEquals(messagePrefix + "OSSellAmount", 300.05M, charge_inNewFactory.OSSellAmount);
			AssertEquals(messagePrefix + "LocalSellAmount", 100.1M, charge_inNewFactory.LocalSellAmount);
			AssertEquals(messagePrefix + "InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals(messagePrefix + "SupplyType", "LOC", charge_inNewFactory.SupplyType);
			charge_inNewFactory = testApprovalRequest_inNewFactory.PostingDetails.Charges[1];
			AssertEquals(messagePrefix + "JobNumber", "S00001234", charge_inNewFactory.JobNumber);
			AssertEquals(messagePrefix + "ChargeCode", "BAF", charge_inNewFactory.ChargeCode);
			AssertEquals(messagePrefix + "Branch", "SYD", charge_inNewFactory.Branch);
			AssertEquals(messagePrefix + "Department", "FES", charge_inNewFactory.Department);
			AssertEquals(messagePrefix + "SellAccount", "ABIGAS", charge_inNewFactory.SellAccount);
			AssertEquals(messagePrefix + "SellCurrency", "GBP", charge_inNewFactory.SellCurrency);
			AssertEquals(messagePrefix + "OSSellAmount", 200.12M, charge_inNewFactory.OSSellAmount);
			AssertEquals(messagePrefix + "LocalSellAmount", 200.12M, charge_inNewFactory.LocalSellAmount);
			AssertEquals(messagePrefix + "InvoiceType", "FIN", charge_inNewFactory.InvoiceType);
			AssertEquals(messagePrefix + "SupplyType", "LOA", charge_inNewFactory.SupplyType);
		}

		public void TestInitialize_ApprovalRequestDepartmentBranch_Invoice()
		{
			var creditnote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.AUD, 1, null);
			var request = Factory.New<ARCreditNoteApprovalRequest>();
			ARCreditNote[] creditNotes = CreateNotes();
			request.Initialize(creditNotes, creditnote.PK, "AH", JobInvoicingPostingOption.Disbursement);

			AssertEquals("Approval Request Branch should equal to Invoice's branch", creditnote.Branch.PK, request.XP_GB_JobBranch);
			AssertEquals("Approval Request Department should equal to Invoice's department", creditnote.Department.PK, request.XP_GE_JobDepartment);
		}

		public void TestInitialize_ApprovalRequestDepartmentBranch_Job()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			ARCreditNote[] creditNotes = CreateNotes();
			TestApprovalRequest.Initialize(creditNotes, job1.PK, "JH", JobInvoicingPostingOption.Disbursement);
			AssertEquals("Approval Request Branch should equal to Job's branch", job1.Branch.PK, TestApprovalRequest.XP_GB_JobBranch);
			AssertEquals("Approval Request Department should equal to Job's department", job1.Department.PK, TestApprovalRequest.XP_GE_JobDepartment);
		}

		public void TestInitialize_ApprovalRequestDepartmentBranch_JobConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			TestApprovalRequest.Initialize(Array.Empty<InvoicingBase>(), consol.CostSupporter.PK, "JK", JobInvoicingPostingOption.Disbursement);
			AssertEquals("Should have no branch when there is no transaction to lift off of", ZGuid.Empty, TestApprovalRequest.XP_GB_JobBranch);
			AssertEquals("Should have no department when there is no transaction to lift off of", ZGuid.Empty, TestApprovalRequest.XP_GE_JobDepartment);

			ARCreditNote creditNote = CreateNotes()[0];
			var branch = Factory.New<GlbBranch>();
			var department = Factory.New<GlbDepartment>();
			creditNote.AH_GB = ZGuid.Empty;
			creditNote.AH_GE = ZGuid.Empty;
			TestApprovalRequest.Initialize(new[] { creditNote }, consol.CostSupporter.PK, "JK", JobInvoicingPostingOption.Disbursement);
			AssertEquals("Should have no branch when transaction has no branch", ZGuid.Empty, TestApprovalRequest.XP_GB_JobBranch);
			AssertEquals("Should have no department when transaction has no department", ZGuid.Empty, TestApprovalRequest.XP_GE_JobDepartment);

			creditNote.AH_GB = branch.PK;
			creditNote.AH_GE = department.PK;
			TestApprovalRequest.Initialize(new[] { creditNote }, consol.CostSupporter.PK, "JK", JobInvoicingPostingOption.Disbursement);
			AssertEquals("Branch should equal transaction's branch", branch.PK, TestApprovalRequest.XP_GB_JobBranch);
			AssertEquals("Department should equal transaction's department", department.PK, TestApprovalRequest.XP_GE_JobDepartment);
		}

		public void TestInitialize_ApprovalRequestDepartmentBranch_Empty()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "JOB2";
			ARCreditNote[] creditNotes = CreateNotes();
			TestApprovalRequest.Initialize(creditNotes, job1.PK, "UN", JobInvoicingPostingOption.Disbursement);
			AssertEquals("Approval Request Branch should be empty", ZGuid.Empty, TestApprovalRequest.XP_GB_JobBranch);
			AssertEquals("Approval Request Department should be empty", ZGuid.Empty, TestApprovalRequest.XP_GE_JobDepartment);
		}

		ARCreditNote[] CreateNotes()
		{
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			creditNote1.AH_OH = TestObjectCreator.LocalClient.PK;
			creditNote1.AH_LocalExTaxAmount = 5M;
			ARCreditNote creditNote2 = Factory.New<ARCreditNote>();
			creditNote2.AH_TransactionCategory = InvoiceTypesList.Codes.DestinationChargesInvoice;
			creditNote2.AH_OH = TestObjectCreator.LocalClient2.PK;
			creditNote2.AH_LocalExTaxAmount = 15M;
			ARCreditNote[] creditNotes = { creditNote1, creditNote2 };
			return creditNotes;
		}

		public void TestInitialize()
		{
			var registryValue = TestObjectCreator.CreateAuthorizationModeAndSettings(10m, 20m, "SEQ");
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			creditNote1.AH_OH = TestObjectCreator.LocalClient.PK;
			creditNote1.AH_LocalExTaxAmount = 5M;
			InvoicingLineBase creditNoteLine = (InvoicingLineBase)creditNote1.Lines.AddNew();
			creditNoteLine.AL_JH = job1.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC1.PK;
			creditNoteLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNoteLine.AL_GE = TestObjectCreator.FEADepartment.PK;
			creditNoteLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			creditNoteLine.AL_OSAmount = -10M;
			creditNoteLine.AL_LineAmount = -5M;
			creditNoteLine.AL_SupplyType = "LOC";
			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB2";
			ARCreditNote creditNote2 = Factory.New<ARCreditNote>();
			creditNote2.AH_TransactionCategory = InvoiceTypesList.Codes.DestinationChargesInvoice;
			creditNote2.AH_OH = TestObjectCreator.LocalClient2.PK;
			creditNote2.AH_LocalExTaxAmount = 15M;
			creditNoteLine = (InvoicingLineBase)creditNote2.Lines.AddNew();
			creditNoteLine.AL_JH = job2.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC2.PK;
			creditNoteLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNoteLine.AL_GE = TestObjectCreator.FEADepartment.PK;
			creditNoteLine.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			creditNoteLine.AL_OSAmount = -20M;
			creditNoteLine.AL_LineAmount = -15M;
			creditNoteLine.AL_SupplyType = "LOA";
			ARCreditNote[] creditNotes = { creditNote1, creditNote2 };
			ZGuid expectedParentID = ZGuid.NewZGuid();
			TestApprovalRequest.Initialize(creditNotes, expectedParentID, "XP", JobInvoicingPostingOption.Disbursement, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.FEADepartment.GE_Code);

			AssertEquals("XP_ParentID", expectedParentID, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", "XP", TestApprovalRequest.XP_ParentTableCode);
			AssertEquals("PostingDetails.PostingOption", "Disbursement", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingDetails.MaxAuthorisationLevelRequired", 1, TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired);
			AssertEquals("PostingOptionForDisplay", "Post Disbursement Charges only", TestApprovalRequest.PostingOptionForDisplay);
			AssertEquals("PostingDetails.ApprovingOption", "SEQ", TestApprovalRequest.PostingDetails.ApprovingOption);
			AssertEquals("PostingDetails.MaxAmountToApprove", 15M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

			var charge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", "JOB1", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.FEADepartment.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.LocalClient.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "USD", charge.SellCurrency);
			AssertEquals("OSSellAmount", -10M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", -5M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.FinalInvoice, charge.InvoiceType);
			AssertEquals("SupplyType", "LOC", charge.SupplyType);

			charge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", "JOB2", charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", TestObjectCreator.NonCurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", TestObjectCreator.FEADepartment.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.LocalClient2.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "GBP", charge.SellCurrency);
			AssertEquals("OSSellAmount", -20M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", -15M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.DestinationChargesInvoice, charge.InvoiceType);
			AssertEquals("SupplyType", "LOA", charge.SupplyType);
		}

		public void TestNextAuthorisationLevelRequired()
		{
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;

			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 6;
			AssertEquals(6, TestApprovalRequest.MaxAuthorisationLevelRequired);

			TestApprovalRequest.XP_GS_NKApprovingUser1 = ZString.Empty;

			var statusList = new string[] { Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Cancelled,
											Constants.GenApprovalRequestApprovalStatus.Posted, Constants.GenApprovalRequestApprovalStatus.Rejected,
											Constants.GenApprovalRequestApprovalStatus.Error };
			statusList.ForEach(x =>
			{
				TestApprovalRequest.XP_ApprovalStatus = x;
				AssertEquals(0, TestApprovalRequest.NextAuthorisationLevelRequired);
			});

			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.Default;
			AssertEquals(6, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.TwoApprovers;
			AssertEquals(6, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.SequentialApprovers;
			AssertEquals(1, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.XP_GS_NKApprovingUser1 = "LV1";
			AssertEquals(2, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.XP_GS_NKApprovingUser2 = "LV2";
			AssertEquals(3, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.XP_GS_NKApprovingUser3 = "LV3";
			AssertEquals(4, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.XP_GS_NKApprovingUser4 = "LV4";
			AssertEquals(5, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.XP_GS_NKApprovingUser5 = "LV5";
			AssertEquals(6, TestApprovalRequest.NextAuthorisationLevelRequired);

			TestApprovalRequest.XP_GS_NKApprovingUser1 = "LV1";
			TestApprovalRequest.XP_GS_NKApprovingUser2 = "";
			TestApprovalRequest.XP_GS_NKApprovingUser3 = "";
			TestApprovalRequest.XP_GS_NKApprovingUser4 = "";
			TestApprovalRequest.XP_GS_NKApprovingUser5 = "";
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 1;
			AssertEquals(1, TestApprovalRequest.MaxAuthorisationLevelRequired);
			AssertEquals(1, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 0;
			AssertEquals(0, TestApprovalRequest.MaxAuthorisationLevelRequired);
			AssertEquals(0, TestApprovalRequest.NextAuthorisationLevelRequired);
		}

		public void TestReasonCode()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote = Factory.New<ARCreditNote>();
				creditNote.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
				creditNote.AH_OH = TestObjectCreator.LocalClient.PK;
				creditNote.AH_LocalExTaxAmount = 100m;
				var creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
				creditNoteLine.AL_JH = job.PK;
				creditNoteLine.AL_AC = TestObjectCreator.FRT.PK;
				creditNoteLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
				creditNoteLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				creditNoteLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
				creditNoteLine.AL_OSAmount = -100m;
				creditNoteLine.AL_LineAmount = -100m;

				var expectedReasonCode = "IDE";
				creditNote.AH_ReceiptType = expectedReasonCode;
				TestApprovalRequest.Initialize(new ARCreditNote[] { creditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				AssertEquals(expectedReasonCode, TestApprovalRequest.XP_ReasonCode);
			}
		}

		public void TestInitializeWhenChargeCodeIsNull()
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			creditNote1.AH_OH = TestObjectCreator.LocalClient.PK;
			creditNote1.AH_LocalExTaxAmount = 5M;
			InvoicingLineBase creditNoteLine = (InvoicingLineBase)creditNote1.Lines.AddNew();
			creditNoteLine.AL_JH = job1.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC1.PK;
			creditNoteLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNoteLine.AL_GE = TestObjectCreator.FESDepartment.PK;
			creditNoteLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			creditNoteLine.AL_OSAmount = -10M;
			creditNoteLine.AL_LineAmount = -5M;

			InvoicingLineBase anotherCNLine = (InvoicingLineBase)creditNote1.Lines.AddNew();
			AccChargeCode.GLPostingAccounts glPostingAccounts = creditNoteLine.ChargeCode.GetGLPostingAccounts(creditNoteLine.Department, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL");
			anotherCNLine.AL_AG = glPostingAccounts.RevenueAccount;
			anotherCNLine.AL_AT = creditNoteLine.AL_AT;
			anotherCNLine.AL_A9_VATClass = creditNoteLine.AL_A9_VATClass;
			anotherCNLine.AL_OSExTaxAmount = -creditNoteLine.AL_OSExTaxAmount;
			anotherCNLine.AL_LocalExTaxAmount = -creditNoteLine.AL_LocalExTaxAmount;
			anotherCNLine.AL_GB = creditNoteLine.AL_GB;
			anotherCNLine.AL_GE = creditNoteLine.AL_GE;
			anotherCNLine.AL_OSTaxAmount = -creditNoteLine.AL_OSTaxAmount;
			anotherCNLine.AL_LocalTaxAmount = -creditNoteLine.AL_LocalTaxAmount;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB2";
			ARCreditNote creditNote2 = Factory.New<ARCreditNote>();
			creditNote2.AH_TransactionCategory = InvoiceTypesList.Codes.DestinationChargesInvoice;
			creditNote2.AH_OH = TestObjectCreator.LocalClient2.PK;
			creditNote2.AH_LocalExTaxAmount = 15M;
			creditNoteLine = (InvoicingLineBase)creditNote2.Lines.AddNew();
			creditNoteLine.AL_JH = job2.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC2.PK;
			creditNoteLine.AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			creditNoteLine.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNoteLine.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			creditNoteLine.AL_OSAmount = -20M;
			creditNoteLine.AL_LineAmount = -15M;

			anotherCNLine = (InvoicingLineBase)creditNote2.Lines.AddNew();
			glPostingAccounts = creditNoteLine.ChargeCode.GetGLPostingAccounts(creditNoteLine.Department, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL");
			anotherCNLine.AL_AG = glPostingAccounts.RevenueAccount;
			anotherCNLine.AL_AT = creditNoteLine.AL_AT;
			anotherCNLine.AL_A9_VATClass = creditNoteLine.AL_A9_VATClass;
			anotherCNLine.AL_OSExTaxAmount = -creditNoteLine.AL_OSExTaxAmount;
			anotherCNLine.AL_LocalExTaxAmount = -creditNoteLine.AL_LocalExTaxAmount;
			anotherCNLine.AL_GB = creditNoteLine.AL_GB;
			anotherCNLine.AL_GE = creditNoteLine.AL_GE;
			anotherCNLine.AL_OSTaxAmount = -creditNoteLine.AL_OSTaxAmount;
			anotherCNLine.AL_LocalTaxAmount = -creditNoteLine.AL_LocalTaxAmount;

			ARCreditNote[] creditNotes = { creditNote1, creditNote2 };
			ZGuid expectedParentID = ZGuid.NewZGuid();
			TestApprovalRequest.Initialize(creditNotes, expectedParentID, "XX", JobInvoicingPostingOption.Agent);

			AssertEquals("XP_ParentID", expectedParentID, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", "XX", TestApprovalRequest.XP_ParentTableCode);
			AssertEquals("PostingDetails.PostingOption", "Agent", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "Post Overseas Agent Charges", TestApprovalRequest.PostingOptionForDisplay);
			AssertEquals("PostingDetails.Charges.Count", 4, TestApprovalRequest.PostingDetails.Charges.Count);

			AssertEquals("Precondition: AccChargeCode", creditNote1.Lines[0].ChargeCode.AC_Code, TestApprovalRequest.PostingDetails.Charges[0].ChargeCode);
			AssertEquals("Precondition: AL_AG Code", creditNote1.Lines[1].GLHeader.AG_AccountNum, TestApprovalRequest.PostingDetails.Charges[1].ChargeCode);
			AssertEquals("Precondition: AccChargeCode", creditNote2.Lines[0].ChargeCode.AC_Code, TestApprovalRequest.PostingDetails.Charges[2].ChargeCode);
			AssertEquals("Precondition: AL_AG Code", creditNote2.Lines[1].GLHeader.AG_AccountNum, TestApprovalRequest.PostingDetails.Charges[3].ChargeCode);

			AssertEquals("Precondition: Job Number", creditNote1.Lines[0].Job.JH_JobNum, TestApprovalRequest.PostingDetails.Charges[0].JobNumber);
			AssertEquals("Precondition: No Job Number", string.Empty, TestApprovalRequest.PostingDetails.Charges[1].JobNumber);
			AssertEquals("Precondition: Job Number", creditNote2.Lines[0].Job.JH_JobNum, TestApprovalRequest.PostingDetails.Charges[2].JobNumber);
			AssertEquals("Precondition: No Job Number", string.Empty, TestApprovalRequest.PostingDetails.Charges[3].JobNumber);
		}

		public void TestFormattedChargeDetails()
		{
			AssertEquals("", TestApprovalRequest.FormattedChargeDetails);

			var currencyWitoutDecimals = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 1)).RX_Code;

			var charge1 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1.SellAccount = "ORG1";
			charge1.SellCurrency = "CUR1";
			charge1.InvoiceType = "INT1";
			charge1.OSSellAmount = 1M;
			var charge2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2.SellAccount = "ORG1";
			charge2.SellCurrency = currencyWitoutDecimals;
			charge2.InvoiceType = "INT1";
			charge2.OSSellAmount = 10M;
			var charge3 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge3.SellAccount = "ORG1";
			charge3.SellCurrency = "CUR1";
			charge3.InvoiceType = "INT2";
			charge3.OSSellAmount = 100M;

			var charge3_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge3_2.SellAccount = "ORG1";
			charge3_2.SellCurrency = "CUR1";
			charge3_2.InvoiceType = "INT2";
			charge3_2.OSSellAmount = 200M;
			var charge1_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge1_2.SellAccount = "ORG1";
			charge1_2.SellCurrency = "CUR1";
			charge1_2.InvoiceType = "INT1";
			charge1_2.OSSellAmount = 2M;
			var charge2_2 = TestApprovalRequest.PostingDetails.Charges.AddNew();
			charge2_2.SellAccount = "ORG1";
			charge2_2.SellCurrency = currencyWitoutDecimals;
			charge2_2.InvoiceType = "INT1";
			charge2_2.OSSellAmount = 20M;
			Factory.Save();

			var testApprovalRequest_InAnotherFactory = new BusinessObjectFactory().Load<ARCreditNoteApprovalRequest>(TestApprovalRequest.PK);
			AssertEquals(string.Format("ORG1 CUR1 INT1: 3.00, ORG1 {0} INT1: 30, ORG1 CUR1 INT2: 300.00", currencyWitoutDecimals), testApprovalRequest_InAnotherFactory.FormattedChargeDetails);

			charge2_2.InvoiceType = "INT2";
			charge3_2.OSSellAmount = 300M;
			Factory.Save();

			AssertEquals(string.Format("ORG1 CUR1 INT1: 3.00, ORG1 {0} INT1: 10, ORG1 CUR1 INT2: 400.00, ORG1 {0} INT2: 20", currencyWitoutDecimals), testApprovalRequest_InAnotherFactory.FormattedChargeDetails);
		}

		public void TestFormattedChargeDetails_ARCreditNoteForReversal()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice1.PK));
			invoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			TestApprovalRequest.ChangeApprovalTypeForInvoiceReversal();
			TestApprovalRequest.Initialize(new[] { invoice1 }, invoice1.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			AssertEquals("AALSHI AUD FIN: -30.00", TestApprovalRequest.FormattedChargeDetails);
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				GenApprovalRequestSchema.PK,
				GenApprovalRequestSchema.XP_ApprovalStatus,
				GenApprovalRequestSchema.XP_ReasonDescription
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, GenApprovalRequestSchema.PK.TableSchema.SqlSchemaName, GenApprovalRequestSchema.PK.TableName, columns);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();
			var approvalForTest = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			AssertEquals("XP_ApprovalType", Constants.GenApprovalRequestApprovalType.ARCreditNote, approvalForTest.XP_ApprovalType);
		}

		public void TestChangeApprovalTypeForInvoiceReversal()
		{
			var approvalForTest = (ARCreditNoteApprovalRequest)GetNewBusinessObject();
			AssertEquals("Initial approcal type should be RCN", Constants.GenApprovalRequestApprovalType.ARCreditNote, approvalForTest.XP_ApprovalType);
			approvalForTest.ChangeApprovalTypeForInvoiceReversal();
			AssertEquals("Now it should be RIR", Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal, approvalForTest.XP_ApprovalType);
		}

		public void TestInitialize_ARCreditNoteForReversal()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, invoice1.PK));
			invoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, invoice1.PK));
			invoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			TestApprovalRequest.ChangeApprovalTypeForInvoiceReversal();
			TestApprovalRequest.Initialize(new[] { invoice1 }, invoice1.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);

			AssertEquals("XP_ParentID", invoice1.PK, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, TestApprovalRequest.XP_ParentTableCode);
			AssertEquals("PostingDetails.PostingOption", "Revenue", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "Post All Revenue Charges", TestApprovalRequest.PostingOptionForDisplay);
			AssertEquals("PostingDetails.MaxAmountToApprove", 30M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

			var charge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", job.JH_JobNum, charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC1.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", job.Department.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.AALSHI.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "AUD", charge.SellCurrency);
			AssertEquals("OSSellAmount", 10M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", 10M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.FinalInvoice, charge.InvoiceType);

			charge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", job.JH_JobNum, charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.CC2.AC_Code, charge.ChargeCode);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", job.Department.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.AALSHI.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "AUD", charge.SellCurrency);
			AssertEquals("OSSellAmount", 20M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", 20M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.FinalInvoice, charge.InvoiceType);
		}

		public void TestInitializeWhenChargeCodeIsNull_ARCreditNoteForReversal()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1m, 10m, 0m, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1m, 20m, 0m, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			TestApprovalRequest.ChangeApprovalTypeForInvoiceReversal();
			TestApprovalRequest.Initialize(new[] { invoice1 }, invoice1.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);

			AssertEquals("XP_ParentID", invoice1.PK, TestApprovalRequest.XP_ParentID);
			AssertEquals("XP_ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, TestApprovalRequest.XP_ParentTableCode);
			AssertEquals("PostingDetails.PostingOption", "Revenue", TestApprovalRequest.PostingDetails.PostingOption);
			AssertEquals("PostingOptionForDisplay", "Post All Revenue Charges", TestApprovalRequest.PostingOptionForDisplay);
			AssertEquals("PostingDetails.MaxAmountToApprove", 30M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.Charges.Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);

			var charge = TestApprovalRequest.PostingDetails.Charges[0];
			AssertEquals("JobNumber", ZString.Empty, charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.GLHeader1.AG_AccountNum, charge.ChargeCode);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.AALSHI.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "AUD", charge.SellCurrency);
			AssertEquals("OSSellAmount", 10M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", 10M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.FinalInvoice, charge.InvoiceType);

			charge = TestApprovalRequest.PostingDetails.Charges[1];
			AssertEquals("JobNumber", ZString.Empty, charge.JobNumber);
			AssertEquals("ChargeCode", TestObjectCreator.GLHeader2.AG_AccountNum, charge.ChargeCode);
			AssertEquals("Branch", GlbBranch.CurrentBranch.GB_Code, charge.Branch);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.GE_Code, charge.Department);
			AssertEquals("SellAccount", TestObjectCreator.AALSHI.OH_Code, charge.SellAccount);
			AssertEquals("SellCurrency", "AUD", charge.SellCurrency);
			AssertEquals("OSSellAmount", 20M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", 20M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", InvoiceTypesList.Codes.FinalInvoice, charge.InvoiceType);
		}

		public void TestFixedPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(TestObjectCreator.AUD.RX_Code))
			{
				var placeofSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeofSupplyCodeType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeofSupplyCode);

				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge1.JR_SellPlaceOfSupply = placeofSupplyCode;
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
				charge2.JR_SellPlaceOfSupply = placeofSupplyCode;
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNote("AR001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m);
				creditNote1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, creditNote1.PK));
				creditNote1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, creditNote1.PK));
				creditNote1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				TestApprovalRequest.Initialize(new[] { creditNote1 }, creditNote1.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
				AssertEquals("PostingDetails Line Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
				AssettFPOS(TestApprovalRequest.PostingDetails.Charges[0].PlaceOfSupply, TestApprovalRequest.PostingDetails.Charges[0].PlaceOfSupplyType);
				AssettFPOS(TestApprovalRequest.PostingDetails.Charges[1].PlaceOfSupply, TestApprovalRequest.PostingDetails.Charges[1].PlaceOfSupplyType);

				var amendingCreditNote = Factory.New<ARCreditNote>();
				TestApprovalRequest.InitializeTransactionLinesFromPostingDetails(amendingCreditNote);
				AssertEquals("Invoice Line Count", 2, amendingCreditNote.Lines.Count);
				AssettFPOS(amendingCreditNote.AH_PlaceOfSupply, amendingCreditNote.AH_PlaceOfSupplyType);
				AssettFPOS(amendingCreditNote.Lines[0].AL_PlaceOfSupply, amendingCreditNote.Lines[1].AL_PlaceOfSupplyType);
				AssettFPOS(amendingCreditNote.Lines[1].AL_PlaceOfSupply, amendingCreditNote.Lines[1].AL_PlaceOfSupplyType);

				void AssettFPOS(ZString fpos, ZString fposType)
				{
					AssertEquals("PlaceOfSupply", placeofSupplyCode, fpos);
					AssertEquals("placeofSupplyCodeType", placeofSupplyCodeType, fposType);
				}
			}
		}

		public void TestInitializeTransactionLinesWithEmptyJobNumber()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var jobNumber = job.JH_JobNum;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.AH_JH = job.PK;
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
			TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.NonAccrualChargeCode, TestObjectCreator.AUD, 1m, "desc", 10m);

			TestApprovalRequest.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			AssertEquals("PostingDetails Line Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
			AssertEquals(jobNumber, TestApprovalRequest.PostingDetails.Charges[0].JobNumber);
			AssertEquals(ZString.Empty, TestApprovalRequest.PostingDetails.Charges[1].JobNumber);

			var amendingCreditNote = ((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
			TestApprovalRequest.InitializeTransactionLinesFromPostingDetails(amendingCreditNote);
			AssertEquals("Invoice Line Count", 2, amendingCreditNote.Lines.Count);
			AssertEquals(job.PK, amendingCreditNote.Lines[0].AL_JH);
			AssertEquals(ZGuid.Empty, amendingCreditNote.Lines[1].AL_JH);
		}

		public void TestInitializeTransactionLinesWithPeriodicInvoice()
		{
			AssertInitializeTransactionLinesWithPeriodicOrConsolInvoice(true);
		}

		public void TestInitializeTransactionLinesWithConsolInvoice()
		{
			AssertInitializeTransactionLinesWithPeriodicOrConsolInvoice(false);
		}

		void AssertInitializeTransactionLinesWithPeriodicOrConsolInvoice(bool isPeriodicInvoice)
		{
			var job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var jobNumber1 = job1.JH_JobNum;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var job2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var jobNumber2 = job2.JH_JobNum;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			if (isPeriodicInvoice)
			{
				arInvoice.AH_TransactionCategory = "FID";
				Assert(arInvoice.IsPeriodicInvoice);
			}
			else
			{
				arInvoice.AH_ConsolidatedInvoiceRef = "C00001001";
				Assert(arInvoice.IsConsolInvoice);
			}
			AssertEquals(ZGuid.Empty, arInvoice.AH_JH);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, arInvoice.PK));

			TestApprovalRequest.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			AssertEquals("PostingDetails Line Count", 2, TestApprovalRequest.PostingDetails.Charges.Count);
			AssertEquals(jobNumber1, TestApprovalRequest.PostingDetails.Charges[0].JobNumber);
			AssertEquals(jobNumber2, TestApprovalRequest.PostingDetails.Charges[1].JobNumber);

			var amendingCreditNote = ((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
			TestApprovalRequest.InitializeTransactionLinesFromPostingDetails(amendingCreditNote);
			AssertEquals(ZGuid.Empty, amendingCreditNote.AH_JH);
			AssertEquals("Invoice Line Count", 2, amendingCreditNote.Lines.Count);
			AssertEquals(job1.PK, amendingCreditNote.Lines[0].AL_JH);
			AssertEquals(job2.PK, amendingCreditNote.Lines[1].AL_JH);
		}

		public void TestInitializeTransactionLinesWithSupplyType()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			charge.JR_SellSupplyType = "LOC";
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateRevenueLine(charge, invoice.PK);
			line.AL_SupplyType = charge.JR_SellSupplyType;
			invoice.Lines.Add(line);
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			TestApprovalRequest.ChangeApprovalTypeForInvoiceReversal();
			TestApprovalRequest.Initialize(new[] { invoice }, invoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);

			AssertEquals("PostingDetails Line Count", 1, TestApprovalRequest.PostingDetails.Charges.Count);
			AssertEquals("LOC", TestApprovalRequest.PostingDetails.Charges[0].SupplyType);

			var amendingCreditNote = ((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
			TestApprovalRequest.InitializeTransactionLinesFromPostingDetails(amendingCreditNote);

			AssertEquals(ZGuid.Empty, amendingCreditNote.AH_JH);
			AssertEquals("Invoice Line Count", 1, amendingCreditNote.Lines.Count);
			AssertEquals("LOC", amendingCreditNote.Lines[0].AL_SupplyType);
		}

		public void TestIsFinalApproval_SingleApprover()
		{
			PrepareTestApprovalRequest();
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.Default;

			TestApprovalRequest.UpdateIsFinalApproval();
			Assert(TestApprovalRequest.IsFinalApproval);
		}

		public void TestIsFinalApproval_DoubleApprover_LastApprover()
		{
			PrepareTestApprovalRequest();
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.TwoApprovers;
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 1;
			TestApprovalRequest.XP_GS_NKApprovingUser1 = "US1";

			TestApprovalRequest.UpdateIsFinalApproval();
			Assert(TestApprovalRequest.IsFinalApproval);
		}

		public void TestIsFinalApproval_DoubleApprover_NotLastApprover()
		{
			PrepareTestApprovalRequest();
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.TwoApprovers;
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 1;

			AssertEquals(1, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.UpdateIsFinalApproval();
			Assert("IsFinalApproval should be false", !TestApprovalRequest.IsFinalApproval);
		}

		public void TestIsFinalApproval_SequentialApprover_LastApprover()
		{
			PrepareTestApprovalRequest();
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.SequentialApprovers;
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 4;
			TestApprovalRequest.XP_GS_NKApprovingUser1 = "US1";
			TestApprovalRequest.XP_GS_NKApprovingUser2 = "US2";
			TestApprovalRequest.XP_GS_NKApprovingUser3 = "US3";

			TestApprovalRequest.UpdateIsFinalApproval();
			Assert(TestApprovalRequest.IsFinalApproval);
		}

		public void TestIsFinalApproval_SequentialApprover_NotLastApprover()
		{
			PrepareTestApprovalRequest();
			TestApprovalRequest.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.SequentialApprovers;
			TestApprovalRequest.PostingDetails.MaxAuthorisationLevelRequired = 4;
			TestApprovalRequest.XP_GS_NKApprovingUser1 = "US1";
			TestApprovalRequest.XP_GS_NKApprovingUser2 = "US2";

			AssertEquals(3, TestApprovalRequest.NextAuthorisationLevelRequired);
			TestApprovalRequest.UpdateIsFinalApproval();
			Assert("IsFinalApproval should be false", !TestApprovalRequest.IsFinalApproval);
		}

		void PrepareTestApprovalRequest()
		{
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
		}

		protected override string GetExpectedEmailSubjectForTestSendEmail(ARCreditNoteApprovalRequest request) => $"AR Credit Note Approval request - {request.JobNumber}";
	}
}
