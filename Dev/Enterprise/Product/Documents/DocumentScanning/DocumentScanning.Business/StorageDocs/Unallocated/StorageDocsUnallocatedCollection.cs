
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsUnallocatedCollection : StorageDocsCollectionBase<StorageDocsUnallocated>//, IDeliverableCollection
	{
		public StorageDocsUnallocatedCollection(NumberedBusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool IsAddNewSupported
		{
			get { return true; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var mainQuery = new ZDBOnlyQuery(typeof(StorageDocsUnallocated));

			if (ExcludeDeletedDocuments)
			{
				mainQuery.AddToFilter(StorageDocsSchema.SC_IsDeleted, false);
			}

			var sqlFilterQuery = new CountrySpecificRefTypesQuery().LiteralTextADO.Replace(StorageMainSchema.SM_Type.Name, "SC_DataType");

			mainQuery.AddFilterAndZSQLParameterCollection(sqlFilterQuery, new ZSqlParameterCollection());

			return mainQuery;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((StorageDocsUnallocated)child).SC_DataType = Core.Constants.DocManagerCodes.Unallocated;
		}
	}
}
