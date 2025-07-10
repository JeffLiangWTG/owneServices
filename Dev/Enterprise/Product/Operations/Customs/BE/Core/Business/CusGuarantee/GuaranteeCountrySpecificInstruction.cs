using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<EUGuaranteeTypeList>();

		public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
		{
			var result = Factory.GetCachedValue(
				key: "BE.CusGuaranteeHeaderLookups.PermitSubTypes" + typeCode,
				getValueDelegate: () =>
				{
					switch (typeCode)
					{
						case EUGuaranteeTypeList.Codes.IMP:
						case EUGuaranteeTypeList.Codes.COD:
							return new GuaranteeSubTypeList();
						case EUGuaranteeTypeList.Codes.TRA:
							return RefCusCodeListTypes.GetCachedList(
								Factory,
								Core.Constants.CountryCodes.Belgium,
								EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL251,
								ZDateTime.Today);
						default:
							return base.GetSubTypeList(typeCode);
					}
				});

			result.Sort();

			return result;
		}
	}
}
