using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Riba;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCollectionOrderLineCollection))]
	sealed class DocCollectionOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCollectionOrderLineCollection>
	{
		protected override DocCollectionOrderLineCollection GetCollectionToTest()
		{
			return new DocCollectionOrderLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCollectionOrderLine.New(OrderLine, Factory);
		}

		AccCollectionOrderLine OrderLine;
		protected override void SetUp()
		{
			OrderLine = Factory.NewWithValidTestData<AccCollectionOrderLine>();
			base.SetUp();
		}
	}
}
