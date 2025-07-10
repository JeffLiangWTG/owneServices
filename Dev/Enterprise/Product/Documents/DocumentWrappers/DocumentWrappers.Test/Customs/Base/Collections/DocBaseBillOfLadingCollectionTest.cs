using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseBillOfLadingCollection))]
	sealed class DocBaseBillOfLadingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocBaseBillOfLadingCollection>
	{
		protected override DocBaseBillOfLadingCollection GetCollectionToTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			return new DocBaseBillOfLadingCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.CreatePrimaryBill(BillTypeList.Codes.MasterBill);
			return new DocBaseBillOfLading(bill);
		}
	}
}
