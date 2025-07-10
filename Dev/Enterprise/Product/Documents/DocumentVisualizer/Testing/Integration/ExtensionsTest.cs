using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.DocumentVisualizer.Integration.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestIsSupportedByDocumentVisualizer()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			Assert("consol is supported by Document Visualizer", consol.IsSupportedByDocumentVisualizer());

			var dummy = Factory.New<DummyBusinessObject>();
			Assert("dummy is not supported by Document Visualizer", !dummy.IsSupportedByDocumentVisualizer());

			Assert("null biz obj is not supported by Document Visualizer", !((BusinessObject)null).IsSupportedByDocumentVisualizer());
		}

		public void TestGetSupporter()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			AssertNotNull("GetSupporter for consol has been found", consol.GetSupporter());

			var dummy = Factory.New<DummyBusinessObject>();
			AssertNull("GetSupporter for dummy which does not support Document Visualizer", dummy.GetSupporter());

			AssertNull("GetSupporter for null biz obj which does not support Document Visualizer", ((BusinessObject)null).GetSupporter());
		}
	}
}
