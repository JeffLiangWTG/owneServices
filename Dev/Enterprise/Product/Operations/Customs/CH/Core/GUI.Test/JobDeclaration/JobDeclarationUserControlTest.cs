using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestDeclarationDetails()
	{
		using (var userControl = new JobDeclarationUserControl())
		{
			AssertEquals($"{nameof(userControl.PhaseStatusDescriptionTextBox)}.Visible", true, userControl.PhaseStatusDescriptionTextBox.Visible);
			AssertEquals($"{nameof(userControl.PhaseStatusDescriptionTextBox)}.CharacterCasing", CharacterCasing.Normal, userControl.PhaseStatusDescriptionTextBox.CharacterCasing);
			AssertEquals($"{nameof(userControl.MessageStatusDescriptionTextBox)}.Visible", true, userControl.PhaseStatusDescriptionTextBox.Visible);
			AssertEquals($"{nameof(userControl.MessageStatusDescriptionTextBox)}.CharacterCasing", CharacterCasing.Normal, userControl.MessageStatusDescriptionTextBox.CharacterCasing);
			AssertEquals($"{nameof(userControl.SelectionResultDescriptionTextBox)}.Visible", true, userControl.PhaseStatusDescriptionTextBox.Visible);
			AssertEquals($"{nameof(userControl.SelectionResultDescriptionTextBox)}.CharacterCasing", CharacterCasing.Normal, userControl.MessageStatusDescriptionTextBox.CharacterCasing);
		}
	}

	public void TestCustomsOfficeCodeFindBox()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		using (var userControl = new JobDeclarationUserControl())
		{
			userControl.JobDeclaration = declaration;

			var customsOfficeCodeFindBox = userControl.FindSingleOrDefault<ZCodeFindBox>("JE_CustomsOfficeCodeFindBox");
			AssertNotNull(nameof(customsOfficeCodeFindBox), customsOfficeCodeFindBox);

			AssertEquals($"{declaration.JE_MessageType} {nameof(customsOfficeCodeFindBox.Visible)}", true, customsOfficeCodeFindBox.Visible);

			declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals($"{declaration.JE_MessageType} {nameof(customsOfficeCodeFindBox.Visible)}", true, customsOfficeCodeFindBox.Visible);

			declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals($"{declaration.JE_MessageType} {nameof(customsOfficeCodeFindBox.Visible)}", false, customsOfficeCodeFindBox.Visible);
		}
	}
}
