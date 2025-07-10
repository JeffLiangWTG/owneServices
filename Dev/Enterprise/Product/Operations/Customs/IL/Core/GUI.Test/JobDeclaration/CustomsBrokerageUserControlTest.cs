using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
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

		public void TestMiscOptionsUserControl()
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.MiscOptionsTabPage.Bind();
				AssertType<MiscOptionsUserControl>(brokerageControl.MiscOptions);
			}
		}

		public void TestGetEntryInstructionUserControl()
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.EntryInstructionDetailsTabPage.Bind();
				AssertType<EntryInstructionDetailsUserControl>(brokerageControl.CustomsEntryInstructionUserControl);
			}
		}

		public void TestGetMessageUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<Business.JobDeclaration>()))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.LoadMessageTabPage();
				var messageUserControl = brokerageControl.MessageUserControl;
				AssertType<EntriesTabUserControl>(messageUserControl);
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
	}
}
