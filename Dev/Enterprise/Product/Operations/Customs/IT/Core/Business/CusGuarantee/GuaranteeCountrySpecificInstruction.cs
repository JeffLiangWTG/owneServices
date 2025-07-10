using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
{
	public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue("IT.GetTypeCodeDescriptionPairList", () =>
	{
		var codeList = new EUGuaranteeTypeList();
		codeList.RemoveCode(EUGuaranteeTypeList.Codes.TST);
		return codeList;
	});

	public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
	{
		switch (typeCode)
		{
			case EUGuaranteeTypeList.Codes.IMP:
				return Factory.GetCachedValue<ImportGuaranteeSubTypeList>();
			default:
				return base.GetSubTypeList(typeCode);
		}
	}
}
