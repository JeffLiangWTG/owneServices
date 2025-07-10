using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public sealed class AsycudaTransportDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaTransportDocumentsUserControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("TransportDocumentTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 1, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
				{
					"TransportDocumentDetailsTabPage"
				}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestAdditionalTabPage()
		{
			using (var control = new AsycudaTransportDocumentsUserControl())
			{
				AssertEquals("Control's additional tab caption shoud be as expected", "Transport Documents", ((IAdditionalTabPage)control).AdditionalTabPageCaption.Caption);
				AssertEquals("Control's tab page sequence shoud be 1", 1, ((IAdditionalTabPage)control).TabPageSequence);
				AssertEquals("Control should be visible", true, ((IAdditionalTabPage)control).AdditionalControlVisibility.isVisible.Invoke(null));
				AssertSame("Control should be referenced correctly", control, ((IAdditionalTabPage)control).AdditionalTabPageUserControl);
			}
		}
	}
}
