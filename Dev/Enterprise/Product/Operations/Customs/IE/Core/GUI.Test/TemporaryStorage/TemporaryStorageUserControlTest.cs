using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class TemporaryStorageUserControlTest : TestCaseWithFactory
	{
		public void TestManifestTypeDropEdit()
		{
			TestControl<ZDropEdit>("ManifestTypeDropEdit", "AMA_ManifestType");
		}

		public void TestCustomsOfficeofLodgementCodeFindBox()
		{
			TestControl<ZCodeFindBox>("CustomsOfficeofLodgementCodeFindBox", "CustomsOfficeOfLodgement");
		}

		public void TestRepresentativeStatusDropEdit()
		{
			TestControl<ZDropEdit>("RepresentativeStatusDropEdit", "AMA_AgentType");
		}

		public void TestBorderTransportTypeDropEdit()
		{
			TestControl<ZDropEdit>("BorderTransportTypeDropEdit", "AMA_TransportMeans");
		}

		public void TestBorderTransportIDTextBox()
		{
			TestControl<ZTextBox>("BorderTransportIDTextBox", "AMA_VesselName");
		}

		void TestControl<T>(string controlName, string expectedBindTo) where T : Control
		{
			using (var tsControl = new TemporaryStorageUserControl())
			{
				var control = tsControl.FindSingle<T>(controlName);
				AssertNotNull(controlName, control);
				AssertEquals("Binding", expectedBindTo, control.GetBindingMember());
				AssertEquals("Visible", true, control.Visible);
			}
		}
	}
}
