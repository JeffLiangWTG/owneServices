using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class CustomsIncoTermOverrideCollectionRegistryItem : StronglyTypedRegistryItem<CustomsIncoTermOverrideCollection>
	{
		public CustomsIncoTermOverrideCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomsIncoTermOverrideRegistryDataType(), RegistryStorageFlags.Company, options))
		{
		}

		public ZString GetInternationalCode(ZString customsCode)
		{
			ZString result = ZString.Empty;
			if (!customsCode.IsEmpty)
			{
				foreach (CustomsIncoTermOverride incoTerm in Value)
				{
					if (incoTerm.CustomsCode == customsCode)
					{
						result = incoTerm.InternationalCode;
						break;
					}
				}
			}
			return result;
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.CustomsIncoTermOverrideRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	class CustomsIncoTermOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomsIncoTermOverrideCollection>
	{
	}
}
