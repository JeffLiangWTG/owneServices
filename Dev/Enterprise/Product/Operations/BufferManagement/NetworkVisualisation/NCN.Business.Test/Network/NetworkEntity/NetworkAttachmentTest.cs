using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(NetworkAttachment))]
	class NetworkAttachmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSharesAttachmentProperties()
		{
			var attachment = (NetworkAttachment)GetNewBusinessObject();
			AssertEquals(attachment.Attachment.PK, attachment.PK);
			AssertEquals(attachment.Attachment.HumanReadableName, attachment.HumanReadableName);

			attachment.Attachment.Delete();
			Assert(attachment.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestAllowEndpointOutsideNetwork()
		{
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(NetworkTestCase.CreateDiagram(Factory));
			var network = networkViewModel.GetJobNetwork();
			var s1 = networkViewModel.CreateNewShape(network.DiagramEntity);
			var s2 = networkViewModel.CreateNewShape(network.DiagramEntity);
			var attachment = (NetworkAttachment)network.CreateRelationship(s1, s2);

			var childNetwork = NetworkTestCase.CreateNetwork(s2.Shape);
			var otherAttachment = childNetwork.Entities.GetInstance(attachment.Attachment);

			AssertEquals(s1.Shape.PK, otherAttachment.From.Shape.PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(NetworkTestCase.CreateDiagram(Factory));
			var network = networkViewModel.GetJobNetwork();
			var s1 = networkViewModel.CreateNewShape(network.DiagramEntity);
			var s2 = networkViewModel.CreateNewShape(network.DiagramEntity);
			var attachment = network.CreateRelationship(s1, s2);

			return (NetworkAttachment)attachment;
		}
	}
}
