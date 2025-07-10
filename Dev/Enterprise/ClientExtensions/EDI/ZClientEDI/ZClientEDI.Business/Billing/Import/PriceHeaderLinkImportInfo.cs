using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceHeaderLinkImportInfo : ImportCollectionInfoImpl
	{
		public PriceHeaderLinkImportInfo(PriceHeaderLinkImportCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.ServerCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.Currency) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.ValidFrom) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.PricelistVersion) { CharacterCasing = ZCharacterCasing.Normal });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.Volume) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.VolumePercent));
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.CorePack) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.CoreUpliftPercent));
			Add(new ImportPropertyInfoImpl<PriceHeaderLinkImport>(PriceHeaderLinkImport.Schema.ValidTo));
		}
	}
}
