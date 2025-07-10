using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorisationHeaderValidation : Customs.Business.CusAuthorisationHeaderValidation
{
	public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent)
		: base(parent)
	{
	}

	public new CusAuthorisationHeader Parent => (CusAuthorisationHeader)base.Parent;

	protected override void CheckCPH_OH_PermitHolder()
	{
		if (Parent.PermitHolder != null && Parent.CPH_Type.EqualsIgnoringCase(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode))
		{
			if (Parent.PermitHolder.CountryCode.Equals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				Parent.CPH_OH_PermitHolderInfo.AddError(Res.GetString("579AA90E-C574-4ACB-9D95-0E18F02E9B72", "Authorization holder cannot be located in the same country where your company is situated."));
			}

			if (Parent.PermitHolder.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode).IsEmpty)
			{
				Parent.CPH_OH_PermitHolderInfo.AddError(Res.GetString("4989EDE2-1E29-4A26-B996-FA5E6FB9527A", "Authorization holder must have an LFR number."));
			}
		}
	}
}
