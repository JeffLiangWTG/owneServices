using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestSupplierHeaderUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(ImportSupplierHeaderUserControl));
		}

		public void TestSupplierHeaderUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(ExportSupplierHeaderUserControl));
		}

		public void TestInvoiceLinesUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(ImportInvoiceLineUserControl));
		}

		public void TestInvoiceLinesUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(ExportInvoiceLineUserControl));
		}

		public void TestEntryInstructionUserControl()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEntryInstructionUserControl(declaration, typeof(EntryInstructionDetailsUserControl));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEntryInstructionUserControl(declaration, typeof(EntryInstructionDetailsUserControl));
		}

		public void TestMiscOptionsUserControl()
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.MiscOptionsTabPage.Bind();
				AssertType<MiscOptionsUserControl>(brokerageControl.MiscOptions);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Business.JobDeclaration>();
		}
		Business.JobDeclaration declaration;

		void AssertInvoiceLinesUserControl(Business.JobDeclaration declaration, Type expectedType)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.InvoiceLinesTabPage.Bind();
				AssertEquals(expectedType, brokerageControl.InvoiceLinesUserControl.GetType());
			}
		}

		void AssertSupplierHeaderUserControl(Business.JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.InvoicesTabPage.Bind();
				AssertEquals(expected, brokerageControl.SupplierHeaderUserControl.GetType());
			}
		}

		void AssertEntryInstructionUserControl(Business.JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				AssertEquals(expected, brokerageControl.CustomsEntryInstructionUserControl.GetType());
			}
		}
	}
}
