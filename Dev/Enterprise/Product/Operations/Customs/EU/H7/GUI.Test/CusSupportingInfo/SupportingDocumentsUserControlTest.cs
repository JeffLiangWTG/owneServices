using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	sealed class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestLayout()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new SupportingDocumentsUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");

				EUH7GUITestHelper.AssertGridLayout(grid,
					[
						(CusSupportingInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo)),
						(SupportingDocument.Schema.DocumentDescription, typeof(ZTextBoxColumnStyleInfo))
					]);
				EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, "CSI_Code");
				EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, "CSI_ReferenceNumber");
				EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, "DocumentDescription");
			}
		}
	}
}
