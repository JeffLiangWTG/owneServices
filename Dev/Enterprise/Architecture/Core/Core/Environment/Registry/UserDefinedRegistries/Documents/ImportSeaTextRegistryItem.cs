using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class ImportSeaTextRegistryItem : DocumentOpenCloseTextRegistryItem
	{
		public ImportSeaTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: this(name, subCategory, caption, hint, null, options)
		{
		}

		public ImportSeaTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, ResourceString defaultText, RegistryOptions options)
			: base(name, RegistryItemSet.CombineCategories(RawDataRegistry.Categories.Documents_Forwarding_Shipment, subCategory, ResString.GetMultilingualString("4abb0474-ae1e-40af-806d-ba3c2853a806", "Import Sea")), caption, hint, RegistryStorageFlags.All, options, defaultText)
		{
			DepartmentsAllowed = DepartmentFlags.ImportSea;
		}
	}
}
