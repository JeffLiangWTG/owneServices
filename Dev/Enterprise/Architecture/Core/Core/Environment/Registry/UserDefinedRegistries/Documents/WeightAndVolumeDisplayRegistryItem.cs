using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	internal class WeightAndVolumeDisplayRegistryItem : RegistryItemImpl
	{
		public WeightAndVolumeDisplayRegistryItem(MultilingualString documentName)
			: this(documentName, RawDataRegistry.Categories.Documents_Forwarding)
		{
		}

		public WeightAndVolumeDisplayRegistryItem(MultilingualString documentName, MultilingualString formCategory, MultilingualString description = null)
			: base(documentName.GetUnresolvedString() + "WeightAndVolumeDisplay", RegistryItemSet.CombineCategories(formCategory, documentName), ResString.GetMultilingualString("9b3c9208-45db-409e-8188-1ab5babb78af", "Weight And Volume Display"), description ?? ResString.GetMultilingualString("aa6be36c-2d89-4345-98a6-779bafb127aa", "Choose the type of weight and volume to display in this document."), new WeightAndVolumeDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, WeightAndVolumeDisplayTypes.Codes.Actual)
		{
		}
	}
}
