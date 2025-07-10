using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

class TobaccoUserControlTest : TestCaseWithFactory
{
	public void TestTobaccoGridLayout()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = new BaseInvoiceLineUserControl())
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.TobaccosTabPage;
			var tobaccosGridColumsStyles = parent.TobaccoUserControl.TobaccosGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_Code, 0);
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_SubType, 1);
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_Description, 2);
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_ItemNumber, 3);
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_Value, 4);
				UserControlTestHelper.AssertColumnStyles(tobaccosGridColumsStyles, Tobacco.Schema.CSI_AdditionalDescription, 5);
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
