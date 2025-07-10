using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestsSubclassesOf(typeof(CustomsBrokerageUserControl))]
	abstract class CustomsBrokerageUserControlAbstractTest<T> : TestCaseWithFactory
		where T : CustomsBrokerageUserControl
	{
		public void TestSupplierHeaderUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(ImportSupplierHeaderUserControl);
		}

		public void TestSupplierHeaderUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(ExportSupplierHeaderUserControl);
		}

		public void TestInvoiceLinesUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(ImportInvoiceLineUserControl);
		}

		public void TestInvoiceLinesUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(ExportInvoiceLineUserControl);
		}

		public void TestMessageUserControl()
		{
			control.MainTabControl.SelectedTab = control.MessagesTabPage;
			AssertEquals(MessageUserControlType, control.MessageUserControl.GetType());
		}

		public void TestContainerUserControl()
		{
			control.ContainerTabPage.TabVisible = true;
			control.MainTabControl.SelectedTab = control.ContainerTabPage;
			AssertEquals(ContainerUserControlType, control.ContainerUserControl.GetType());
		}

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

		protected abstract Type ImportSupplierHeaderUserControl { get; }
		protected abstract Type ExportSupplierHeaderUserControl { get; }
		protected abstract Type ImportInvoiceLineUserControl { get; }
		protected abstract Type ExportInvoiceLineUserControl { get; }
		protected abstract Type MiscOptionsUserControlType { get; }
		protected abstract Type MessageUserControlType { get; }
		protected abstract Type ContainerUserControlType { get; }

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Business.JobDeclaration>();
			form = new ZForm(declaration);
			control = Activator.CreateInstance<T>();
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

		protected Business.JobDeclaration declaration;
		protected CustomsBrokerageUserControl control;
		ZForm form;
	}
}
