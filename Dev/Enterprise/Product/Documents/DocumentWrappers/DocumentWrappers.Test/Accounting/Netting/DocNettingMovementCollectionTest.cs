using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingMovementCollection))]
	sealed class DocNettingMovementCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocNettingMovementCollection>
	{
		protected override DocNettingMovementCollection GetCollectionToTest()
		{
			return DocNettingMovementCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocNettingMovement.New(new NettingMovement(Factory), Factory);
		}
	}
}
