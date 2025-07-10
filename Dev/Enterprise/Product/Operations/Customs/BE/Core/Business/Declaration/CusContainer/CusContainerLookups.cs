using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusContainerLookups : EU.Business.Declaration.CusContainerLookups
{
	public CusContainerLookups(CusContainer parent)
		: base(parent)
	{
	}
	public CodeDescriptionPairList SealParty_List
	{
		get
		{
			return Factory.GetCachedValue("BE.CusContainer.SealParty_List", () =>
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
