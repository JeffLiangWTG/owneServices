using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	internal class ExportAirTextRegistryItem : DocumentOpenCloseTextRegistryItem
	{
		public ExportAirTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: this(name, subCategory, caption, hint, null, options)
		{
		}

		public ExportAirTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, ResourceString defaultText, RegistryOptions options)
			: base(name, RegistryItemSet.CombineCategories(RawDataRegistry.Categories.Documents_Forwarding_Consol, subCategory, ResString.GetMultilingualString("b2d3fad9-1cdc-4f4f-8430-16e686a50bb1", "Export Air")), caption, hint, RegistryStorageFlags.All, options, defaultText)
		{
			DepartmentsAllowed = DepartmentFlags.ExportAir;
		}
	}
}
