using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.Data
{
	sealed class ObjectFactoryRegistrationTest : TestCase
	{
		public void TestGetVisualizerDocumentData()
		{
			var factory = new BusinessObjectFactory();

			var obj = factory.New<Integration.IVisualizerDocumentData>();

			Assert("IVisualizerDocumentData is obtainable via ObjectFactory", obj is VisualizerDocumentData);
		}

		public void TestGetVisualizerDocumentDataLoader()
		{
			var obj = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			Assert("IVisualizerDocumentDataLoader is obtainable via ObjectFactory", obj is VisualizerDocumentDataLoader);
		}

		public void TestGetVisualizerDocumentDataContextManager()
		{
			var obj = ObjectFactory.Get("VisualizerDocumentDataContextManager");

			Assert("VisualizerDocumentDataContextManager is obtainable via ObjectFactory", obj is VisualizerDocumentDataContextManager);
		}
	}
}