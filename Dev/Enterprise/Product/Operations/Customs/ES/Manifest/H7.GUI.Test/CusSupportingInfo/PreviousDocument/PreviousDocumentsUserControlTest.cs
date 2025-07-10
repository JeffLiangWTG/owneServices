using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	sealed class PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestCodeTypeDropEdit()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new PreviousDocumentsUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var codeTypeColumn = grid.GetColumnStyle("CSI_Code");

				AssertType(typeof(ZDropEditColumnStyleInfo), codeTypeColumn);
			}
		}
	}
}
