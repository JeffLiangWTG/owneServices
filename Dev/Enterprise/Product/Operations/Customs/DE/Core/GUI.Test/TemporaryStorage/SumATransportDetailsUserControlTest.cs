using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class SumATransportDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestTransportModeAndMeansOrder()
		{
			using (var sumTransportDetailsControl = new SumATransportDetailsUserControl())
			{
				var transportDetailsGroupBox = sumTransportDetailsControl.Controls.Find("TransportDetailsGroupBox", true).Single();
				var transportMeansTabIndex = transportDetailsGroupBox.Controls["TransportMeansDropEdit"].TabIndex;
				AssertEquals(0, transportMeansTabIndex);
				AssertEquals(transportMeansTabIndex + 1, transportDetailsGroupBox.Controls["TransportModeDropEdit"].TabIndex);
			}
		}
	}
}
