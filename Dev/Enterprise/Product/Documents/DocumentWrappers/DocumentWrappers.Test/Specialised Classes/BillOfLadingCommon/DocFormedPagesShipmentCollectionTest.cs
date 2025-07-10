using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocFormedPagesShipmentCollection))]
	sealed class DocFormedPagesShipmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocFormedPagesShipmentCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocFormedPagesShipment();
		}

		protected override DocFormedPagesShipmentCollection GetCollectionToTest()
		{
			return new DocFormedPagesShipmentCollection(Factory);
		}

		#endregion
	}
}
