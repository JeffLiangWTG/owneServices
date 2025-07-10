using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Forwarding.Testing
{
	[TestedType(typeof(DocForwardingConsolCollection))]
	sealed class DocForwardingConsolCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocForwardingConsolCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			return DocForwardingConsol.New(forwardingConsol, Factory);
		}

		protected override DocForwardingConsolCollection GetCollectionToTest()
		{
			return new DocForwardingConsolCollection(Factory);
		}
	}
}
