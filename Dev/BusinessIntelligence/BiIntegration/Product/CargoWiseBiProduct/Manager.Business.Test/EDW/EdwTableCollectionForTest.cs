using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class EdwTableCollectionForTest : EdwTableCollection<EdwTestTable>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EdwTestTable("Schema", "Name");
		}
	}
}
