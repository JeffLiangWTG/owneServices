using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureHeaderContainerLookups : CusInBondContainerLookups
	{
		public NctsDepartureHeaderContainerLookups(NctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("EU.NctsHeaderContainerLookups.CargoIdTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					return result;
				});
			}
		}
	}
}
