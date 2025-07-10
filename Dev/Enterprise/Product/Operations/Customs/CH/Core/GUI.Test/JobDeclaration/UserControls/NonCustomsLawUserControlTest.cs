using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

internal class NonCustomsLawUserControlTest : TestCaseWithFactory
{
	public void TestNonCustomsLawGridLayout()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.NonCustomsLawTabPage;
			var nonCustomsLawGridColumsStyles = parent.NonCustomsLawUserControl.NonCustomsLawGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(nonCustomsLawGridColumsStyles, NonCustomsLaw.Schema.CSI_Code, 0);
				UserControlTestHelper.AssertColumnStyles(nonCustomsLawGridColumsStyles, NonCustomsLaw.Schema.CodeDescription, 1);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
}
