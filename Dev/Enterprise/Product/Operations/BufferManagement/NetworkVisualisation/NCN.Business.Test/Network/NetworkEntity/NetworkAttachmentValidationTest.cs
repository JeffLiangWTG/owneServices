using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCreatingNetworkAttachmentRunsValidation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagramShape);
			var shape2 = networkViewModel.CreateNewShape(diagramShape);

			shape1.SetCoordinates(300, 300, 0, 0);
			shape2.SetCoordinates(300, 300, 0, 0);

			var attachment = (NetworkAttachment)network.CreateRelationship(shape1, shape2);
			AssertHasRowWarning(attachment, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
		}
	}
}
