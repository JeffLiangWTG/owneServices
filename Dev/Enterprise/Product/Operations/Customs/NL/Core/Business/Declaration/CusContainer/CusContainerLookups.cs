using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusContainerLookups : EU.Business.Declaration.CusContainerLookups
{
	public CusContainerLookups(CusContainer parent)
		: base(parent)
	{
	}
	public CodeDescriptionPairList SealPartyList
	{
		get
		{
			return Factory.GetCachedValue("NL.CusContainer.SealParty_List", () =>
			{
				var result = new CodeDescriptionPairList();
				var list = Parent.JobContainer.Lookups.SealParty_List;

				result.Add(list[Core.Constants.ContainerSealParties.Codes.CarrierShippingLine]);
				result.Add(list[Core.Constants.ContainerSealParties.Codes.ConsignorShipper]);
				result.Add(list[Core.Constants.ContainerSealParties.Codes.Customs]);
				result.Add(list[Core.Constants.ContainerSealParties.Codes.Terminal]);

				return result;
			});
		}
	}
}
