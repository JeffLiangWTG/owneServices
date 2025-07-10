using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ChargeCodeWithTypeRegistryItem : StronglyTypedRegistryItem<ChargeCodeWithTypeCollection>
	{
		public ChargeCodeWithTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ChargeCodeWithTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeCodesWithTypeRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ChargeCodesWithTypeRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class ChargeCodesWithTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeCodeWithTypeCollection>
	{
		protected override ChargeCodeWithTypeCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);

			// Ensure that the default party types exist after the deserialisation,
			// even if they have not been saved in a previous registry override.
			result.EnsureDefaultPartyTypesExist();

			return result;
		}
	}
}
