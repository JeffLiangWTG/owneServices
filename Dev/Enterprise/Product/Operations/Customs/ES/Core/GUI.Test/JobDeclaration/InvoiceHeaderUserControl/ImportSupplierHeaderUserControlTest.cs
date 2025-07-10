using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ImportSupplierHeaderUserControl))]
public class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

	public void TestSupplierOrgPKColumn()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
			var supplierHeaderUserControl = (ImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
			var supplierColumnStyle = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "SupplierOrgPK");
			AssertNull(supplierColumnStyle);
		}
	}

	public void TestAdditionalDocumentsTabPageWhenNotUCC6()
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportSupplierHeaderUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalDocumentsTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalDocumentsTabPage visible", expected: true, tabPage.TabVisible);
				AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("additionalDocumentsUserControl");
				AssertEquals("UserControl type", typeof(AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
			});
		}
	}

	public void TestAdditionalDocumentsTabPageWhenUCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportSupplierHeaderUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabControl = userControl.InvoiceTabControl;
			var additionalDocumentsTabPage = tabControl.FindSingleOrDefault<ZTabPage>("AdditionalDocumentsTabPage");
			var addInfoTabPage = tabControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");

			AssertNull("Additional Documents tab page should not exist", additionalDocumentsTabPage);
			AssertNotNull("Add Info Documents tab page should exist", addInfoTabPage);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JobComInvoiceLines.AddNew();
	}

	JobDeclaration declaration;
}
