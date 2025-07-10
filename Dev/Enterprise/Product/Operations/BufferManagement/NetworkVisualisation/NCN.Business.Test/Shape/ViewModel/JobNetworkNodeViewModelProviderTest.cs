using CargoWise.NetworkVisualisation.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class JobNetworkNodeViewModelProviderTest : NetworkTestCase
	{
		public void TestGetViewModel()
		{
			var network = new DummyNetwork { DiagramEntity = new Entity() };

			var entity = new Entity();
			var annotation = new Entity { ShapeType = ShapeTypeList.Codes.Annotation };
			var buffer = new Entity { ShapeType = ShapeTypeList.Codes.Buffer };

			var provider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = new NetworkViewModel(network);

			var normalEntityNode = provider.Create(entity, networkViewModel);
			var annotationNode = provider.Create(annotation, networkViewModel);
			var bufferEntityNode = provider.Create(buffer, networkViewModel);

			AssertType<JobNetworkNodeViewModel>(normalEntityNode);
			AssertType<AnnotationViewModel>(annotationNode);
			AssertType<BufferViewModel>(bufferEntityNode);
		}
	}
}
