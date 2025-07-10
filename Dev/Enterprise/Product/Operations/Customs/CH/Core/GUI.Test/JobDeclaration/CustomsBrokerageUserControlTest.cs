using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

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
		using (var testForm = new JobDeclarationForm(declaration))
		{
			var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
			brokerageControl.EntryInstructionDetailsTabPage.Bind();
			AssertType<EntryInstructionDetailsUserControl>(brokerageControl.CustomsEntryInstructionUserControl);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	void AssertInvoiceLinesUserControl(JobDeclaration declaration, Type expectedType)
	{
		using (var testForm = new JobDeclarationForm(declaration))
		{
			var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
			brokerageControl.InvoiceLinesTabPage.Bind();
			AssertEquals(expectedType, brokerageControl.InvoiceLinesUserControl.GetType());
		}
	}

	void AssertSupplierHeaderUserControl(JobDeclaration declaration, Type expected)
	{
		using (var testForm = new JobDeclarationForm(declaration))
		{
			var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
			brokerageControl.InvoicesTabPage.Bind();
			AssertEquals(expected, brokerageControl.SupplierHeaderUserControl.GetType());
		}
	}

	public void TestMessageUserControl()
	{
		using (var testForm = new JobDeclarationForm(Factory.New<JobDeclaration>()))
		{
			var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
			brokerageControl.LoadMessageTabPage();
			var messageUserControl = brokerageControl.MessageUserControl;
			AssertType<MessageUserControl>(messageUserControl);
			messageUserControl.Dispose();
			brokerageControl.Dispose();
		}
	}
}
