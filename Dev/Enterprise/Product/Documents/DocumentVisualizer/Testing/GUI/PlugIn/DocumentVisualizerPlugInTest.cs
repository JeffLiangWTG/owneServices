using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DocumentVisualizerPlugInTest : ZPlugInGenericTest
	{
		public void TestDocumentVisualizerPlugIn_Disabled()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as BusinessObject;
			using (var plugIn = new DocumentVisualizerPlugIn(consol))
			{
				AssertEquals("Plugin is disabled but is still required for ZDocumentMenuItem.GetDocumentCustomisationMenuItemMenusMaker", false, plugIn.Enabled);
			}
		}

		#region Implementation

		protected override ZPlugIn GetPlugInToTest()
		{
			return new DocumentVisualizerPlugIn(Factory.New<DummyBusinessObject>());
		}

		#endregion
	}
}
