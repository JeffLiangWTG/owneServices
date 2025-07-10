using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	class UpdateRegistryKeyItemsFromSection321ToLowValueEntries : RegistryDataTransformation
	{
		public override string UserDescription => "Update Customs Registry Key Items From Section 321 to Low Value Entries";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("EnableFDAforSection321Entries", "EnableFDAForLowValueEntries");
			UpdateRegistryItemName("Section321ReleaseMessages", "LowValueEntriesReleaseMessages");
			UpdateRegistryItemName("RemoveNonWesternEuropeanCharactersUSSection321", "RemoveNonWesternEuropeanCharactersUSLowValueEntries");
		}
	}
}
