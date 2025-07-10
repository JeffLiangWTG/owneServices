using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocWorkFlowCollection))]
	sealed class DocWorkFlowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWorkFlowCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			return DocWorkFlow.New(forwardingShipment.WorkflowItems.AddNew(), Factory);
		}

		protected override DocWorkFlowCollection GetCollectionToTest()
		{
			return new DocWorkFlowCollection(Factory);
		}
	}
}
