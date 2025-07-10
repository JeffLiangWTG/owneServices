using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
{
	public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<GuaranteeTypeList>();

	public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
	{
		if (typeCode == GuaranteeTypeList.Codes.TRA)
		{
			return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSBondType, ZDateTime.Today);
		}
		else
		{
			return base.GetSubTypeList(typeCode);
		}
	}
}
