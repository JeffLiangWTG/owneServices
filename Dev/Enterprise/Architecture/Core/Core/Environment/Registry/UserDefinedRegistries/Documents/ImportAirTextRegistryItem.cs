using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class ImportAirTextRegistryItem : DocumentOpenCloseTextRegistryItem
	{
		public ImportAirTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: this(name, subCategory, caption, hint, null, options)
		{
		}

		public ImportAirTextRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, ResourceString defaultText, RegistryOptions options)
			: base(name, RegistryItemSet.CombineCategories(RawDataRegistry.Categories.Documents_Forwarding_Shipment, subCategory, ResString.GetMultilingualString("ee7538db-8692-45f0-af45-692d9db7dd29", "Import Air")), caption, hint, RegistryStorageFlags.All, options, defaultText)
		{
			DepartmentsAllowed = DepartmentFlags.ImportAir;
		}
	}
}
