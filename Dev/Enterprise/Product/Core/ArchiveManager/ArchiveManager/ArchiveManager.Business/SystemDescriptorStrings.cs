using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business
{
	public static class SystemDescriptorStrings
	{
		public static MultilingualString GetNoun(SystemDescriptorType descriptorType)
		{
			switch (descriptorType)
			{
				case SystemDescriptorType.Archive:
					return ResString.GetMultilingualString("0A1795CA-A34F-4EC6-B9ED-50B6501A8ABA", "Archive");
				case SystemDescriptorType.Purge:
					return ResString.GetMultilingualString("E2387A32-AB38-45DF-AE57-146BC03DA405", "Purge");
				case SystemDescriptorType.Offline:
					return ResString.GetMultilingualString("E674FDFF-3341-4B9A-B095-3697A824A48D", "Offline Archive");
				default:
					throw new ArgumentOutOfRangeException(nameof(descriptorType));
			}
		}

		public static MultilingualString GetPresentTenseVerb(SystemDescriptorType descriptorType)
		{
			switch (descriptorType)
			{
				case SystemDescriptorType.Archive:
					return ResString.GetMultilingualString("B2B76542-B703-442A-BDD7-5638DBF52999", "Archiving");
				case SystemDescriptorType.Purge:
					return ResString.GetMultilingualString("B243A2B0-A738-4525-A158-CE3D4A0CC707", "Purging");
				case SystemDescriptorType.Offline:
					return ResString.GetMultilingualString("3CC9E1FF-DBB1-4A9B-821D-4170FA713AE4", "Offline Archiving");
				default:
					throw new ArgumentOutOfRangeException(nameof(descriptorType));
			}
		}

		public static MultilingualString GetPastTenseVerb(SystemDescriptorType descriptorType)
		{
			switch (descriptorType)
			{
				case SystemDescriptorType.Archive:
					return ResString.GetMultilingualString("83198784-1BDE-45CA-93FA-4BBA430684DE", "Archived");
				case SystemDescriptorType.Purge:
					return ResString.GetMultilingualString("B3535027-9B24-4DD0-98DE-B9D6CB2A350E", "Purged");
				case SystemDescriptorType.Offline:
					return ResString.GetMultilingualString("50109441-2C62-47B3-8219-8538E8987BF0", "Archived Offline");
				default:
					throw new ArgumentOutOfRangeException(nameof(descriptorType));
			}
		}
	}
}
