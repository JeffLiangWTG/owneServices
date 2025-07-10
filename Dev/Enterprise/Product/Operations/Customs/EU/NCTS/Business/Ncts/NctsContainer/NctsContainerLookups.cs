using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public NctsContainerLookups(NctsContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TypeOfServiceList => Factory.GetCachedValue<ContainerTypeOfServiceList>();

		public CodeDescriptionPairList CargoIdTypeList => Factory.GetCachedValue("EU.NCTS.NctsContainerLookups.CargoIdTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
			result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
			return result;
		});
	}
}
