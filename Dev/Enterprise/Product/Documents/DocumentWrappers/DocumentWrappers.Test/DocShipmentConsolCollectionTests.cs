using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocShipmentConsolCollection))]
	sealed class DocShipmentConsolCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocShipmentConsolCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			return DocShipmentConsol.New(forwardingConsol, Factory);
		}

		protected override DocShipmentConsolCollection GetCollectionToTest()
		{
			return new DocShipmentConsolCollection(Factory);
		}
	}
}
