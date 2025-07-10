using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.LandedCosting.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocLandedCostInputCollection))]
	sealed class DocLandedCostInputCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocLandedCostInputCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lCInput = Factory.New<LandCostInput>();
			return DocLandedCostInput.New(lCInput, Factory);
		}

		protected override DocLandedCostInputCollection GetCollectionToTest()
		{
			return new DocLandedCostInputCollection(Factory);
		}
	}
}
