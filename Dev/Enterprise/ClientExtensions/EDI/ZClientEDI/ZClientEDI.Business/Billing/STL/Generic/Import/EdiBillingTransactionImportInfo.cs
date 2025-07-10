using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBillingTransactionImportInfo : ImportCollectionInfoImpl
	{
		public EdiBillingTransactionImportInfo(EdiBillingTransactionFlattenedCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Category, true) { CharacterCasing = ZCharacterCasing.Upper, });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.PriceItemCode, true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.BillableCount, true));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.ReportingSource, true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.ServiceOccuredUTC, true));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.ClientNumber, true) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Reference1, true));

			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Branch) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.ClientStaffCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.MessageTrackingID));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Reference2));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Reference3));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Reference4));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Reference5));
			Add(new ImportPropertyInfoImpl<EdiBillingTransactionFlattened>(AutoEdiBillingTransactionFlattened.Schema.Version));
		}
	}
}
