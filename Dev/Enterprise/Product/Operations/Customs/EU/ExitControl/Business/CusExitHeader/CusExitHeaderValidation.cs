using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderValidation : ExitControlBase.Business.CusExitHeaderValidation
	{
		public CusExitHeaderValidation(AutoCusExitHeader parent)
			: base(parent)
		{
		}

		protected new CusExitHeader Parent => (CusExitHeader)base.Parent;

		protected override void CheckCXH_OA_Carrier()
		{
			if (Parent.CXH_OA_Carrier.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CXH_OA_CarrierInfo);
			}
			else if (Parent.Carrier is OrgAddress carrierAddress && carrierAddress.Header is OrgHeader carrierOrg)
			{
				var carrierEORI = carrierOrg.GetEuIdentificationNumber(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ignoreCountryOfIssuanceIfNotMatched: true);
				ValidateCarrierEORI(carrierEORI);
			}
		}

		protected virtual void ValidateCarrierEORI(ZString carrierEORI)
		{
			if (carrierEORI.IsEmpty)
			{
				Parent.CXH_OA_CarrierInfo.AddMessageError(TheSelectedOrganizationMustHaveAValidEORIMessage);
			}
		}

		public static string TheSelectedOrganizationMustHaveAValidEORIMessage => Res.GetString("443B42FD-7173-4F55-917F-5E447AEB0953", "The selected Organization must have a valid EORI code.");

		protected override void CheckCXH_GB_Branch()
		{
			base.CheckCXH_GB_Branch();
			if (Parent.Branch is GlbBranch branch && branch.Company != Parent.Company)
			{
				Parent.CXH_GB_BranchInfo.AddError(Res.GetString("C236F3F4-F95A-4F0A-92B7-B2B2C6900EC2", "The selected branch does not belong to the same company as the job."));
			}
		}
	}
}

