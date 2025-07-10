using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentDeliveryDefaultLanguagesRegistryItem : StronglyTypedRegistryItem<DocumentDeliveryDefaultLanguagesCollection>
	{
		public DocumentDeliveryDefaultLanguagesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DocumentDeliveryDefaultLanguagesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentDeliveryDefaultLanguagesRegistryDataType(), storage, defaultValue))
		{
		}

		public DocumentDeliveryDefaultLanguagesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ZInt defaultOrder)
			: this(name, category, caption, hint, storage, new DocumentDeliveryDefaultLanguagesCollection { new DocumentDeliveryDefaultLanguages(true) { Fallback = Enterprise.Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System, Order = defaultOrder } })
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DocumentDeliveryDefaultLanguagesRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	class DocumentDeliveryDefaultLanguagesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentDeliveryDefaultLanguagesCollection>
	{
		protected override void ValidateCore(IRegistryItem registryItem, DocumentDeliveryDefaultLanguagesCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue != null && proposedValue.Count > 0)
			{
				if (proposedValue.Cast<DocumentDeliveryDefaultLanguages>().Any(p => p.Order > proposedValue.Count))
				{
					throw new RegistryValidationException(Res.GetString("E3C9FAAA-60E1-4961-80FE-6C39822B5195", "The Order sequence should equal the number of Fallbacks used."));
				}
			}
		}
	}
}
