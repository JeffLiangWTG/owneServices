using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InAndOutwardProcessingUserControl))]
sealed class InAndOutwardProcessingUserControlTest : TestCaseWithFactory
{
	public void TestNotifyCustomsOfficeGridVisibility() => CombineAssertions(() =>
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = new BaseInvoiceLineUserControl())
		{
			form.Controls.Add(parent);
			form.Show();
			var userControl = parent.InAndOutwardProcessingUserControl;
			parent.LineDetailTabControl.SelectedTab = parent.InAndOutwardProcessingTabPage;
			form.Show();
			declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals($"MessageType={declaration.JE_MessageType}", false, userControl.NotifyCustomsOfficesGroupBox.Visible);
			declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals($"MessageType={declaration.JE_MessageType}", true, userControl.NotifyCustomsOfficesGroupBox.Visible);
			declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals($"MessageType={declaration.JE_MessageType}", false, userControl.NotifyCustomsOfficesGroupBox.Visible);
		}
	});

	public void TestGridLayout()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = new BaseInvoiceLineUserControl())
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.InAndOutwardProcessingTabPage;
			var inAndOutwardProcessingGridColumsStyles = parent.InAndOutwardProcessingUserControl.NotifyCustomsOffice.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(inAndOutwardProcessingGridColumsStyles, NotifyCustomsOffice.Schema.CY_Data, 0);
				UserControlTestHelper.AssertColumnStyles(inAndOutwardProcessingGridColumsStyles, NotifyCustomsOffice.Schema.DataDescription, 1);
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
