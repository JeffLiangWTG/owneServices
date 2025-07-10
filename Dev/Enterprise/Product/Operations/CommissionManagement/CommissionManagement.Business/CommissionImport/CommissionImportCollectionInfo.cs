using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionImportCollectionInfo : ImportCollectionInfoImpl
	{
		public CommissionImportCollectionInfo(CommissionFlattenedCollection collection, ZBool currentCompanyOnly)
			: base(collection)
		{
			CurrentCompanyOnly = currentCompanyOnly;

			if (!CurrentCompanyOnly)
			{
				Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.CompanyCode) { CharacterCasing = ZCharacterCasing.Upper });
			}

			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.GroupingSourceCode));
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.InvoiceType) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.StaffCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.PartyCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.CH0_CommissionDate));
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.CL0_CommissionType) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.CommissionCurrencyCode) { CharacterCasing = ZCharacterCasing.Upper });
			Add(new ImportPropertyInfoImpl<CommissionFlattened>(CommissionFlattened.Schema.EntityCommissionAmount));
		}

		public ZBool CurrentCompanyOnly { get; private set; }
		public ZString ImportType { get; set; }
	}
}
