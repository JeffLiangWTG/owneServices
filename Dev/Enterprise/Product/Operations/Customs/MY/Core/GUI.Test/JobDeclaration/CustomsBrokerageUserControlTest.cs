using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestSupplierHeaderUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(typeof(MYImportSupplierHeaderUserControl));
		}

		public void TestSupplierHeaderUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(typeof(MYExportSupplierHeaderUserControl));
		}

		public void TestInvoiceLinesUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(typeof(MYImportInvoiceLineUserControl));
		}

		public void TestInvoiceLinesUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(typeof(MYExportInvoiceLineUserControl));
		}

		public void TestMiscOptionsUserControl()
		{
			control.MainTabControl.SelectedTab = control.MiscOptionsTabPage;
			AssertEquals(typeof(MiscOptionsUserControl), control.MiscOptions.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Business.JobDeclaration>();
			form = new ZForm(declaration);
			control = new CustomsBrokerageUserControl();
			form.Controls.Add(control);
			form.Show();
			control.JobDeclaration = declaration;
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
			form.Dispose();
		}
		Business.JobDeclaration declaration;
		CustomsBrokerageUserControl control;
		ZForm form;

		void AssertInvoiceLinesUserControl(Type expected)
		{
			control.MainTabControl.SelectedTab = control.InvoiceLinesTabPage;
			AssertEquals(expected, control.InvoiceLinesUserControl.GetType());
		}

		void AssertSupplierHeaderUserControl(Type expected)
		{
			control.MainTabControl.SelectedTab = control.InvoicesTabPage;
			AssertEquals(expected, control.SupplierHeaderUserControl.GetType());
		}
	}
}
