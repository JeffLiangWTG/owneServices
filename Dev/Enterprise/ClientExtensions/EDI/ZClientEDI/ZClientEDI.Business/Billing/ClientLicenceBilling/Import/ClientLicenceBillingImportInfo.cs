using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingImportInfo : ImportCollectionInfoImpl
	{
		public ClientLicenceBillingImportInfo(ClientLicenceBillingFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<ClientLicenceBillingFlattened>(ClientLicenceBillingFlattened.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceBillingFlattened>(ClientLicenceBillingFlattened.Schema.CurrentPrepaymentBalance));
			Add(new ImportPropertyInfoImpl<ClientLicenceBillingFlattened>(ClientLicenceBillingFlattened.Schema.CurrentPrepaymentCurrency) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<ClientLicenceBillingFlattened>(ClientLicenceBillingFlattened.Schema.FuturePrepaymentBalance));
			Add(new ImportPropertyInfoImpl<ClientLicenceBillingFlattened>(ClientLicenceBillingFlattened.Schema.FuturePrepaymentCurrency) { CharacterCasing = ZCharacterCasing.Upper });
		}
	}
}
