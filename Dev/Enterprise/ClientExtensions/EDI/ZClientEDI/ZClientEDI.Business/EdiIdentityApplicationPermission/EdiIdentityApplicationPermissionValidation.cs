//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityApplicationPermissionValidation
//
//    This class should be used for overriding validation in AutoEdiIdentityApplicationPermissionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business
{
	using CargoWise.EntityFramework;
	using Res = ZClientEDI.Business.Res;

	public class EdiIdentityApplicationPermissionValidation : AutoEdiIdentityApplicationPermissionValidation
	{
		public EdiIdentityApplicationPermissionValidation(AutoEdiIdentityApplicationPermission parent) : base(parent)
		{
		}

		protected override void CheckIAP_Scope()
		{
			base.CheckIAP_Scope();

			MandatoryValidation.CheckEntered(Parent.IAP_ScopeInfo, "Permission");

			if (Parent.Application.IDA_IDA_ParentApplication.IsEmpty && Parent.IAP_Scope.Contains(CustomerApplicationPermissionsList.Codes.AuditAPI))
			{
				Parent.IAP_ScopeInfo.AddError(Res.GetString("83F28BC4-4A3F-476E-9CEA-43522D4ACEC9", "Non-CW1 customer application should not have {0} Permission.", CustomerApplicationPermissionsList.Codes.AuditAPI));
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.IAP_ScopeInfo, Parent.Application.Permissions);
		}

		new EdiIdentityApplicationPermission Parent => (EdiIdentityApplicationPermission)base.Parent;
	}
}
