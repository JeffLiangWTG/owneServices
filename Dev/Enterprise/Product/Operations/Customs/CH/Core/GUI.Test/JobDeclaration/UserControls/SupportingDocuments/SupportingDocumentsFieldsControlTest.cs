using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.GUI.Testing;

public class SupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using (var control = new SupportingDocumentsFieldsControl())
		{
			AssertEquals(typeof(SupportingDocument), control.BindingSource.DataSourceType);
		}
	}

	public void TestFieldsProperties()
	{
		using (var control = new SupportingDocumentsFieldsControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_CodeCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.CSI_CodeCodeFindBox.CodeBox.CharacterCasing);
				AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, control.CSI_ReferenceNumberTextBox.CharacterCasing);
				AssertEquals("CSI_ReferenceNumber2TextBox: CharacterCasing", CharacterCasing.Normal, control.CSI_ReferenceNumber2TextBox.CharacterCasing);
				AssertEquals("CSI_DateOfIssueDateEdit: DateTimeFormat", ZDateTimePickerFormat.Short, control.CSI_DateOfIssueDateEdit.DateTimeFormat);
			});
		}
	}

	public void TestFieldsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var control = new SupportingDocumentsFieldsControl())
		{
			control.JobDeclaration = declaration;

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			AssertEquals(nameof(control.CSI_CodeCodeFindBox), true, control.CSI_CodeCodeFindBox.Visible);
			AssertEquals(nameof(control.CSI_ReferenceNumberTextBox), true, control.CSI_ReferenceNumberTextBox.Visible);
			AssertEquals(nameof(control.CSI_ReferenceNumber2TextBox), true, control.CSI_ReferenceNumber2TextBox.Visible);
			AssertEquals(nameof(control.CSI_DateOfIssueDateEdit), true, control.CSI_DateOfIssueDateEdit.Visible);

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
			AssertEquals(nameof(control.CSI_CodeCodeFindBox), true, control.CSI_CodeCodeFindBox.Visible);
			AssertEquals(nameof(control.CSI_ReferenceNumberTextBox), true, control.CSI_ReferenceNumberTextBox.Visible);
			AssertEquals(nameof(control.CSI_ReferenceNumber2TextBox), false, control.CSI_ReferenceNumber2TextBox.Visible);
			AssertEquals(nameof(control.CSI_DateOfIssueDateEdit), false, control.CSI_DateOfIssueDateEdit.Visible);
		}
	}
}
