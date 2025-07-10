using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalHeaderContainerLookups : CusInBondContainerLookups
	{
		public NctsArrivalHeaderContainerLookups(NctsArrivalHeaderContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("EU.NctsArrivalHeaderContainerLookups.CargoIdTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					return result;
				});
			}
		}

		public CodeDescriptionPairList UnloadedStates
		{
			get
			{
				return Factory.GetCachedValue("EU.NctsArrivalHeaderContainerLookups.UnloadedStates", () =>
				{
					var result = new NctsUnloadedStateList();
					result.RemoveCode(NctsUnloadedStateList.Codes.NEW);
					result.RemoveCode(NctsUnloadedStateList.Codes.DAM);
					return result;
				});
			}
		}
	}
}
