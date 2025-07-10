using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
		{
			var result = base.GetSubTypeList(typeCode);
			if (typeCode == EUGuaranteeTypeList.Codes.TRA)
			{
				result = Factory.GetCachedValue<GuaranteeSubTypeList>();
			}
			return result;
		}
	}
}
