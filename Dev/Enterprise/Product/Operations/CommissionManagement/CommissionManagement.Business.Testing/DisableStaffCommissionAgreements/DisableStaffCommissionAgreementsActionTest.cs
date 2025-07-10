using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(DisableStaffCommissionAgreementsAction))]
	public class DisableStaffCommissionAgreementsActionTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		[TestDate(2002, 2, 2)]
		public void TestDateDefaultValue()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_DepartureDate = new ZDateTime(2001, 1, 1);
			var action = DisableStaffCommissionAgreementsAction.New(staff);
			AssertEquals("Should be defaulted from departure date", new ZDateTime(2001, 1, 1), action.Date);

			staff.GS_DepartureDate = ZDateTime.Empty;
			action = DisableStaffCommissionAgreementsAction.New(staff);
			AssertEquals("Should fallback to current date when no departure date", new ZDateTime(2002, 2, 2), action.Date);
		}

		#endregion

		#region Execute

		public void TestExecuteButDontApprove_WithoutUpdatingEarlierEndDates()
		{
			var action = DisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteButDontApprove();
			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement1adlRecipient.CAR_EndDate);
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement1risRecipient.CAR_EndDate);

				var agreement1Draft = Agreement1.Draft;

				var agreement1DraftAdlRecipient = agreement1Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");
				AssertEquals("Should be updated since no previous end date", new ZDateTime(2002, 2, 2), agreement1DraftAdlRecipient.CAR_EndDate);

				var agreement1DraftRisRecipient = agreement1Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "RIS");
				AssertEquals("Should be unchaged - not ADL", ZDateTime.Empty, agreement1DraftRisRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement1adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
				AssertStatusChangeLogReferences(Agreement1risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Original Agreements should be unchanged", new ZDateTime(2003, 1, 1), Agreement2adlRecipient_End2003.CAR_EndDate);

				var agreement2Draft = Agreement2.Draft;
				var agreement2DraftAdlRecipient = agreement2Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");
				AssertEquals("Should be updated since previous end date later than new end date", new ZDateTime(2002, 2, 2), agreement2DraftAdlRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement2adlRecipient_End2003.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date: 01-Jan-03 > 02-Feb-02");
			}

			{
				AssertEquals("Original Agreements should be unchanged", new ZDateTime(2001, 1, 1), Agreement3adlRecipient_End2001.CAR_EndDate);
				AssertEquals("Should not be updated since previous end date earlier than new end date", false, Agreement3.HasDraft);

				AssertStatusChangeLogReferences(Agreement3adlRecipient_End2001.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement4risRecipient.CAR_EndDate);
				AssertEquals("Agreement does not have ADL recipient", false, Agreement4.HasDraft);

				AssertStatusChangeLogReferences(Agreement4risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement5adlRecipient.CAR_EndDate);
				AssertEquals("Should not create new drafts since one already exists", Agreement5Draft.PK, Agreement5.Draft.PK);
				AssertEquals("Should be updated since no previous end date", new ZDateTime(2002, 2, 2), Agreement5DraftAdlRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement5adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}

			{
				AssertEquals("Should be updated since it is a new unapproved agreement", new ZDateTime(2002, 2, 2), NewUnapprovedAgreementAdlRecipient.CAR_EndDate);
				AssertEquals("Should not create a draft", false, NewUnapprovedAgreement.HasDraft);

				AssertStatusChangeLogReferences(NewUnapprovedAgreementAdlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}
		}

		public void TestExecuteButDontApprove_WithUpdatingEarlierEndDates()
		{
			var action = DisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = true;

			action.ExecuteButDontApprove();
			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement1adlRecipient.CAR_EndDate);
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement1risRecipient.CAR_EndDate);
				var agreement1Draft = Agreement1.Draft;

				var agreement1DraftAdlRecipient = agreement1Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");
				AssertEquals("Should be updated since no previous end date", new ZDateTime(2002, 2, 2), agreement1DraftAdlRecipient.CAR_EndDate);

				var agreement1DraftRisRecipient = agreement1Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "RIS");
				AssertEquals("Should be unchaged - not ADL", ZDateTime.Empty, agreement1DraftRisRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement1adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
				AssertStatusChangeLogReferences(Agreement1risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Original Agreements should be unchanged", new ZDateTime(2003, 1, 1), Agreement2adlRecipient_End2003.CAR_EndDate);
				var agreement2Draft = Agreement2.Draft;
				var agreement2DraftAdlRecipient = agreement2Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");
				AssertEquals("Should be updated since previous end date later than new end date", new ZDateTime(2002, 2, 2), agreement2DraftAdlRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement2adlRecipient_End2003.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date: 01-Jan-03 > 02-Feb-02");
			}

			{
				AssertEquals("Original Agreements should be unchanged", new ZDateTime(2001, 1, 1), Agreement3adlRecipient_End2001.CAR_EndDate);
				var agreement3Draft = Agreement3.Draft;
				var agreement3DraftAdlRecipient = agreement3Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");
				AssertEquals("Should be updated since 'ShouldUpdateEarlierEndDates' flag is true", new ZDateTime(2002, 2, 2), agreement3DraftAdlRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement3adlRecipient_End2001.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date: 01-Jan-01 > 02-Feb-02");
			}

			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement4risRecipient.CAR_EndDate);
				AssertEquals("Agreement does not have ADL recipient", false, Agreement4.HasDraft);

				AssertStatusChangeLogReferences(Agreement4risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Original Agreements should be unchanged", ZDateTime.Empty, Agreement5adlRecipient.CAR_EndDate);
				AssertEquals("Should not create new drafts since one already exists", Agreement5Draft.PK, Agreement5.Draft.PK);
				AssertEquals("Should be updated since no previous end date", new ZDateTime(2002, 2, 2), Agreement5DraftAdlRecipient.CAR_EndDate);

				AssertStatusChangeLogReferences(Agreement5adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}

			{
				AssertEquals("Should be updated since it is a new unapproved agreement", new ZDateTime(2002, 2, 2), NewUnapprovedAgreementAdlRecipient.CAR_EndDate);
				AssertEquals("Should not create a draft", false, NewUnapprovedAgreement.HasDraft);

				AssertStatusChangeLogReferences(NewUnapprovedAgreementAdlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}
		}

		public void TestExecuteAndApprove()
		{
			var action = DisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteAndApprove(null);
			{
				AssertEquals("Should be updated since no previous end date", new ZDateTime(2002, 2, 2), Agreement1adlRecipient.CAR_EndDate);
				AssertEquals("Should not be updated since not ADL recipient", ZDateTime.Empty, Agreement1risRecipient.CAR_EndDate);
				AssertEquals("Should not create any new drafts", false, Agreement1.HasDraft);

				AssertStatusChangeLogReferences(Agreement1.Logs, "Approved: From Date = ALL, Rework Old Commissions = Y");
				AssertStatusChangeLogReferences(Agreement1adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02");
				AssertStatusChangeLogReferences(Agreement1risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Should be updated since previous end date later than new end date", new ZDateTime(2002, 2, 2), Agreement2adlRecipient_End2003.CAR_EndDate);
				AssertEquals("Should not create any new drafts", false, Agreement2.HasDraft);

				AssertStatusChangeLogReferences(Agreement2.Logs, "Approved: From Date = ALL, Rework Old Commissions = Y");
				AssertStatusChangeLogReferences(Agreement2adlRecipient_End2003.Logs,
					"Staff Disable Date: 02-Feb-02");
			}

			{
				AssertEquals("Should not be updated since previous end date earlier than new end date", new ZDateTime(2001, 1, 1), Agreement3adlRecipient_End2001.CAR_EndDate);
				AssertEquals("Should not create any new drafts", false, Agreement3.HasDraft);

				AssertStatusChangeLogReferences("Should not add approved log", Agreement3.Logs, Array.Empty<ZString>());
				AssertStatusChangeLogReferences("Should not add disabled log", Agreement3adlRecipient_End2001.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Should not be updated since not ADL recipient", ZDateTime.Empty, Agreement4risRecipient.CAR_EndDate);
				AssertEquals("Should not create any new drafts", false, Agreement4.HasDraft);

				AssertStatusChangeLogReferences("Should not add approved log", Agreement4.Logs, Array.Empty<ZString>());
				AssertStatusChangeLogReferences("Should not add disabled log", Agreement4risRecipient.Logs, Array.Empty<ZString>());
			}

			{
				AssertEquals("Both the original and draft end dates should have been updated", new ZDateTime(2002, 2, 2), Agreement5adlRecipient.CAR_EndDate);
				AssertEquals("Both the original and draft end dates should have been updated", new ZDateTime(2002, 2, 2), Agreement5DraftAdlRecipient.CAR_EndDate);
				AssertEquals("Should not create new drafts", Agreement5Draft.PK, Agreement5.Draft.PK);
				AssertEquals("Should have not approved the draft", true, Agreement5Draft.IsDraft);
				AssertEquals("Should have not approved the draft", false, Agreement5Draft.IsApproved);

				AssertStatusChangeLogReferences(Agreement5.Logs, "Approved: From Date = ALL, Rework Old Commissions = Y");
				AssertStatusChangeLogReferences(Agreement5adlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}

			{
				AssertEquals("Should be updated since it is a new unapproved agreement", new ZDateTime(2002, 2, 2), NewUnapprovedAgreementAdlRecipient.CAR_EndDate);
				AssertEquals("Should not create any new drafts", false, NewUnapprovedAgreement.HasDraft);
				AssertEquals("Should have not approved", true, NewUnapprovedAgreement.IsDraft);
				AssertEquals("Should have not approved", false, NewUnapprovedAgreement.IsApproved);

				AssertStatusChangeLogReferences("Should not add approved log", Agreement4.Logs, Array.Empty<ZString>());
				AssertStatusChangeLogReferences(NewUnapprovedAgreementAdlRecipient.Logs,
					"Staff Disable Date: 02-Feb-02",
					"Entitlement End Date:  > 02-Feb-02");
			}
		}

		public virtual void TestExecuteAndApprove_CalcQueue_BackDateNotSet()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);

			var action = DisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteAndApprove(null);

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, Agreement1.PK));

			AssertNotNull("Queue Item for Agreement 1 exists", queueItem);

			AssertEquals("Always override", true, queueItem.CAQ_OverwriteExistingCommissions);
			AssertEquals("No Date Set", ZDateTime.MinSmallDateTimeValue, queueItem.CAQ_MinimumInvoicePostedDate);
		}

		public virtual void TestExecuteAndApprove_CalcQueue_BackDateSet()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2002, 1, 1));

			var action = DisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteAndApprove(null);

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, Agreement1.PK));

			AssertNotNull("Queue Item for Agreement 1 exists", queueItem);

			AssertEquals("Always override", true, queueItem.CAQ_OverwriteExistingCommissions);
			AssertEquals("No Date Set", ZDateTime.MinSmallDateTimeValue, queueItem.CAQ_MinimumInvoicePostedDate);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.New<GlbStaff>();
			return DisableStaffCommissionAgreementsAction.New(staff);
		}

		#endregion

		#region Implementation

		void AssertStatusChangeLogReferences(Logs logs, params ZString[] expectedLogReferences)
		{
			AssertStatusChangeLogReferences("", logs, expectedLogReferences);
		}

		void AssertStatusChangeLogReferences(string message, Logs logs, params ZString[] expectedLogReferences)
		{
			var actualLogReferences = logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
			AssertContainsExactElementsInAnyOrder(message, expectedLogReferences, actualLogReferences);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var commissionPeriodList = new CommissionPeriodCollection();
			commissionPeriodList.AddNew("0-12", (NoResString)"First Year", 0, 12);
			commissionPeriodList.AddNew("12-24", (NoResString)"Second Year", 12, 24);
			commissionPeriodList.AddNew("24-0", (NoResString)"Third Year onwards", 12, 24);
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriodList);

			AdlStaff = Factory.NewWithValidTestData<GlbStaff>();
			AdlStaff.GS_Code = "ADL";
			AdlStaff.GS_IsSalesRep = true;

			RisStaff = Factory.NewWithValidTestData<GlbStaff>();
			RisStaff.GS_Code = "RIS";
			RisStaff.GS_IsSalesRep = true;

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Agreement1 = opportunity.ApprovedCommissionAgreements.AddNew();
			Agreement1.CA0_OH_Customer = opportunity.P8_OH;
			Agreement1.FillWithValidTestData();
			Agreement1adlRecipient = Agreement1.Recipients.AddNew();
			Agreement1adlRecipient.CAR_GS_NKStaff = AdlStaff.GS_Code;
			Agreement1risRecipient = Agreement1.Recipients.AddNew();
			Agreement1risRecipient.CAR_GS_NKStaff = RisStaff.GS_Code;

			Agreement2 = opportunity.ApprovedCommissionAgreements.AddNew();
			Agreement2.CA0_OH_Customer = opportunity.P8_OH;
			Agreement2.FillWithValidTestData();
			Agreement2adlRecipient_End2003 = Agreement2.Recipients.AddNew();
			Agreement2adlRecipient_End2003.CAR_GS_NKStaff = AdlStaff.GS_Code;
			Agreement2adlRecipient_End2003.CAR_EndDate = new ZDate(2003, 1, 1);

			Agreement3 = opportunity.ApprovedCommissionAgreements.AddNew();
			Agreement3.CA0_OH_Customer = opportunity.P8_OH;
			Agreement3.FillWithValidTestData();
			Agreement3adlRecipient_End2001 = Agreement3.Recipients.AddNew();
			Agreement3adlRecipient_End2001.CAR_GS_NKStaff = AdlStaff.GS_Code;
			Agreement3adlRecipient_End2001.CAR_EndDate = new ZDate(2001, 1, 1);

			Agreement4 = opportunity.ApprovedCommissionAgreements.AddNew();
			Agreement4.CA0_OH_Customer = opportunity.P8_OH;
			Agreement4.FillWithValidTestData();
			Agreement4risRecipient = Agreement4.Recipients.AddNew();
			Agreement4risRecipient.CAR_GS_NKStaff = RisStaff.GS_Code;

			Agreement5 = opportunity.ApprovedCommissionAgreements.AddNew();
			Agreement5.CA0_OH_Customer = opportunity.P8_OH;
			Agreement5.FillWithValidTestData();
			Agreement5adlRecipient = Agreement5.Recipients.AddNew();
			Agreement5adlRecipient.CAR_GS_NKStaff = AdlStaff.GS_Code;

			Agreement5Draft = Agreement5.CreateDraft();
			Agreement5DraftAdlRecipient = Agreement5Draft.Recipients.Single(x => x.CAR_GS_NKStaff == "ADL");

			NewUnapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			NewUnapprovedAgreement.CA0_OH_Customer = opportunity.P8_OH;
			NewUnapprovedAgreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			NewUnapprovedAgreementAdlRecipient = NewUnapprovedAgreement.Recipients.AddNew();
			NewUnapprovedAgreementAdlRecipient.CAR_GS_NKStaff = AdlStaff.GS_Code;

			Factory.Save();
		}

		protected GlbStaff AdlStaff;
		GlbStaff RisStaff;
		protected OrgCommissionAgreement Agreement1;
		OrgCommissionAgreement Agreement2;
		OrgCommissionAgreement Agreement3;
		OrgCommissionAgreement Agreement4;
		OrgCommissionAgreement Agreement5;
		OrgCommissionAgreement Agreement5Draft;
		OrgCommissionAgreement NewUnapprovedAgreement;
		protected OrgCommissionAgreementRecipient Agreement1adlRecipient;
		OrgCommissionAgreementRecipient Agreement1risRecipient;
		OrgCommissionAgreementRecipient Agreement2adlRecipient_End2003;
		OrgCommissionAgreementRecipient Agreement3adlRecipient_End2001;
		OrgCommissionAgreementRecipient Agreement4risRecipient;
		OrgCommissionAgreementRecipient Agreement5adlRecipient;
		OrgCommissionAgreementRecipient Agreement5DraftAdlRecipient;
		OrgCommissionAgreementRecipient NewUnapprovedAgreementAdlRecipient;

		#endregion
	}

	[UseSnapshotProtection]
	class DisableStaffCommissionAgreementsActionNonTransactionedTest : TestCase
	{
		public void TestExecuteAndApprove_ShouldBeDoneInSameTransaction()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.LoadTop1<OrgHeader>(new ZQuery());
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var opportunity = factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;

			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.CA0_OH_Customer = org.PK;
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_EndDate = ZDate.Empty;
			recipient.CAR_GS_NKStaff = staff.GS_Code;

			factory.Save();

			var action = new DisableStaffCommissionAgreementsActionThatThrowsExceptionInApproveCreateCommission(staff);
			action.Date = new ZDateTime(2002, 2, 2);

			AssertExceptionThrown("Exception should have been forcefully thrown", typeof(InsufficientMemoryException), () =>
			{
				action.ExecuteAndApprove(null);
			});

			var newFactory = new BusinessObjectFactory();
			var recipientInNewFactory = newFactory.Load<OrgCommissionAgreementRecipient>(recipient.PK);
			AssertEquals("Should remain unchanged because exception should have rolled back everything", ZDate.Empty, recipientInNewFactory.CAR_EndDate);
		}

		class DisableStaffCommissionAgreementsActionThatThrowsExceptionInApproveCreateCommission : DisableStaffCommissionAgreementsAction
		{
			public DisableStaffCommissionAgreementsActionThatThrowsExceptionInApproveCreateCommission(GlbStaff staff)
				: base(staff)
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
		}
	}
}
