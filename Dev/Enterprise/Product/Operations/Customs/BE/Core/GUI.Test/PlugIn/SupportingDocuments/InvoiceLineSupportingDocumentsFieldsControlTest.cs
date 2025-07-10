using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

class InvoiceLineSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestGroupBox()
	{
		SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(InvoiceLineSupportingDocumentsFieldsControl), expectedCaption: "[UCC 2/3] Supporting documents");
	}

	public void TestFields()
	{
		SupportingDocumentsControlTestHelper.AssertFields(typeof(InvoiceLineSupportingDocumentsFieldsControl), FieldsDetails);
	}

	public void TestFieldsProperties()
	{
		using (var control = new InvoiceLineSupportingDocumentsFieldsControl())
		{
			var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Value: Caption", "Amount", groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ValueCalcEdit")?.CaptionResourceString.Caption);
				AssertEquals("CSI_DateOfExpiry: Caption", "Validity Date", groupBox.FindSingleOrDefault<ZDateEdit>("CSI_DateOfExpiryDateEdit")?.CaptionResourceString.Caption);
				AssertEquals("CSI_AdditionalDescription: Caption", "Issuing Authority", groupBox.FindSingleOrDefault<ZTextBox>("SupDocAdditionalDescriptionTextBox")?.CaptionResourceString.Caption);
				AssertEquals("CSI_UnitOfQuantity: Caption", "Unit of Measure", groupBox.FindSingleOrDefault<ZDropEditWithFixedWidth>("SupDocUnitOfQuantityDropEdit")?.CaptionResourceString.Caption);
			});
		}
	}

	IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
	{
		("SupDocTypeCodeFindBox", 0, typeof(ZCodeFindBox)),
		("SupDocReferenceTextBox", 1, typeof(ZTextBox)),
		("SupDocQuantityCalcEdit", 2, typeof(ZCalcEdit)),
		("SupDocUnitOfQuantityDropEdit", 3, typeof(ZDropEditWithFixedWidth)),
		("CSI_ValueCalcEdit", 4, typeof(ZCalcEdit)),
		("CSI_RX_NKCurrencyCodeFindBox", 5, typeof(ZCodeFindBox)),
		("CSI_DateOfExpiryDateEdit", 6, typeof(ZDateEdit)),
		("SupDocAdditionalDescriptionTextBox", 7, typeof(ZTextBox)),
		("SupDocLineNumberCalcEdit", 8, typeof(ZCalcEdit)),
	};
}
