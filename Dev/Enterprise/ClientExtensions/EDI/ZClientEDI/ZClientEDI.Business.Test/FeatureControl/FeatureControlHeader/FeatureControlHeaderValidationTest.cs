using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	internal class FeatureControlHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFCM_WKI_ActiveWorkItem()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var featureControl = Factory.New<FeatureControlHeader>();
			featureControl.FCM_WKI_DeactivateWorkItem = workItem.PK;
			featureControl.FCM_WKI_ActiveWorkItem = workItem.PK;
			AssertHasError(featureControl.FCM_WKI_ActiveWorkItemInfo, "The work item numbers in the two fields must not be the same work item");
			featureControl.FCM_WKI_ActiveWorkItem = ZGuid.Empty;
			AssertNoErrors(featureControl.FCM_WKI_ActiveWorkItemInfo);
		}

		public void TestCheckFCM_WKI_DeactivateWorkItem()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var featureControl = Factory.New<FeatureControlHeader>();
			featureControl.FCM_WKI_ActiveWorkItem = workItem.PK;
			featureControl.FCM_WKI_DeactivateWorkItem = workItem.PK;
			AssertHasError(featureControl.FCM_WKI_DeactivateWorkItemInfo, "The work item numbers in the two fields must not be the same work item");
			featureControl.FCM_WKI_DeactivateWorkItem = ZGuid.Empty;
			AssertNoErrors(featureControl.FCM_WKI_DeactivateWorkItemInfo);
		}

		public void TestCheckFCM_Description()
		{
			var featureControl = Factory.New<FeatureControlHeader>();
			featureControl.FCM_Description = "aaa";
			AssertNoErrors(featureControl.FCM_DescriptionInfo);
			featureControl.FCM_Description = "";
			AssertHasErrors(featureControl.FCM_DescriptionInfo);
			featureControl.FCM_Description = "bbb";
			AssertNoErrors(featureControl.FCM_DescriptionInfo);
		}

		public void TestCheckFCM_GG_ReleaseGroup()
		{
			EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed = true;
			var featureControl = Factory.New<FeatureControlHeader>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			featureControl.FCM_GG_ReleaseGroup = group.PK;
			AssertNoErrors(featureControl.FCM_GG_ReleaseGroupInfo);
			featureControl.FCM_GG_ReleaseGroup = Guid.Empty;
			AssertHasErrors(featureControl.FCM_GG_ReleaseGroupInfo);
			featureControl.FCM_GG_ReleaseGroup = group.PK;
			AssertNoErrors(featureControl.FCM_GG_ReleaseGroupInfo);

			EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed = false;
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			group.Staff.Add(currentStaff);
			featureControl.FCM_GG_ReleaseGroup = group.PK;
			AssertNoErrors(featureControl.FCM_GG_ReleaseGroupInfo);

			group.Staff.RemoveAll();
			featureControl.FCM_GG_ReleaseGroup = group.PK;
			AssertHasError(
				featureControl.FCM_GG_ReleaseGroupInfo,
				"You must be a member of this release group or have the Allow feature edit for any release group security right"
			);
		}
	}
}
