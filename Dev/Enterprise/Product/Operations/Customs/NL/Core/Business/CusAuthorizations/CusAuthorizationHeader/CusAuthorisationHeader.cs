using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorisationHeader : Customs.Business.CusAuthorisationHeader
{
	public CusAuthorisationHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusAuthorisationHeaderValidation Validation => (CusAuthorisationHeaderValidation)GetNewValidation();

	public new CusPermitHeaderValidation GetNewValidation() => new CusAuthorisationHeaderValidation(this);

	public new CusAuthorisationHeaderProvider Provider => GetProviderCore();

	protected new CusAuthorisationHeaderProvider GetProviderCore()
	{
		if (provider == null || provider.CountryCode != CPH_RN_NKCountryCode)
		{
			provider = new CusAuthorisationHeaderProvider(CountryCodes.Netherlands);
		}
		return provider;
	}
	CusAuthorisationHeaderProvider provider;

	public override ZGuid CPH_OH_PermitHolder
	{
		get => base.CPH_OH_PermitHolder;
		set
		{
			base.CPH_OH_PermitHolder = value;
			if (PermitHolder != null && base.CPH_Type.EqualsIgnoringCase(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode))
			{
				base.CPH_Number = PermitHolder.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode);
				Validation.ValidateCPH_OH_PermitHolder();
			}
		}
	}

	CusAuthorisationRuleCollection cusAuthorisationRules;
	[ChildEditable]
	public new CusAuthorisationRuleCollection CusAuthorisationRules
	{
		get
		{
			if (cusAuthorisationRules == null)
			{
				cusAuthorisationRules = new CusAuthorisationRuleCollection(this);
				RegisterEditableChildObject(cusAuthorisationRules);
			}
			return cusAuthorisationRules;
		}
	}
}
