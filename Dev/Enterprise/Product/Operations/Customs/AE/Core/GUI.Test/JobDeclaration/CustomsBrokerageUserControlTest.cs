using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI.Testing;

public class CustomsBrokerageUserControlTest : TestCaseWithFactory
{
	public void TestMessageUserControl()
	{
		BrokerageControl.MessagesTabPage.Bind();
		AssertType<MessageUserControl>(BrokerageControl.MessageUserControl);
	}

	public void TestDeclarationUserControl()
	{
		BrokerageControl.DeclarationTabPage.Bind();
		AssertType<AEJobDeclarationUserControl>(BrokerageControl.DeclarationUserControl);
	}

	public void TestSupplierHeaderUserControl()
	{
		BrokerageControl.InvoicesTabPage.Bind();
		AssertType<SupplierHeaderUserControl>(BrokerageControl.SupplierHeaderUserControl);
	}

	public void TestInvoiceGroupingUserControl()
	{
		BrokerageControl.InvoiceGroupingTabPage.Bind();
		AssertType<GroupInvoiceUserControl>(BrokerageControl.InvoiceGroupUserControl);
	}

	public void TestMiscOptionsUserControl()
	{
		BrokerageControl.MiscOptionsTabPage.Bind();
		AssertType<MiscOptionsUserControl>(BrokerageControl.MiscOptions);
	}

	public void TestEntryInstructionUserControl()
	{
		BrokerageControl.EntryInstructionDetailsTabPage.Bind();
		AssertType<EntryInstructionUserControl>(BrokerageControl.CustomsEntryInstructionUserControl);
	}

	public void TestEntryInstructionsTabVisible()
		=> Assert("EntryInstructionDetailsTab visible", BrokerageControl.EntryInstructionDetailsTabPage.TabVisible);

	public void TestEntryInstructionsUserControl()
	{
		BrokerageControl.EntryInstructionDetailsTabPage.Bind();
		AssertType<EntryInstructionUserControl>(BrokerageControl.CustomsEntryInstructionUserControl);
	}

	public void TestInvoiceLinesUserControl()
	{
		BrokerageControl.InvoiceLinesTabPage.Bind();
		AssertType<InvoiceLineUserControl>(BrokerageControl.InvoiceLinesUserControl);
	}

	protected override void TearDown()
	{
		base.TearDown();
		if (declarationForm != null)
		{
			BrokerageControl.Dispose();
			declarationForm.Dispose();
		}
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobDeclarationForm DeclarationForm => declarationForm ??= new JobDeclarationForm(Declaration);
	JobDeclarationForm declarationForm;

	CustomsBrokerageUserControl BrokerageControl => (CustomsBrokerageUserControl)DeclarationForm.CustomsBrokerageUserControl;
}
