using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionAgreementLogFilterBusinessObject))]
	public class CommissionAgreementLogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filter

		public void TestOnlyIncludeStatusChangeLogs()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var statusLog = agreement.Logs.AddNew(Events.StatusChange, "My Status Change Log");
			var miscLog = agreement.Logs.AddNew(Events.MiscellaneousEvent, "My Miscellaneous Log");

			Factory.Save();

			var filterBizObj = new CommissionAgreementLogFilterBusinessObject(agreement);
			AssertContainsExactElementsInAnyOrder(
				new[] { statusLog },
				Factory.Load<StmALog>(filterBizObj.Filter));
		}

		#endregion

		#region Module Filters

		[TestDate(2002, 2, 2)]
		public void TestSinceLastApprovedOrDisapprovedFilter_AgreementNotPreviouslyApproved()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_SystemCreateTimeUtc = new ZDateTime(2002, 2, 2);
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2003, 3, 3);
			var agreementLog1 = agreement.Logs.AddNew(Events.StatusChange);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2004, 4, 4);
			var agreementLog2 = agreement.Logs.AddNew(Events.StatusChange);
			Factory.Save();

			var filterBizObj = new CommissionAgreementLogFilterBusinessObject(agreement);
			var sinceLastApprovedOrDisapprovedFilter = (ModuleFlagsFilter)filterBizObj[CommissionAgreementLogFilterBusinessObject.FilterDescription.SinceLastApprovedOrDisapproved];
			AssertEquals(FilterVisibility.AlwaysVisible, sinceLastApprovedOrDisapprovedFilter.Visibility);
			AssertEquals("should be True by default", true, sinceLastApprovedOrDisapprovedFilter.Property0);

			sinceLastApprovedOrDisapprovedFilter.IsActive = true;
			sinceLastApprovedOrDisapprovedFilter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(
				new[] { agreementLog1, agreementLog2 },
				Factory.Load<StmALog>(filterBizObj.Filter));

			sinceLastApprovedOrDisapprovedFilter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(
				new[] { agreementLog1, agreementLog2 },
				Factory.Load<StmALog>(filterBizObj.Filter));
		}

		[TestDate(2002, 2, 2)]
		public void TestSinceLastApprovedOrDisapprovedFilter_AgreementPreviouslyApproved()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_SystemCreateTimeUtc = new ZDateTime(2002, 2, 2);
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2003, 3, 3);
			var agreementLog1 = agreement.Logs.AddNew(Events.StatusChange);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2004, 4, 4);
			var draft = agreement.CreateDraft();
			draft.CA0_SystemCreateTimeUtc = new ZDateTime(2004, 4, 4);
			var agreementLog2 = agreement.Logs.AddNew(Events.StatusChange);
			Factory.Save();

			var filterBizObj = new CommissionAgreementLogFilterBusinessObject(draft);
			var sinceLastApprovedOrDisapprovedFilter = (ModuleFlagsFilter)filterBizObj[CommissionAgreementLogFilterBusinessObject.FilterDescription.SinceLastApprovedOrDisapproved];
			AssertEquals(FilterVisibility.AlwaysVisible, sinceLastApprovedOrDisapprovedFilter.Visibility);
			AssertEquals("should be True by default", true, sinceLastApprovedOrDisapprovedFilter.Property0);

			sinceLastApprovedOrDisapprovedFilter.IsActive = true;
			sinceLastApprovedOrDisapprovedFilter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(
				new[] { agreementLog2 },
				Factory.Load<StmALog>(filterBizObj.Filter));

			sinceLastApprovedOrDisapprovedFilter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(
				new[] { agreementLog1, agreementLog2 },
				Factory.Load<StmALog>(filterBizObj.Filter));
		}

		#endregion

		#region Overrides

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var commissionAgreement = Factory.New<OrgCommissionAgreement>();
			commissionAgreement.CA0_SystemCreateTimeUtc = ZDateTime.Now;
			return new CommissionAgreementLogFilterBusinessObject(commissionAgreement);
		}

		#endregion
	}
}
