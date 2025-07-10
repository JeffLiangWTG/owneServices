using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class ITJobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestControlsVisible()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("BadgeCodeDropEdit Visible", false, control.BadgeCodeDropEdit.Visible);
			AssertEquals("SoeDropDown not Visible", false, control.StyleOfEntrySOEDropDown.Visible);
		});
	}

	public void TestCustomsOfficeOfPresentationIsRemoved()
	{
		var customsOfficeOfPresentation = control.Controls.Find("CustomsOfficeCodeFindBox", true).SingleOrDefault();
		AssertNull("CustomsOfficeCodeFindBox should be", customsOfficeOfPresentation);
	}

	public void TestGetCustomsOfficesUserControlType()
	{
		form.Show();
		var customsOfficesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");
		AssertEquals(typeof(CustomsOfficesUserControl), customsOfficesUserControl.UserControlType);
		AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(255, 696, true), customsOfficesUserControl.Location);
	}

	public void TestShouldOverrideC100SupportingDocumentRexNumberConfirmationBoxIsShownWhenDoMerge()
	{
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "200";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var supplierWithRexCode = Factory.New<OrgHeader>();
		var rexCode = supplierWithRexCode.CustomsCodes.AddNew();
		rexCode.OK_CodeType = "REX";
		rexCode.OK_CustomsRegNo = "REX12345";

		var c100SupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		c100SupportingDocument.CSI_Code = "C100";
		c100SupportingDocument.CSI_ReferenceNumber = "REX00000";

		declaration.JE_OH_Supplier = supplierWithRexCode.PK;

		var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
		customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.DeclarationTabPage;

		UnitTestUserNotification.Instance.ClearMessages();
		declaration.DoMerge();

		AssertEquals("Confirmation box message", "Do you want to update the existing Supporting Documents of type C100 with the REX code of the Supplier?", UnitTestUserNotification.Instance.LastMessage.Text);
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
	}

	public void TestOnGetEntryToPrintSadHC88()
	{
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.CustomsEntryHeaders.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var sadh88MenuItemForTesting = Factory.New<IStmMenuItem>();
		sadh88MenuItemForTesting.SU_MenuName = "SADH C88";

		declaration.DocumentSupporter.GetDataStateBeforeRun(sadh88MenuItemForTesting);
		AssertType<SadDocumentSupporterConfiguratorForm>(ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestMessageVersionControlVisibility()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = "ITF";
		AssertEquals("When EXP and Application code ITF, MessageVersion control visibility", false, control.MessageVersionDropEdit.Visible);

		declaration.JE_ApplicationCode = "BLT";
		AssertEquals("When EXP and Application code BLT, MessageVersion control visibility", true, control.MessageVersionDropEdit.Visible);

		declaration.JE_MessageType = "IMP";
		AssertEquals("When IMP, MessageVersion control visibility", false, control.MessageVersionDropEdit.Visible);
	}

	public void TestShipmentTypeGroupBoxHeight()
	{
		AssertEquals("Height", 310, control.ShipmentTypeGroupBox.Height);
	}

	public void TestShipmentDetailsGroupBox()
	{
		AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(255, 362, true), control.ShipmentDetailsGroupBox.Location);
	}

	public void TestTransportDetailsGroupBox()
	{
		AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(464, 305, true), control.TransportDetailsGroupBox.Size);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		form = new JobDeclarationForm(declaration);
		control = (JobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControl;
	}

	protected override void TearDown()
	{
		base.TearDown();
		form.Dispose();
	}
	JobDeclaration declaration;
	JobDeclarationForm form;
	JobDeclarationUserControl control;
}
