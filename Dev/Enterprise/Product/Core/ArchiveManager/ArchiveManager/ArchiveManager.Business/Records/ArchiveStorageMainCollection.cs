using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class ArchiveStorageMainCollection : ActiveBusinessObjectCollection<ArchiveStorageMain>
	{
		public ArchiveStorageMainCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery();
			_ = filter.AddToFilter(StorageMainSchema.SM_Archived, SQLComparisonOperator.NotEqual, null);
			return filter;
		}
	}
}
