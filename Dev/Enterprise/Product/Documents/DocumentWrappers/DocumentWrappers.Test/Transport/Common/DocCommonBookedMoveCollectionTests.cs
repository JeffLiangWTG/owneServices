using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonBookedMoveCollection))]
	sealed class DocCommonBookedMoveCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCommonBookedMoveCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			return DocCommonBookedMove.New(move, Factory);
		}

		protected override DocCommonBookedMoveCollection GetCollectionToTest()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			return new DocCommonBookedMoveCollection(cartage);
		}
	}
}
