using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	internal class ExportSeaTextRegistryItem : DocumentOpenCloseTextRegistryItem
	{
		public ExportSeaTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: this(name, subCategory, caption, hint, null, options)
		{
		}

		public ExportSeaTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, ResourceString defaultText, RegistryOptions options)
			: base(name, RegistryItemSet.CombineCategories(RawDataRegistry.Categories.Documents_Forwarding_Consol, subCategory, ResString.GetMultilingualString("b2f849fa-55e1-4d2a-8a79-eea2dbb939a5", "Export Sea")), caption, hint, RegistryStorageFlags.All, options, defaultText)
		{
			DepartmentsAllowed = DepartmentFlags.ExportSea;
		}
	}
}
