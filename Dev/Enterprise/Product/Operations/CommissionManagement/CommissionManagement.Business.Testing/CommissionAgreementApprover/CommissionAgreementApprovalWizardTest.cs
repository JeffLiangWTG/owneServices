using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionAgreementApprovalWizard))]
	internal class CommissionAgreementApprovalWizardTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2016, 07, 01));

			var agreementApprovalWizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals(true, agreementApprovalWizard.ShouldOverwriteOldCommission);
			AssertEquals(true, agreementApprovalWizard.ShouldAddToCalculationQueue);
			AssertEquals(new DateTime(2016, 07, 01), agreementApprovalWizard.FromDate);
		}

		#endregion

		#region Properties

		public void TestSettingFromTypeResetsFromDateToDefaultRegistryValue()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2016, 07, 01));

			var agreementApprovalWizard = new CommissionAgreementApprovalWizard(Factory);
			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
			agreementApprovalWizard.FromDate = new ZDateTime(2010, 1, 1);

			AssertEquals("Pre-Condition", new ZDateTime(2010, 1, 1), agreementApprovalWizard.FromDate);

			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
			AssertEquals("New FromDate value should be the value in the registry.", OrganisationRegistry.Instance.DefaultSpecifiedBackdate.Value, agreementApprovalWizard.FromDate);
		}

		public void TestFromDate_ReadOnly()
		{
			var agreementApprovalWizard = new CommissionAgreementApprovalWizard(Factory);

			agreementApprovalWizard.FromType = "";
			AssertEquals(true, agreementApprovalWizard.FromDateInfo.ReadOnly);

			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
			AssertEquals(false, agreementApprovalWizard.FromDateInfo.ReadOnly);

			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
			AssertEquals(true, agreementApprovalWizard.FromDateInfo.ReadOnly);
		}

		#endregion

		#region Suspend Refresh Collection

		public void TestSuspendRefreshCollectionNesting()
		{
			var approverWizard = new CommissionAgreementApprovalWizardForTest(Factory);
			var collection = (CommissionAgreementApprovalItemCollectionForTest)approverWizard.CommissionAgreementApprovalItemCollection;
			AssertEquals("Initial Refresh Allowed", false, collection.IsSuspendRefresh_Exposed);

			using (collection.TemporarilySuspendRefreshCollection())
			{
				AssertEquals("First Nesting Level Suspend", true, collection.IsSuspendRefresh_Exposed);

				using (collection.TemporarilySuspendRefreshCollection())
				{
					AssertEquals("Second Nesting Level Suspend", true, collection.IsSuspendRefresh_Exposed);

					using (collection.TemporarilySuspendRefreshCollection())
					{
						AssertEquals("Third Nesting Level Suspend", true, collection.IsSuspendRefresh_Exposed);
					}
				}

				AssertEquals("First Nesting Level Still Suspended", true, collection.IsSuspendRefresh_Exposed);
			}

			AssertEquals("Final Refresh Allowed", false, collection.IsSuspendRefresh_Exposed);
		}

		#endregion

		#region Approve

		public void TestApprove()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementDraft1 = opportunity.CommissionAgreements.AddNew();
			agreementDraft1.FillWithValidTestData();
			var agreementDraft2 = opportunity.CommissionAgreements.AddNew();
			agreementDraft2.FillWithValidTestData();

			Factory.Save();

			var approverWizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 2, approverWizard.CommissionAgreementApprovalItems.Count());

			var approvalItem1 = approverWizard.CommissionAgreementApprovalItems.First(x => x.CommissionAgreement.PK == agreementDraft1.PK);
			approvalItem1.IsInclude = true;
			var approvalItem2 = approverWizard.CommissionAgreementApprovalItems.First(x => x.CommissionAgreement.PK == agreementDraft2.PK);
			approvalItem2.IsInclude = false;

			approverWizard.Approve(null);

			AssertEquals("Should have been approved", false, agreementDraft1.IsDraft);
			AssertEquals("Should not have been approved", true, agreementDraft2.IsDraft);
			AssertContainsExactElementsInAnyOrder("Should have removed approved items", new[] { agreementDraft2 }, approverWizard.CommissionAgreementApprovalItems.Select(x => x.CommissionAgreement));
		}

		public void TestApprove_SuspendRefreshCollection()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementDraft1 = opportunity.CommissionAgreements.AddNew();
			agreementDraft1.FillWithValidTestData();
			var agreementDraft2 = opportunity.CommissionAgreements.AddNew();
			agreementDraft2.FillWithValidTestData();

			Factory.Save();

			var approverWizard = new CommissionAgreementApprovalWizardForTest(Factory);
			AssertEquals("Precondition", 2, approverWizard.CommissionAgreementApprovalItems.Count());

			var approvalItem1 = approverWizard.CommissionAgreementApprovalItems.First(x => x.CommissionAgreement.PK == agreementDraft1.PK);
			approvalItem1.IsInclude = true;
			var approvalItem2 = approverWizard.CommissionAgreementApprovalItems.First(x => x.CommissionAgreement.PK == agreementDraft2.PK);
			approvalItem2.IsInclude = true;
			var testCollection = approverWizard.CommissionAgreementApprovalItemCollection as CommissionAgreementApprovalItemCollectionForTest;

			testCollection.ResetCounter();
			approverWizard.Approve(null);

			AssertEquals("Refresh Collection should have been done once", 1, testCollection.TimesRefreshCollectionDone);
		}

		#endregion

		#region Disapprove

		public void TestDisapprove()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementDraft1 = opportunity.CommissionAgreements.AddNew();
			agreementDraft1.FillWithValidTestData();
			var agreementDraft2 = opportunity.CommissionAgreements.AddNew();
			agreementDraft2.FillWithValidTestData();

			Factory.Save();

			var approvalWizard = new CommissionAgreementApprovalWizardForOpportunity(opportunity, Factory);
			AssertEquals("Precondition", 2, approvalWizard.CommissionAgreementApprovalItemCollection.Count);

			var approvalItem1 = approvalWizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().First(x => x.CommissionAgreement.PK == agreementDraft1.PK);
			approvalItem1.IsInclude = true;
			var approvalItem2 = approvalWizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().First(x => x.CommissionAgreement.PK == agreementDraft2.PK);
			approvalItem2.IsInclude = false;

			approvalWizard.Disapprove();

			AssertEquals("Should have been disapproved", true, agreementDraft1.IsDeleted);
			AssertEquals("Should not have been disapproved", false, agreementDraft2.IsDeleted);
			AssertContainsExactElementsInAnyOrder("Should have removed disapproved items", new[] { agreementDraft2 }, approvalWizard.CommissionAgreementApprovalItems.Select(x => x.CommissionAgreement));
		}

		public void TestDisapprove_WhereDraftFromOrganisationMerge()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreementApproved1 = opportunity.CommissionAgreements.AddNew();
			agreementApproved1.FillWithValidTestData();
			agreementApproved1.ApproveDraft();
			var agreementApproved2 = opportunity.CommissionAgreements.AddNew();
			agreementApproved2.FillWithValidTestData();
			agreementApproved2.ApproveDraft();
			var agreementApproved3 = opportunity.CommissionAgreements.AddNew();
			agreementApproved3.FillWithValidTestData();
			agreementApproved3.ApproveDraft();

			var agreementDraft1 = agreementApproved1.CreateDraft();
			agreementDraft1.Reverse();
			var agreementDraft2 = agreementApproved2.CreateDraft();
			agreementDraft2.Expire(ZDate.Today);
			var agreementDraft3 = agreementApproved3.CreateDraft();

			Factory.Save();

			var approvalWizard = new CommissionAgreementApprovalWizardForOpportunity(opportunity, Factory);
			AssertEquals("Precondition", 3, approvalWizard.CommissionAgreementApprovalItemCollection.Count);

			var approvalItem1 = approvalWizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().First(x => x.CommissionAgreement.PK == agreementDraft1.PK);
			approvalItem1.IsInclude = true;
			var approvalItem2 = approvalWizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().First(x => x.CommissionAgreement.PK == agreementDraft2.PK);
			approvalItem2.IsInclude = true;
			var approvalItem3 = approvalWizard.CommissionAgreementApprovalItemCollection.Cast<CommissionAgreementApprovalItem>().First(x => x.CommissionAgreement.PK == agreementDraft3.PK);
			approvalItem3.IsInclude = true;

			approvalWizard.Disapprove();

			var reloadedOpportunity = Factory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.PK, opportunity.PK));
			var reversedCommissionAgreements = reloadedOpportunity.CommissionAgreements.Where(c => c.IsReversed).ToArray();

			AssertEquals("Only the Draft created from Organisation Merge should reverse the parent", 1, reversedCommissionAgreements.Length);
			var reversedCommissionAgreement = reversedCommissionAgreements.First();

			AssertEquals("Parent of Reverse Draft is the third approved agreement", agreementApproved3.PK, reversedCommissionAgreement.CA0_CA0_ParentVersion);
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Commission Agreement Approval", wizard.HumanReadableName);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommissionAgreementApprovalWizard(Factory);
		}

		#endregion
	}

	internal class CommissionAgreementApprovalItemCollectionForTest : CommissionAgreementApprovalItemCollection
	{
		public CommissionAgreementApprovalItemCollectionForTest(CommissionAgreementApprovalWizard wizard) : base(wizard)
		{
		}

		int timesRefreshCollectionDone;

		protected override void RefreshCollection()
		{
			if (!IsRefreshSuspended)
			{
				timesRefreshCollectionDone++;
			}

			base.RefreshCollection();
		}

		public int TimesRefreshCollectionDone => timesRefreshCollectionDone;
		public void ResetCounter() => timesRefreshCollectionDone = 0;

		public bool IsSuspendRefresh_Exposed => IsRefreshSuspended;
	}

	internal class CommissionAgreementApprovalWizardForTest : CommissionAgreementApprovalWizard
	{
		public CommissionAgreementApprovalWizardForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override CommissionAgreementApprovalItemCollection GetCommissionAgreementApprovalItemCollection(CommissionAgreementApprovalWizard wizard)
		{
			return new CommissionAgreementApprovalItemCollectionForTest(wizard);
		}
	}

	internal class CommissionAgreementApprovalWizardForOpportunity : CommissionAgreementApprovalWizard
	{
		public CommissionAgreementApprovalWizardForOpportunity(OrgOpportunity opportunityForFilter, BusinessObjectFactory factory)
			: base(factory)
		{
			this.opportunity = opportunityForFilter;
		}

		public override OrgCommissionAgreementCollection UnapprovedCommissionAgreements
		{
			get
			{
				if (unapprovedCommissionAgreementsForOpportunity == null)
				{
					var requiresApprovalQuery = new ZQuery(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, null);
					requiresApprovalQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_P8, opportunity.PK);
					unapprovedCommissionAgreementsForOpportunity = new OrgCommissionAgreementCollection(Factory, requiresApprovalQuery);
				}

				return unapprovedCommissionAgreementsForOpportunity;
			}
		}
		OrgCommissionAgreementCollection unapprovedCommissionAgreementsForOpportunity;
		readonly OrgOpportunity opportunity;
	}

	[UseSnapshotProtection]
	class CommissionAgreementApprovalWizardNonTransactionedTest : TestCase
	{
		public void TestApprove_ApproveAgreementsAndCreateCommissionsShouldBeDoneInSameTransaction()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.LoadTop1<OrgHeader>(new ZQuery());
			var opportunity = factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.FillWithValidTestData();

			var agreementDraft1 = opportunity.CommissionAgreements.AddNew();
			agreementDraft1.CA0_OH_Customer = org.PK;
			agreementDraft1.FillWithValidTestData();

			factory.Save();

			var approverWizard = new CommissionAgreementApprovalWizardThatThrowsExceptionInApproveCreateCommission(factory);
			AssertEquals("Precondition", 1, approverWizard.CommissionAgreementApprovalItems.Count());

			var approvalItem1 = approverWizard.CommissionAgreementApprovalItems.First(x => x.CommissionAgreement.PK == agreementDraft1.PK);
			approvalItem1.IsInclude = true;
			approverWizard.ShouldAddToCalculationQueue = false;

			AssertExceptionThrown("Exception should have been forcefully thrown", typeof(DivideByZeroException), () =>
			{
				approverWizard.Approve(null);
			});

			var newFactory = new BusinessObjectFactory();
			var agreementDraftInNewFactory = newFactory.Load<OrgCommissionAgreement>(agreementDraft1.PK);
			AssertEquals("Should remain draft because exception should have rolled back everything", true, agreementDraftInNewFactory.IsDraft);
		}

		class CommissionAgreementApprovalWizardThatThrowsExceptionInApproveCreateCommission : CommissionAgreementApprovalWizard
		{
			public CommissionAgreementApprovalWizardThatThrowsExceptionInApproveCreateCommission(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override CommissionAgreementApprover GetCommissionAgreementApprover()
			{
				return new CommissionAgreementApproverThatThrowsExceptionInCreateCommission(Factory);
			}
		}

		class CommissionAgreementApproverThatThrowsExceptionInCreateCommission : CommissionAgreementApprover
		{
			public CommissionAgreementApproverThatThrowsExceptionInCreateCommission(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void QueueForCommissionsCalculation(CreateCommissionContext context)
			{
				throw new InsufficientMemoryException();
			}

			protected override void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
			{
				throw new DivideByZeroException();
			}
		}
	}
}
