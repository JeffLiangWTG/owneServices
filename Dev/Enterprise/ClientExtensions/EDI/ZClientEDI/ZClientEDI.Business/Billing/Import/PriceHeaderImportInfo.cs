using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceHeaderImportInfo : ImportCollectionInfoImpl
	{
		public PriceHeaderImportInfo(PriceHeaderImportCollection collection)
			: base(collection)
		{
			Add(new ImportPropertyInfoImpl<PriceHeaderImport>(PriceHeaderImport.Schema.OrgCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderImport>(PriceHeaderImport.Schema.Currency) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderImport>(PriceHeaderImport.Schema.ValidFrom) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<PriceHeaderImport>(PriceHeaderImport.Schema.PricelistVersion) { CharacterCasing = ZCharacterCasing.Normal });
			Add(new ImportPropertyInfoImpl<PriceHeaderImport>(PriceHeaderImport.Schema.UseStdDiscount));
		}
	}
}
