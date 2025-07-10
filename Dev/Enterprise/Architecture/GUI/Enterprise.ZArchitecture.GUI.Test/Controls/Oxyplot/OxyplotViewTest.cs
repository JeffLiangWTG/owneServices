using System;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class OxyplotViewTest : ControlTestCase<OxyplotView>
	{
#if !WINZOR
		public void TestPlotModelNullifiedOnDispose()
		{
			OxyplotView plotView = new OxyplotView();
			plotView.Disposed += new EventHandler(OnPlotview_Disposed);
			plotView.Dispose();
		}

		void OnPlotview_Disposed(object sender, EventArgs e)
		{
			OxyplotView plotView = (OxyplotView)sender;
			AssertNull("The model should be nullified now", plotView.Model);
		}
#endif
	}
}
