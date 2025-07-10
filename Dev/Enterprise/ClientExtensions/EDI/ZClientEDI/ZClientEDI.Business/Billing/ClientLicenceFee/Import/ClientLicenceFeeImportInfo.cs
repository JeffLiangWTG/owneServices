using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeImportInfo : ImportCollectionInfoImpl
	{
		public ClientLicenceFeeImportInfo(ClientLicenceFeeFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.OrgCode, isMandatory: true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.ServerCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.SystemCode, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.FeeType, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.AmountAsText, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.Currency, isMandatory: true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.ChargeCode, isMandatory: true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.RenewalMonths, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.StartDate, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.EndDate));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.Description, isMandatory: true));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.Comment));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.IsDiscountable));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.Order));
			Add(new ImportPropertyInfoImpl<ClientLicenceFeeFlattened>(AutoClientLicenceFeeFlattened.Schema.TaxDate) { CharacterCasing = ZCharacterCasing.Upper });
		}
	}
}
