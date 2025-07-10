using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DummyDataRegistryTest : RegistryItemSet
	{
		public override bool IsForProductivityWise => false;

		public CodeDescriptionPairListRegistryItem TestCodeDescriptionPairListRegistry
		{
			get
			{
				return GetItem("TestCodeDescriptionPairListRegistry", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("TS1", ResString.GetMultilingualString("TestKey1", "Placeholder 1"));
					list.AddPair("TS2", ResString.GetMultilingualString("TestKey2", "Placeholder 2"));
					list.AddPair("TS3", ResString.GetMultilingualString("TestKey3", "Placeholder 3"));
					var multilingualString = ResString.GetMultilingualString("TestKey", "Test description");
					return new CodeDescriptionPairListRegistryItem("StaffMembershipTypeList", multilingualString, multilingualString, multilingualString, 3, RegistryStorageFlags.System, list);
				});
			}
		}
	}
}
