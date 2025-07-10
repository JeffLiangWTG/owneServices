using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BillImportActionCollection))]
	class BillImportActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillImportActionCollection>
	{
		protected override BillImportActionCollection GetCollectionToTest() => new BillImportActionCollection(header.Bills);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BillImportAction(header.Bills.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<JPAFRHeader>();
		}
		JPAFRHeader header;
	}
}
