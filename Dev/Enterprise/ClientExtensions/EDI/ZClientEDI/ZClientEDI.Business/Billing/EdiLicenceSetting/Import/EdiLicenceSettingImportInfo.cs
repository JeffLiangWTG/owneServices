using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingImportInfo : ImportCollectionInfoImpl
	{
		public EdiLicenceSettingImportInfo(EdiLicenceSettingFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.EnterpriseID) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.EnterpriseCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.ServerCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.ValidFrom));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.ValidTo));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.Comment));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.DiscountName) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.DiscountPercentAsText));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.DiscountActiveAsText));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.PriceAsText));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.PriceCategory) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.PriceCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.ApplyDiscounts));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.LicenceUnits));
			Add(new ImportPropertyInfoImpl<EdiLicenceSettingFlattened>(AutoEdiLicenceSettingFlattened.Schema.BWPurchasedLicencesAsText));
		}
	}
}
