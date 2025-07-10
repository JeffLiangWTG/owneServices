using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsDepartureHeaderContainerLookups : NctsDepartureHeaderContainerLookups
	{
		public FRNctsDepartureHeaderContainerLookups(FRNctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Modes
		{
			get
			{
				return Factory.GetCachedValue("FRNctsHeaderContainerLookups.Modes", delegate
				{
					var list = FreightCodePairLists.JS_PackingModeList(ZString.Empty);
					list.AddPairIfNotExist(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					list.AddPairIfNotExist(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					list.RemoveCode(Core.Constants.ContainerModes.BuyersConsol);
					list.RemoveCode(Core.Constants.ContainerModes.ShippersConsol);
					list.RemoveCode(Core.Constants.ContainerModes.AgentConsol);
					return list;
				});
			}
		}
	}
}
