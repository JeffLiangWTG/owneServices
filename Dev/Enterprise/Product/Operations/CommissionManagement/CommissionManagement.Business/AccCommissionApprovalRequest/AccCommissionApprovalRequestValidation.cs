//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionApprovalRequestValidation
//
//    This class should be used for overriding validation in AutoAccCommissionApprovalRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestValidation : AutoAccCommissionApprovalRequestValidation
	{
		public AccCommissionApprovalRequestValidation(AutoAccCommissionApprovalRequest parent) : base(parent)
		{
		}

		new AccCommissionApprovalRequest Parent
		{
			get { return (AccCommissionApprovalRequest)base.Parent; }
		}

		protected override void CheckCRQ_GS_NKApprovingStaff1()
		{
			base.CheckCRQ_GS_NKApprovingStaff1();
			ListValidation.ErrorIfInvalidCode(Parent.CRQ_GS_NKApprovingStaff1Info);

			if (!Parent.IsInDatabase)
			{
				if (OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.Value >= 1)
				{
					MandatoryValidation.CheckEntered(Parent.CRQ_GS_NKApprovingStaff1Info);
				}

				if (!Parent.CRQ_GS_NKApprovingStaff1.IsEmpty)
				{
					CompareValidation.CheckNotEqual(Parent.CRQ_GS_NKApprovingStaff1Info, Parent.CRQ_GS_NKApprovingStaff2Info);
				}

				var approvingStaff = Parent.ApprovingStaff1;
				if (approvingStaff != null)
				{
					AddErrorIfEmailAddressIsEmptyOrNotValid(approvingStaff, Parent.CRQ_GS_NKApprovingStaff1Info);
					if (Parent.ApprovingStaff1Security != null && !Parent.ApprovingStaff1Security.CommissionAuthorizationLevel1.IsAllowed)
					{
						Parent.CRQ_GS_NKApprovingStaff1Info.AddError(GetStaffDoesNotHaveSecurityErrorMessage(Parent.ApprovingStaff1Security.CommissionAuthorizationLevel1));
					}
				}
			}
		}

		protected override void CheckCRQ_GS_NKApprovingStaff2()
		{
			base.CheckCRQ_GS_NKApprovingStaff2();
			ListValidation.ErrorIfInvalidCode(Parent.CRQ_GS_NKApprovingStaff2Info);

			if (!Parent.IsInDatabase)
			{
				if (OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.Value >= 2)
				{
					MandatoryValidation.CheckEntered(Parent.CRQ_GS_NKApprovingStaff2Info);
				}

				if (!Parent.CRQ_GS_NKApprovingStaff2.IsEmpty)
				{
					CompareValidation.CheckNotEqual(Parent.CRQ_GS_NKApprovingStaff2Info, Parent.CRQ_GS_NKApprovingStaff1Info);
				}

				var approvingStaff = Parent.ApprovingStaff2;
				if (approvingStaff != null)
				{
					AddErrorIfEmailAddressIsEmptyOrNotValid(approvingStaff, Parent.CRQ_GS_NKApprovingStaff2Info);
					if (Parent.ApprovingStaff2Security != null && !Parent.ApprovingStaff2Security.CommissionAuthorizationLevel2.IsAllowed)
					{
						Parent.CRQ_GS_NKApprovingStaff2Info.AddError(GetStaffDoesNotHaveSecurityErrorMessage(Parent.ApprovingStaff2Security.CommissionAuthorizationLevel2));
					}
				}
			}
		}

		static string GetStaffDoesNotHaveSecurityErrorMessage(SecurityCheckpoint securityCheckpoint)
		{
			return Res.GetString("e5104ef7-5741-48b9-890c-899cf773ee03", @"Staff does not have the appropriate security rights to approve this.

If this staff requires access to this function, ask your system administrator to change their Staff or Group Security Rights to allow access to:

{0}",
					securityCheckpoint.DisplayTextPathToSecurityRight);
		}

		#region Implementation

		void AddErrorIfEmailAddressIsEmptyOrNotValid(GlbStaff staff, ZPropertyInfo info)
		{
			if (staff.GS_EmailAddress.IsEmpty)
			{
				info.AddError(Res.GetString("eb70b580-0c10-4110-be0c-ec6f5af14291", "Staff does not have an email address."));
			}
			else if (!EmailAddressValidation.IsEmailAddressValid(staff.GS_EmailAddress))
			{
				info.AddError(Res.GetString("35dc8014-3444-428e-a89c-0dd232bbab1c", "Staff email address is not valid."));
			}
		}

		#endregion
	}
}
