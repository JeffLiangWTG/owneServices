using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ManifestModuleButtonGrid))]
	internal class ManifestModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestHideButtonRow()
		{
			using (var manifestModuleButtonGrid = new ManifestModuleButtonGrid())
			{
				var toolStrip = manifestModuleButtonGrid.FindSingle<ZToolStrip>("toolStrip");
				AssertGreaterThan("toolStrip row visible", toolStrip.Size.Height, 20);
				AssertGreaterThan("grid sized to show toolStrip", manifestModuleButtonGrid.Height - manifestModuleButtonGrid.InnerGrid.Height, 20);

				manifestModuleButtonGrid.HideButtonRow();
				AssertLessThan("toolStrip row collapsed", toolStrip.Size.Height, 2);
				AssertLessThan("grid sized to cover toolStrip (with margins)", manifestModuleButtonGrid.Height - manifestModuleButtonGrid.InnerGrid.Height, 8);
			}
		}
	}
}
