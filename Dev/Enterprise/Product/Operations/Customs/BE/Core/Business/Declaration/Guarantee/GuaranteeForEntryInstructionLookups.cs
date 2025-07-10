using Enterprise.Customs.BE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class GuaranteeForEntryInstructionLookups : EU.Business.Declaration.GuaranteeForEntryInstructionLookups
{
	public GuaranteeForEntryInstructionLookups(GuaranteeForEntryInstruction guarantee) : base(guarantee)
	{
	}

	protected override CodeDescriptionPairList BondTypeListCore
	{
		get
		{
			var res = new GuaranteeSubTypeList();
			res.Sort();
			return res;
		}
	} 

	public ZZRefCusCodeListCombinedCollection FacilityCollection => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, CargoWise.Types.ZDateTime.Today);
}
