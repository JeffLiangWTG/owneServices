using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeletedPackingGroupCollection))]
	class DeletedPackingGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<CusEntryHeader>();
			return new DeletedPackingGroupCollection(header);
		}
	}
}
