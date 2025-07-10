using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.LandedCosting.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocLandedCostHistoryCollection))]
	sealed class DocLandedCostHistoryCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocLandedCostHistoryCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lCHistory = Factory.New<LandedCostHistory>();
			return DocLandedCostHistory.New(lCHistory, Factory);
		}

		protected override DocLandedCostHistoryCollection GetCollectionToTest()
		{
			return new DocLandedCostHistoryCollection(Factory);
		}
	}
}
