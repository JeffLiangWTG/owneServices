using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsAdditionalInfoPhase5Lookups : EU.NCTS.Business.NctsAdditionalInfoPhase5Lookups
{
	public NctsAdditionalInfoPhase5Lookups(NctsAdditionalInfo parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			var parent = Parent;
			var inTransition = parent.IsInPhase5TransitionPeriod;

			return Factory.GetCachedValue("BE.NctsAdditionalInfoPhase5Lookups.SubTypeList" + inTransition, () =>
			{
				var list = new AdditionalInfoSubTypeList();
				if (parent.ParentAsGoodsItem != null)
				{
					if (parent.IsPhase5Arrival)
					{
						list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
					}
					else
					{
						if (!inTransition)
						{
							list.RemoveCode(AdditionalInfoSubTypeList.Codes.TransportDocument);
						}
					}
				}
				return list;
			});
		}
	}

	protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;
}
