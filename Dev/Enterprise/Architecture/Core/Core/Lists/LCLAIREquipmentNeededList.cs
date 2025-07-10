using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class LCLAIREquipmentNeededList : CodeDescriptionPairList
	{
		public static class Descriptions
		{
			public static MultilingualString Premise { get { return SourceGenerated.ResString.GetMultilingualString("Core|LCLAIREquipmentNeeded|Premise", "Premise Supplies Lift"); } }
			public static MultilingualString Haulier { get { return SourceGenerated.ResString.GetMultilingualString("Core|LCLAIREquipmentNeeded|Haulier", "Haulier Supplies Lift"); } }
			public static MultilingualString HandUnloadLoad { get { return SourceGenerated.ResString.GetMultilingualString("Core|LCLAIREquipmentNeeded|HandUnloadLoad", "Hand Unload/Load by Premise"); } }
			public static MultilingualString HandHaulier { get { return SourceGenerated.ResString.GetMultilingualString("Core|LCLAIREquipmentNeeded|HandHaulier", "Hand Unload/Load by Haulier"); } }
		}

		public LCLAIREquipmentNeededList(bool addAskClient) : base()
		{
			AddRange(EnvProxy.Instance.Registry.LCLAIREquipmentNeededList);

			AddPairIfNotExist(Constants.LCLAIREquipmentNeeded.Premise, Descriptions.Premise);
			AddPairIfNotExist(Constants.LCLAIREquipmentNeeded.Haulier, Descriptions.Haulier);
			AddPairIfNotExist(Constants.LCLAIREquipmentNeeded.HandHaulier, Descriptions.HandHaulier);
			AddPairIfNotExist(Constants.LCLAIREquipmentNeeded.HandUnloadLoad, Descriptions.HandUnloadLoad);

			AddPairIfNotExist(Constants.EquipmentNeeded.Any, SourceGenerated.ResString.GetMultilingualString("Core|EquipmentNeeded|ANY", "Any"));

			Sort();
			if (addAskClient)
			{
				AddPair(Constants.EquipmentNeeded.Ask, SourceGenerated.ResString.GetMultilingualString("Core|LCLAIREquipmentNeeded|ASK", "Ask Client"));
			}
		}

		public LCLAIREquipmentNeededList() : this(true)
		{
		}
	}
}
