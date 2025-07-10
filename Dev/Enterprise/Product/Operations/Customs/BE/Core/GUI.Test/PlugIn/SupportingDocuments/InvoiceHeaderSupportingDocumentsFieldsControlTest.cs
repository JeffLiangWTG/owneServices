using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

class InvoiceHeaderSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(InvoiceHeaderSupportingDocumentsFieldsControl), typeof(JobDeclaration));
	}

	public void TestGroupBox()
	{
		SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(InvoiceHeaderSupportingDocumentsFieldsControl), expectedCaption: "Supporting Documents");
	}

	public void TestFields()
	{
		SupportingDocumentsControlTestHelper.AssertFields(typeof(InvoiceHeaderSupportingDocumentsFieldsControl), FieldsDetails);
	}

	public void TestFieldsProperties()
	{
		using (var control = new InvoiceHeaderSupportingDocumentsFieldsControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, control.FindSingle<ZCodeFindBox>("CSI_CodeCodeFindBox").CodeBox.CharacterCasing);
				AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, control.FindSingle<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
				AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, control.FindSingle<ZTextBox>("CSI_AdditionalDescriptionTextBox").CharacterCasing);
				AssertEquals("CSI_ItemNumber: Decimals", 0, control.FindSingle<ZCalcEdit>("CSI_ItemNumberCalcEdit").Decimals);
			});
		}
	}

	static IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new[]
	{
		("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
		("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
		("CSI_DateOfExpiryDateEdit", 3, typeof(ZDateEdit)),
		("CSI_AdditionalDescriptionTextBox", 4, typeof(ZTextBox)),
		("CSI_ItemNumberCalcEdit", 5, typeof(ZCalcEdit))
	};
}
