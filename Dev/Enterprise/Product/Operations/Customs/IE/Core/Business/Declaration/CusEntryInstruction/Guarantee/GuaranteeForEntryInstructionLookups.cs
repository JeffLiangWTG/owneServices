using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class GuaranteeForEntryInstructionLookups : EU.Business.Declaration.GuaranteeForEntryInstructionLookups
	{
		public GuaranteeForEntryInstructionLookups(GuaranteeForEntryInstruction guarantee) : base(guarantee)
		{
		}

		public CodeDescriptionPairList IEBondTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, CargoWise.Types.ZDateTime.Today);

		public CodeDescriptionPairList EuropeanUnionCountryList => Factory.GetEuropeanUnionCountryList();
	}
}
