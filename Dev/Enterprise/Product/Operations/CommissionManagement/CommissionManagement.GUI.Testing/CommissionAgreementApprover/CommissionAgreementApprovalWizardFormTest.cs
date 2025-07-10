using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionAgreementApprovalWizardForm))]
	public class CommissionAgreementApprovalWizardFormTest : ZFormBasherTest
	{
		#region Options

		public void TestFromDateDateEdit_Visibility()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();

				wizard.FromType = "";
				AssertEquals("Precondition", false, wizard.FromTypeRequiresDate);

				AssertEquals(false, form.FromDateDateEdit_Exposed.Visible);

				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				AssertEquals("Precondition", true, wizard.FromTypeRequiresDate);

				AssertEquals(true, form.FromDateDateEdit_Exposed.Visible);
			}
		}

		#endregion

		#region CommissionBackdateItemsGrid

		public void TestSelectAllButton()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var unapprovedAgreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var unapprovedAgreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 2, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = false;
				wizard.CommissionAgreementApprovalItemCollection[1].IsInclude = false;

				form.SelectAllButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("CommissionAgreementApprovalItems[0].IsInclude", true, wizard.CommissionAgreementApprovalItemCollection[0].IsInclude);
					AssertEquals("CommissionAgreementApprovalItems[1].IsInclude", true, wizard.CommissionAgreementApprovalItemCollection[1].IsInclude);
				});
			}
		}

		public void TestDeselectAllButton()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var unapprovedAgreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var unapprovedAgreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 2, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;
				wizard.CommissionAgreementApprovalItemCollection[1].IsInclude = true;

				form.DeselectAllButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("CommissionAgreementApprovalItems[0].IsInclude", false, wizard.CommissionAgreementApprovalItemCollection[0].IsInclude);
					AssertEquals("CommissionAgreementApprovalItems[1].IsInclude", false, wizard.CommissionAgreementApprovalItemCollection[1].IsInclude);
				});
			}
		}

		#endregion

		#region Approve Button

		public void TestApproveButton()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsSalesRep = true;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(unapprovedAgreement, "ALL", "ALL", "ALL");
			unapprovedAgreement.FillWithValidTestData();
			var recipient = unapprovedAgreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			var recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;

			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				form.CheckBoxAddToQueue_Exposed.Checked = false;
				form.ApproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertType("ZFormModaliser.LastFormShownForTest", typeof(CommissionAgreementApproveProgressForm), ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestApproveButton_ValidationErrors()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			unapprovedAgreement.FillWithValidTestData();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = "XXX";

				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;
				form.ApproveButton_Exposed.PerformClick();

				AssertEquals("Precondition", true, form.ApproveButton_Exposed.Enabled);

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Approve", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
					Assert("Approve Button is Enabled", form.ApproveButton_Exposed.Enabled);
					Assert("Disapprove Button is Enabled", form.DisapproveButton_Exposed.Enabled);
				});
			}
		}

		public void TestApproveButton_CommissionAgreementApprovalException()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsSalesRep = true;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(unapprovedAgreement, "ALL", "ALL", "ALL");
			unapprovedAgreement.FillWithValidTestData();
			var recipient = unapprovedAgreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			var recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;

			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard_ThrowingCommissionAgreementApprovalException(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				form.CheckBoxAddToQueue_Exposed.Checked = false;
				form.ApproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("error message for user", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Approve Button is Disabled", !form.ApproveButton_Exposed.Enabled);
					Assert("Disapprove Button is Disabled", !form.DisapproveButton_Exposed.Enabled);
				});
			}
		}

		public void TestApproveButton_PreApproveValidationErrors()
		{
			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement.FillWithValidTestData();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = false;

				AssertEquals(false, form.ApproveButton_Exposed.Enabled);
			}
		}

		public void TestApproveButton_ConcurrencyException()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsSalesRep = true;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var agreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "ALL", "ALL", "ALL");
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			var recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(otherFactory))
				{
					var agreementInOtherFactory = otherFactory.Load<OrgCommissionAgreement>(agreement.PK);
					agreementInOtherFactory.ApproveDraft();
					otherFactory.Save();
				}

				var formClosed = false;
				form.FormClosed += (sender, e) =>
					{
						formClosed = true;
					};

				form.ApproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "WARNING", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("formClosed", true, formClosed);
				});
			}
		}

		public void TestApproveButton_IgnoreUnresolvableZSaveConcurrencyException()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsSalesRep = true;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var agreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "ALL", "ALL", "ALL");
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			var recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard_ThrowingZSaveConcurrencyException(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};

				var isUserInteractive = Globals.IsUserInteractive;

				try
				{
					ExceptionReporter.Instance.TestingDoReportException.Value = true;
					Globals.IsUserInteractive = false;
					form.ApproveButton_Exposed.PerformClick();
				}
				finally
				{
					Globals.IsUserInteractive = isUserInteractive;
				}

				AssertEquals("formClosed", true, formClosed);
				AssertEquals("While you have been working with this form, another user has made changes which cannot be merged. Please re-open the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestApprovePreviouslyApprovedAgreement_While2FormsAreOpen()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_IsSalesRep = true;

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement.CA0_Name = "#1";
			unapprovedAgreement.FillWithValidTestData();
			var recipient = unapprovedAgreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			var recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;

			var approvedAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			approvedAgreement.CA0_Name = "#2";
			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(approvedAgreement, "ALL", "ALL", "ALL");
			approvedAgreement.FillWithValidTestData();
			recipient = approvedAgreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;

			recipient1Rate = recipient.Rates.AddNew();
			recipient1Rate.CAT_CommissionPercentage = 10;

			var newDraftAgreement = approvedAgreement.CreateDraft();

			Factory.Save();
			var factory2 = new BusinessObjectFactory();

			var wizard1 = new CommissionAgreementApprovalWizard(Factory);
			var wizard2 = new CommissionAgreementApprovalWizard(factory2);
			AssertContainsExactElementsInAnyOrder("Precondition",
				new[] { unapprovedAgreement, newDraftAgreement },
				wizard1.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().Select(x => x.CommissionAgreement));

			AssertContainsExactElementsInAnyOrder("Precondition", BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
				new[] { unapprovedAgreement, newDraftAgreement },
				wizard2.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().Select(x => x.CommissionAgreement));

			using (var form1 = new CommissionAgreementApprovalWizardFormForTest(wizard1))
			using (var form2 = new CommissionAgreementApprovalWizardFormForTest(wizard2))
			{
				form1.Show();
				form2.Show();
				wizard1.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard1.FromDate = new ZDateTime(2002, 2, 2);
				wizard1.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().Single(x => x.CommissionAgreement == newDraftAgreement).IsInclude = true;

				form1.CheckBoxAddToQueue_Exposed.Checked = false;
				form1.ApproveButton_Exposed.PerformClick();

				AssertEquals("Precondition", true, newDraftAgreement.IsDeleted);

				AssertEquals(1, form1.CommissionAgreementApprovalFilterControlGrid_Exposed.List.Count);
				AssertNoExceptionThrown(() =>
				{
					form2.CommissionAgreementApprovalFilterControlGrid_Exposed.ListManager.Position = 0;
					form2.CommissionAgreementApprovalFilterControlGrid_Exposed.ListManager.Position = 1;
					form2.CommissionAgreementApprovalFilterControlGrid_Exposed.ListManager.Position = 0;
				});
			}
		}

		public void TestDirectProcessing_DontReinstate()
		{
			var wizard = new CommissionAgreementApprovalWizardForTest(Factory);
			wizard.Agreement = CreateAgreementForReapproval();
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);
			AssertEquals("Precondition", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
				wizard.FromDate = new ZDateTime(2016, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				ZFormModaliser.SetDelegateToCallOnFormClosing((x) =>
				{
					var excludeCancelledCommissionLinesForm = ((ExcludeCancelledCommissionLinesForm)x);
					excludeCancelledCommissionLinesForm.ContinueButton.PerformClick();
				});
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.ApproveButton_Exposed.PerformClick();

				AssertEquals("Reversal and Reinstate line should not have been created", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestDirectProcessing()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var wizard = new CommissionAgreementApprovalWizardForTest(Factory);
			wizard.Agreement = CreateAgreementForReapproval();

			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);
			AssertEquals("Precondition", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
				wizard.FromDate = new ZDateTime(2016, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				form.CheckBoxAddToQueue_Exposed.Checked = false;

				ZFormModaliser.SetDelegateToCallOnFormClosing((x) =>
				{
					var excludeCancelledCommissionLinesForm = ((ExcludeCancelledCommissionLinesForm)x);
					excludeCancelledCommissionLinesForm.UntickAllSubcontrols();
					excludeCancelledCommissionLinesForm.ContinueButton.PerformClick();
				});
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.ApproveButton_Exposed.PerformClick();

				AssertEquals("Reversal and Reinstate line should have been created", 3, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestAgreementPlacedInCalculationQueue()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var wizard = new CommissionAgreementApprovalWizardForTest(Factory);
			wizard.Agreement = CreateAgreementForReapproval();

			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);
			AssertEquals("Precondition", 1, Factory.Load<AccCommissionLine>(new ZQuery()).Length);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
				wizard.FromDate = new ZDateTime(2016, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				ZFormModaliser.SetDelegateToCallOnFormClosing((x) =>
				{
					var excludeCancelledCommissionLinesForm = ((ExcludeCancelledCommissionLinesForm)x);
					excludeCancelledCommissionLinesForm.ContinueButton.PerformClick();
				});
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));

				form.ApproveButton_Exposed.PerformClick();
				AssertEquals("Agreement should be placed in calculation queue.", 1, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));
			}
		}

		public void TestSearchPerformed_ApproveButtonEnableWorks()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 0, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();

				var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);
				var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
				OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(unapprovedAgreement, "ALL", "ALL", "ALL");
				unapprovedAgreement.FillWithValidTestData();
				Factory.Save();

				form.Find();

				AssertEquals("After Find", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

				AssertEquals("Approve Button should be disabled", false, form.ApproveButton_Exposed.Enabled);

				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				AssertEquals("Approve Button should be enabled", true, form.ApproveButton_Exposed.Enabled);
			}
		}

		#endregion

		#region Disapprove Button

		public void TestDisapproveButton()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				form.DisapproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				});
			}
		}

		public void TestDisapproveButton_PreDisapproveValidationErrors()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = false;

				AssertEquals(false, form.ApproveButton_Exposed.Enabled);
			}
		}

		public void TestDisapproveButton_PreDisapproveValidationErrorsOnChildren()
		{
			var approvedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			approvedAgreement.Approve();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			wizard.CommissionAgreementApprovalItemCollection.Add(new CommissionAgreementApprovalItem(wizard, approvedAgreement));
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				AssertEquals("Precondition", true, form.ApproveButton_Exposed.Enabled);

				form.DisapproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Disapprove", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertContains("LastMessage.Text", "Cannot disapprove an approved agreement. Reverse or disable the agreement instead.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ZFormModaliser.LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
				});
			}
		}

		public void TestDisapproveButton_ConcurrencyException()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				wizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
				wizard.FromDate = new ZDateTime(2002, 2, 2);
				wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = true;

				var otherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(otherFactory))
				{
					var agreementInOtherFactory = otherFactory.Load<OrgCommissionAgreement>(unapprovedAgreement.PK);
					agreementInOtherFactory.DisapproveDraft();
					otherFactory.Save();
				}

				var formClosed = false;
				form.FormClosed += (sender, e) =>
					{
						formClosed = true;
					};

				form.DisapproveButton_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "WARNING", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("formClosed", true, formClosed);
				});
			}
		}

		#endregion

		#region Append Button

		public void TestAppendButton()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();

				form.AppendButton_Exposed.PerformClick();
				AssertType(typeof(EmbeddedModulePopup), ZFormModaliser.LastFormShownDialogForTest);
				using (var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(AppendCommissionAgreementItemModuleDecisionProvider), modulePopup.EmbeddedModulePopupOKButtonStrategy);
				}
			}
		}

		#endregion

		#region ViewQueueButton

		public void TestViewQueueButton_ShouldShowCalculationQueueActionMenuItems()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			using (var form = new CommissionAgreementApprovalWizardFormForTest(wizard))
			{
				form.Show();
				form.ViewQueueButton_Exposed.PerformClick();
				AssertType(typeof(EmbeddedModulePopup), ZFormModaliser.LastFormShownDialogForTest);
				using (var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest)
				{
					var menu = modulePopup.Module_ForTest.FormActionMenu.Single(x => x.Text == "&Actions")
						.MenuItems.OfType<MenuItem>()
						.Single(x => x.Text.Contains("Remove from Calculation Queue"));
					AssertNotNull(menu);
				}
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			return new CommissionAgreementApprovalWizardForm(wizard);
		}

		#endregion

		#region Implementation

		OrgCommissionAgreement CreateAgreementForReapproval()
		{
			var postDate = new ZDateTime(2016, 2, 2);
			var customerA = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_IsSalesRep = true;

			var unapprovedAgreement1 = opportunity.CommissionAgreements.AddNew();
			var recipient = unapprovedAgreement1.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staff.GS_Code;
			recipient.CAR_IsCommissionRateOverriden = true;
			recipient.CAR_Share = 10;
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;

			var rate = recipient.Rates.AddNew();
			rate.FillWithValidTestData();

			OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(
				unapprovedAgreement1,
				OrgCommissionAgreementItemLookups.AllProductsCode,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			unapprovedAgreement1.FillWithValidTestData();
			unapprovedAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

			var tHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			tHeader.AH_PostDate = postDate;
			tHeader.AH_TransactionType = TransactionTypes.Invoice;

			var shipmentA = Factory.NewWithValidTestData<ForwardingShipment>();
			var jHeader = new JobHeader.Loader(shipmentA).TryCreate();
			jHeader.JH_OA_LocalChargesAddr = customerA.MainAddress.PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TESTCHRG";

			var inv = Factory.NewWithValidTestData<ARInvoice>();
			inv.AH_JH = jHeader.PK;
			inv.AH_PostDate = postDate;

			var tLine = (TransactionLine)inv.Lines.AddNew();
			tLine.FillWithValidTestData();
			tLine.AL_LineType = TransactionLineTypes.Revenue;
			tLine.AL_JH = jHeader.PK;
			tLine.AL_AC = chargeCode.PK;
			tLine.AL_LineAmount = 200;
			tLine.AL_OSAmount = 200;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AL_ARLine = tLine.PK;
			charge.JR_JH = tLine.AL_JH;
			charge.JR_LocalSellAmt = 200;

			var header = Factory.NewWithValidTestData<AccCommissionHeader>();
			header.CH0_AH_Source = inv.PK;
			header.CH0_GroupingSourceID = jHeader.PK;
			header.CH0_GroupingSourceTableCode = jHeader.TablePrefix;
			header.CH0_OH_Customer = customerA.PK;
			header.CH0_Product = "SHP";
			header.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			header.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			header.CH0_CA0 = unapprovedAgreement1.PK;

			var lineGroup = header.LineGroups.AddNew();
			lineGroup.FillWithValidTestData();
			lineGroup.CLG_AC = chargeCode.PK;

			var line = lineGroup.Lines.AddNew();
			line.FillWithValidTestData();
			line.CL0_OH_Party = recipient.CAR_OH_Party;
			line.CL0_RX_NKTransactionCurrency = "AUD";
			line.CL0_GS_NKStaff = "TST";
			line.Cancel();

			Factory.Save();

			return unapprovedAgreement1;
		}

		class CommissionAgreementApprovalWizard_ThrowingCommissionAgreementApprovalException : CommissionAgreementApprovalWizard
		{
			class CommissionAgreementApproverForTest : CommissionAgreementApprover
			{
				public CommissionAgreementApproverForTest(BusinessObjectFactory factory) : base(factory)
				{
				}

				protected override void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
				{
					throw new CommissionAgreementApprovalException("error message for log", "error message for user");
				}
			}

			public CommissionAgreementApprovalWizard_ThrowingCommissionAgreementApprovalException(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override CommissionAgreementApprover GetCommissionAgreementApprover()
			{
				return new CommissionAgreementApproverForTest(Factory);
			}
		}

		class CommissionAgreementApprovalWizard_ThrowingZSaveConcurrencyException : CommissionAgreementApprovalWizard
		{
			public CommissionAgreementApprovalWizard_ThrowingZSaveConcurrencyException(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override CommissionAgreementApprover GetCommissionAgreementApprover()
			{
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, Db.Connection), Factory);
			}
		}

		class CommissionAgreementApprovalWizardForTest : CommissionAgreementApprovalWizard
		{
			public CommissionAgreementApprovalWizardForTest(BusinessObjectFactory factory)
				: base(factory) { }

			public OrgCommissionAgreement Agreement;

			CommissionAgreementAndRates AgreementAndRates => new CommissionAgreementAndRates(Agreement, new ZDateTime(2016, 2, 2).Date);

			protected override CreateCommissionContext GetCreateCommissionContext(IEnumerable<CommissionAgreementApprovalItem> agreementApprovalItems)
			{
				var context = new CreateCommissionContext();
				context.FromDate = FromDate.IsEmpty ? ZDateTime.MinSmallDateTimeValue : FromDate;
				context.OverwriteOldValues = ShouldOverwriteOldCommission;
				context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(agreementApprovalItems.Select(x => x.CommissionAgreement));
				context.AgreementAndRatesOverride = new Dictionary<ZString, ICommissionAgreementAndRates>() { { ZString.Empty, AgreementAndRates } };
				return context;
			}
		}

		class CommissionAgreementApprovalWizardFormForTest : CommissionAgreementApprovalWizardForm
		{
			public CommissionAgreementApprovalWizardFormForTest(CommissionAgreementApprovalWizard wizard)
				: base(wizard)
			{
			}

			public ZGrid CommissionAgreementApprovalFilterControlGrid_Exposed => commissionAgreementApprovalFilterControl.Grid;

			public void Find() => commissionAgreementApprovalFilterControl.Find();

			public ZDateEdit FromDateDateEdit_Exposed
			{
				get { return FromDateDateEdit; }
			}

			public ZButton SelectAllButton_Exposed
			{
				get { return SelectAllButton; }
			}

			public ZButton DeselectAllButton_Exposed
			{
				get { return DeselectAllButton; }
			}

			public ZButton ApproveButton_Exposed
			{
				get { return ApproveButton; }
			}

			public ZButton DisapproveButton_Exposed
			{
				get { return DisapproveButton; }
			}

			public ZButton AppendButton_Exposed
			{
				get { return AppendButton; }
			}

			public ZCheckBox CheckBoxAddToQueue_Exposed
			{
				get { return checkBoxAddToQueue; }
			}

			public ZToolStripButton ViewQueueButton_Exposed => ViewQueueButton;
		}

		#endregion
	}
}
