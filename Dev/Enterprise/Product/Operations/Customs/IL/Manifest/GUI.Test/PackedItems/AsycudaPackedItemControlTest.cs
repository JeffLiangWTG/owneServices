using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public class AsycudaPackedItemControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaPackedItemControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 3, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
				{
					"PackedItemDetailsTabPage", "PackPackedItemPivotTabPage", "AdditionalInfoTabPage"
				}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestSplitterSize()
		{
			using (var control = new AsycudaPackedItemControl())
			{
				var splitter = control.FindSingle<KSplitter>("Splitter");
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(862, 3, true), splitter.Size);
			}
		}

		public void TestPackedItemTabControlSize()
		{
			using (var control = new AsycudaPackedItemControl())
			{
				var packedItemTabControl = control.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(862, 200, true), packedItemTabControl.Size);
				var additionalTabPage = control as IAdditionalTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Items", additionalTabPage.AdditionalTabPageCaption.Caption);
					AssertEquals(1, additionalTabPage.TabPageSequence);
					AssertEquals(true, additionalTabPage.AdditionalControlVisibility.isVisible(null));
					AssertEquals(control, additionalTabPage.AdditionalTabPageUserControl);
				});
			}
		}
	}
}
