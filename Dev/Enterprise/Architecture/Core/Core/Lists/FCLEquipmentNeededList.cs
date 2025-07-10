using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class FCLEquipmentNeededList : CodeDescriptionPairList
	{
		public static class Descriptions
		{
			public static MultilingualString WaitForUnpack { get { return SourceGenerated.ResString.GetMultilingualString("Core|FCLEquipmentNeeded|WaitForUnpack", "Wait for Pack/Unpack"); } }
			public static MultilingualString SideLoader { get { return SourceGenerated.ResString.GetMultilingualString("Core|FCLEquipmentNeeded|SideLoader", "Drop Container with Sideloader"); } }
			public static MultilingualString Trailer { get { return SourceGenerated.ResString.GetMultilingualString("Core|FCLEquipmentNeeded|Trailer", "Drop Trailer"); } }
			public static MultilingualString LiftOffOn { get { return SourceGenerated.ResString.GetMultilingualString("Core|FCLEquipmentNeeded|LiftOffOn", "Drop Container - Premise supplies Lift"); } }
		}

		public FCLEquipmentNeededList(bool addAskClient) : base()
		{
			AddRange(EnvProxy.Instance.Registry.FCLEquipmentNeededList);

			AddPairIfNotExist(Constants.FCLEquipmentNeeded.WaitForUnpack, Descriptions.WaitForUnpack);
			AddPairIfNotExist(Constants.FCLEquipmentNeeded.SideLoader, Descriptions.SideLoader);
			AddPairIfNotExist(Constants.FCLEquipmentNeeded.Trailer, Descriptions.Trailer);
			AddPairIfNotExist(Constants.FCLEquipmentNeeded.LiftOffOn, Descriptions.LiftOffOn);

			AddPairIfNotExist(Constants.EquipmentNeeded.Any, SourceGenerated.ResString.GetMultilingualString("Core|EquipmentNeeded|ANY", "Any"));

			Sort();
			if (addAskClient)
			{
				AddPair(Constants.EquipmentNeeded.Ask, SourceGenerated.ResString.GetMultilingualString("Core|FCLEquipmentNeeded|ASK", "Ask Client"));
			}
		}

		public FCLEquipmentNeededList() : this(true)
		{
		}
	}
}
