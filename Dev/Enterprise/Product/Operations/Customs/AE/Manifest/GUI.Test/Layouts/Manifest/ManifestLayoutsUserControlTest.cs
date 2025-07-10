using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

sealed class ManifestLayoutsUserControlTest : TestCaseWithFactory
{
	public void TestCarrierMPCITextBox() => AssertType<ZTextBox>(control.Find(c => c.Name == "CarrierMPCITextBox").First());

	public void TestShippingAgentMPCITextBox() => AssertType<ZTextBox>(control.Find(c => c.Name == "ShippingAgentMPCITextBox").First());

	protected override void SetUp()
	{
		base.SetUp();
		control = new ManifestLayoutsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ManifestLayoutsUserControl control;
}
