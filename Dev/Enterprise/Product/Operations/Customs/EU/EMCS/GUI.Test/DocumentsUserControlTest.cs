using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	class DocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestCetificates_UpperLowerCase()
		{
			using (var form = new ZForm(declaration))
			using (var control = new DocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var document = declaration.Documents.AddNew();
				document.CSI_Description = "ABC description";
				document.CSI_ReferenceNumber = "REFERENCE number";

				var documentsGrid = control.FindSingle<ZGrid>("DocumentsGrid");
				documentsGrid.Select(0);
				var row = (EMCSDocument)documentsGrid.GetFirstSelectedRow();

				CombineAssertions(() =>
				{
					AssertEquals("ABC description", row.CSI_Description);
					AssertEquals("REFERENCE number", row.CSI_ReferenceNumber);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
