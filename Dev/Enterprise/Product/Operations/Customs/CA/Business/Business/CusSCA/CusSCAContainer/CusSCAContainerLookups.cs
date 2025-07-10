
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAContainerLookups : Customs.Business.CusSCAContainerLookups
	{
		public CusSCAContainerLookups(CusSCAContainer container)
			: base(container)
		{
		}

		public CodeDescriptionPairList SCRContainerModes
		{
			get
			{
				return Factory.GetCachedValue("SCRContainerModes",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.ContainerModes.AIR, Core.Constants.ContainerModeDescriptions.AIR);
						result.AddPair(Core.Constants.ContainerModes.Empty, Core.Constants.ContainerModeDescriptions.Empty);
						result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
						result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList SCRContainerisedModes
		{
			get
			{
				return Factory.GetCachedValue("SCRContainerisedModes",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.ContainerModes.Empty, Core.Constants.ContainerModeDescriptions.Empty);
						result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList SCRNonContainerModes
		{
			get
			{
				return Factory.GetCachedValue("SCRNonContainerModes",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.ContainerModes.AIR, Core.Constants.ContainerModeDescriptions.AIR);
						result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
						return result;
					}
				);
			}
		}
	}
}
