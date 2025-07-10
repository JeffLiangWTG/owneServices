using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public sealed class AsycudaTransportDocumentDetailsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaTransportDocumentDetailsControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("typeDropEdit"));
				AssertNotNull(control.FindSingle<ZTextBox>("referenceTextBox"));
			}
		}
	}
}
