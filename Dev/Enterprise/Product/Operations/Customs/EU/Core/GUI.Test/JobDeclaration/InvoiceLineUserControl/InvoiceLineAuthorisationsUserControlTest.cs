using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class InvoiceLineAuthorisationsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_AuthorisationsGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusAuthorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new InvoiceLineAuthorisationsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var authorisationsGrid = control.FindSingleOrDefault<ZGrid>("AuthorisationsGrid");

					AssertEquals("Columns", 6, authorisationsGrid.ColumnStyles.Count);
					AssertEquals("AGC_Code", 40, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Code).Width);
					AssertEquals("CustomsCode", 100, authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode)).Width);
					AssertEquals("AGC_Number", 130, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Number).Width);
					AssertEquals("AGC_OH_Owner", 80, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_OH_Owner).Width);
					AssertEquals("AGC_CPH_Authorization", 130, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization).Width);
					AssertEquals("CPH_Number", 130, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").Width);
				});
			}
		}
	}
}
