using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(CustomsBrokerageUserControl))]
sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
{
	public void TestSupplierHeaderUserControl_Import()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertSupplierHeaderUserControl(typeof(ImportSupplierHeaderUserControl));
	}

	public void TestSupplierHeaderUserControl_Export()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertSupplierHeaderUserControl(typeof(ExportSupplierHeaderUserControl));
	}

	public void TestInvoiceLinesUserControl_Import()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertInvoiceLinesUserControl(typeof(ImportInvoiceLineUserControl));
	}

	public void TestInvoiceLinesUserControl_Export()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertInvoiceLinesUserControl(typeof(ExportInvoiceLineUserControl));
	}

	public void TestMiscOptionsUserControl()
	{
		BrokerageControl.MiscOptionsTabPage.Bind();
		AssertType<DynamicMiscOptionsUserControl>(BrokerageControl.DynamicMiscOptions);
	}

	public void TestEntryInstructionsTabVisibleForCountry()
	{
		AssertEquals("EntryInstructionsTabVisibleForCountry", true, BrokerageControl.EntryInstructionsTabVisibleForCountry);
	}

	public void TestContainerUserControl()
	{
		Declaration.JE_TransportMode = Constants.TransportModes.Sea;
		BrokerageControl.ContainerTabPage.Bind();
		AssertType<ContainerUserControl>(BrokerageControl.ContainerUserControl);
	}

	public void TestGetMessageUserControl()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertMessageUserControl(typeof(CustomsEntryAndDiscardedMessagesUserControl));

		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertMessageUserControl(typeof(EntriesWithMessagesOnDeclarationUserControl));
	}

	void AssertMessageUserControl(Type expected)
	{
		using var form = new ZForm(Declaration);
		using var userControl = new CustomsBrokerageUserControl();
		userControl.JobDeclaration = Declaration;
		form.Controls.Add(userControl);
		form.Show();
		userControl.MainTabControl.SelectedTab = userControl.MessagesTabPage;
		AssertEquals(expected, userControl.MessageUserControl.GetType());
	}

	public void TestEntryInstructionUserControl()
	{
		AssertEntryInstructionUserControl(typeof(EntryInstructionDetailsUserControl));
	}

	void AssertEntryInstructionUserControl(Type expected)
	{
		using var testForm = new JobDeclarationForm(Declaration);
		var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
		brokerageControl.EntryInstructionDetailsTabPage.Bind();
		AssertEquals(expected, brokerageControl.CustomsEntryInstructionUserControl.GetType());
	}

	void AssertInvoiceLinesUserControl(Type expectedType)
	{
		BrokerageControl.InvoiceLinesTabPage.Bind();
		AssertEquals(expectedType, BrokerageControl.InvoiceLinesUserControl.GetType());
	}

	void AssertSupplierHeaderUserControl(Type expected)
	{
		BrokerageControl.InvoicesTabPage.Bind();
		AssertEquals(expected, BrokerageControl.SupplierHeaderUserControl.GetType());
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
