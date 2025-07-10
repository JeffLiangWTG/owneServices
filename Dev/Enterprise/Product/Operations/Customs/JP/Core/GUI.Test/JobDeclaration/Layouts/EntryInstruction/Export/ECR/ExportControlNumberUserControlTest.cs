using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(ExportControlNumberUserControl))]
sealed class ExportControlNumberUserControlTest : TestCaseWithFactory
{
	public void TestDeleteButtonVisible()
	{
		var (decl, _, entryInstruction) = GetTestData();
		using var form = new ZForm(decl);
		form.SetDataBinding(form.BusinessEntity, "CustomsEntryInstructions");
		using var control = new ExportControlNumberUserControl();
		form.Controls.Add(control);
		form.Show();
		var deleteButton = control.FindSingle<ZButton>("DeleteButton");
		Assert("Should be visible", deleteButton.Visible);

		entryInstruction.ExportControlNumberInfo.ClearValue();
		Assert("Shouldn't be visible", !deleteButton.Visible);
	}

	public void TestDeleteButton_Click()
	{
		var (decl, header, _) = GetTestData();
		decl.Factory.Save();
		using var form = new ZForm(decl);
		form.SetDataBinding(form.BusinessEntity, "CustomsEntryInstructions");
		using var control = new ExportControlNumberUserControl();
		form.Controls.Add(control);
		form.Show();
		var deleteButton = control.FindSingle<ZButton>("DeleteButton");

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var dialog = (MessageSendingForm)obj;
			var messageSendingParent = dialog.BusinessEntity;
			var context = messageSendingParent.Context;
			CombineAssertions(() =>
			{
				Assert("EnableMessageVisual", context.EnableMessageVisual);
				AssertEquals("ProcedureCode", JPProcedureCodeList.Codes.ECR, context.ProcedureCode);
				AssertEquals("Action", ActionList.Codes.One, context.Action);
				AssertContainsExactElementsInAnyOrder("EntryHeadersToBeSent", [header], context.EntryHeadersToBeSent);
			});
		});

		deleteButton.PerformClick();
	}

	(JobDeclaration decl, CusEntryHeader header, CusEntryInstruction instruction) GetTestData()
	{
		var declarationForTesting = Factory.NewWithValidTestData<JobDeclaration>();
		declarationForTesting.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declarationForTesting.JE_MessageType = JPJobMessageTypeList.Codes.Export;
		var entryHeader = declarationForTesting.CustomsEntryHeaders.AddNew();
		var instruction = declarationForTesting.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		instruction.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, "123", true);
		return (declarationForTesting, entryHeader, instruction);
	}
}
