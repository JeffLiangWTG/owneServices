using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;

			var rightTabControl = jobDeclarationUserControl.RightTabControl;
			AssertEquals("DetailsTabPage should be the first tab", 0, rightTabControl.TabPages.IndexOf(jobDeclarationUserControl.DetailsTabPage));
			AssertEquals("Default selected tab", jobDeclarationUserControl.DetailsTabPage, rightTabControl.SelectedTab);
			AssertEquals("ExportOrientedUnitsTabPage should be the first tab", 1, rightTabControl.TabPages.IndexOf(jobDeclarationUserControl.ExportOrientedUnitsTabPage));
		}
	}

	public void TestImporterAndSupplierDocAddressControlsExistence()
	{
		using (var control = new JobDeclarationUserControl())
		{
			CombineAssertions(() =>
			{
				control.AssertContainsControl<ZDocAddressControl>("ImporterDocAddress", x => x
				.WithBindTo(nameof(JobDeclaration.ImporterDocumentaryAddress))
				.WithCaption("Importer"));

				control.AssertContainsControl<ZDocAddressControl>("SupplierDocAddress", x => x
				.WithBindTo(nameof(JobDeclaration.SupplierDocumentaryAddress))
				.WithCaption("Main Supplier"));
			});
		}
	}

	public void TestDeclarationDetailsGroupBox()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		using (var control = new JobDeclarationUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, control.DeclarationDetailsGroupBox.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, control.DeclarationDetailsGroupBox.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals(false, control.DeclarationDetailsGroupBox.Visible);
			});
		}
	}

	public void TestExportOrientedUnitsTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;

			CombineAssertions(() =>
			{
				AssertEquals(true, jobDeclarationUserControl.ExportOrientedUnitsTabPage.TabVisible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Application.DoEvents();
				AssertEquals(false, jobDeclarationUserControl.ExportOrientedUnitsTabPage.TabVisible);
			});
		}
	}
}
