using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	public class EntryChargeTypeSettingCollectionRegistryItem : StronglyTypedRegistryItem<EntryChargeTypeSettingCollection>
	{
		public EntryChargeTypeSettingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new EntryChargeTypeRegistryDataType(), storage))
		{
		}

		public EntryChargeTypeSettingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new EntryChargeTypeRegistryDataType(), storage, options))
		{
		}

		public List<ZGuid> GetAllChargeCodesIncludingDefault()
		{
			var result = new List<ZGuid>();

			result.Add(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);

			foreach (EntryChargeTypeSetting chargeType in Value)
			{
				if (!result.Contains(chargeType.AC_ChargeCode))
				{
					result.Add(chargeType.AC_ChargeCode);
				}
			}

			return result;
		}

		[RegistryEditor("Enterprise.Registry.GUI.Customs.EntryChargeTypeRegistryItemEditor, Enterprise.Registry.GUI")]
#if DEBUG
		public
#endif
		class EntryChargeTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EntryChargeTypeSettingCollection>
		{
		}
	}
}
