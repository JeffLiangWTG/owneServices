using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocViewCommissionLineGroupingCollection))]
	sealed class DocViewCommissionLineGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocViewCommissionLineGroupingCollection>
	{
		protected override DocViewCommissionLineGroupingCollection GetCollectionToTest()
		{
			var collection = new ViewCommissionLineGroupingCollection(Factory);
			return new DocViewCommissionLineGroupingCollection(collection, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var viewCommissionLineGrouping = new ViewCommissionLineGrouping(Factory);
			return DocViewCommissionLineGrouping.New(viewCommissionLineGrouping, Factory);
		}
	}
}
