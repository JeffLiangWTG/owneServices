//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlHeaderValidation
//
//    This class should be used for overriding validation in AutoFeatureControlHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlHeaderValidation : AutoFeatureControlHeaderValidation
	{
		public FeatureControlHeaderValidation(AutoFeatureControlHeader parent) : base(parent)
		{
		}

		protected override void CheckFCM_WKI_ActiveWorkItem()
		{
			base.CheckFCM_WKI_ActiveWorkItem();
			CheckWorkItem(Parent.FCM_WKI_ActiveWorkItemInfo);
		}

		protected override void CheckFCM_WKI_DeactivateWorkItem()
		{
			base.CheckFCM_WKI_DeactivateWorkItem();
			CheckWorkItem(Parent.FCM_WKI_DeactivateWorkItemInfo);
		}

		void CheckWorkItem(ZPropertyInfo propertyInfo)
		{
			if (!Parent.FCM_WKI_ActiveWorkItem.IsEmpty && !Parent.FCM_WKI_DeactivateWorkItem.IsEmpty
				&& Parent.FCM_WKI_ActiveWorkItem == Parent.FCM_WKI_DeactivateWorkItem)
			{
				propertyInfo.AddError("The work item numbers in the two fields must not be the same work item");
			}
		}

		protected override void CheckFCM_Description()
		{
			base.CheckFCM_Description();
			MandatoryValidation.CheckEntered(Parent.FCM_DescriptionInfo);
		}

		protected override void CheckFCM_GG_ReleaseGroup()
		{
			base.CheckFCM_GG_ReleaseGroup();
			if (Parent.ReleaseGroup != null)
			{
				var hasGlobalEditRight = EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed;
				var isInReleaseGroup = Parent.ReleaseGroup.Staff.Contains(Env.CurrentUser.PK);
				if (!hasGlobalEditRight && !isInReleaseGroup && !Parent.FCM_GG_ReleaseGroupInfo.ReadOnly)
				{
					Parent.FCM_GG_ReleaseGroupInfo.AddError("You must be a member of this release group or have the Allow feature edit for any release group security right");
				}
			}
		}
	}
}
