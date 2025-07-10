using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.GUI.Testing
{
	[TestedType(typeof(GVMSManifestLayoutBuilder<AsycudaManifestHeader>))]
	class GVMSManifestLayoutBuilderTests : ColumnLayoutBuilderAbstractTest<GVMSManifestLayoutBuilder<AsycudaManifestHeader>, AsycudaManifestHeader, CommonManifestControlBag>
	{
		protected override GVMSManifestLayoutBuilder<AsycudaManifestHeader> GetColumnLayoutBuilderForTesting() => new GVMSManifestLayoutBuilder<AsycudaManifestHeader>();
		protected override int ExpectedMaxColumns => 3;
		public void TestROAFieldsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			using (var form = new ManifestForm(manifest))
			using (var control = new AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var trailer1RegNoTextBox = control.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer1RegNoTextBox));
				var trailer2RegNoTextBox = control.FindSingle<ZTextBox>(nameof(CommonManifestControlBag.Trailer2RegNoTextBox));
				AssertEquals(true, trailer1RegNoTextBox.Visible);
				AssertEquals(true, trailer2RegNoTextBox.Visible);
				manifest.AMA_TransportMode = manifest.AMA_TransportMode = Core.Constants.TransportModes.RollOnRollOff;
				AssertEquals(true, trailer1RegNoTextBox.Visible);
				AssertEquals(true, trailer2RegNoTextBox.Visible);
				manifest.AMA_TransportMode = manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, trailer1RegNoTextBox.Visible);
				AssertEquals(true, trailer2RegNoTextBox.Visible);
			}
		}
	}
}
