using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseImportInfo : ImportCollectionInfoImpl
	{
		public LicenceDatabaseImportInfo(LicenceDatabaseFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.EnterpriseID) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.EnterpriseCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.ServerCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.Product) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.Edition) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.ReleaseType) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.InvoiceBranch) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.InvoiceCurrency) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.InvoiceGst) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.InvoiceSalesTax) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.TenantID) { CharacterCasing = ZCharacterCasing.Normal });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.SystemID) { CharacterCasing = ZCharacterCasing.Normal });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.AutoGenerateEntCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.SystemType) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.RegistrationStatus) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.PreRegistrationExpiryDateUTC) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.AllowWebAutoLogin) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<LicenceDatabaseFlattened>(LicenceDatabaseFlattened.Schema.HostedLocation) { CharacterCasing = ZCharacterCasing.Upper });
		}
	}
}
