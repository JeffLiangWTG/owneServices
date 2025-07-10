using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportInvoiceLineSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ExportInvoiceLineSupportingDocumentsFieldsControl), typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(ExportInvoiceLineSupportingDocumentsFieldsControl));
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(ExportInvoiceLineSupportingDocumentsFieldsControl), FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new ExportInvoiceLineSupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_FullType: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_FullTypeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
					AssertEquals("CSI_ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_ReferenceNumberCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_DescriptionTextBox").CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumber2TextBox").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_UnitOfQuantityDropEdit").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_UnitOfQuantity2DropEdit").CharacterCasing);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_AdditionalDescriptionTextBox").CharacterCasing);
					AssertEquals("CSI_ItemNumber: Decimals", 0, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ItemNumberCalcEdit").Decimals);
					AssertEquals("CSI_Quantity: Decimals", 4, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_QuantityCalcEdit").Decimals);
					AssertEquals("CSI_Value: Decimals", 2, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ValueCalcEdit").Decimals);
				});
			}
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_FullTypeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ReferenceNumberCodeFindBox", 1, typeof(ZCodeFindBox)),
			("CSI_DescriptionTextBox", 2, typeof(ZTextBox)),
			("CSI_ReferenceNumber2TextBox", 3, typeof(ZTextBox)),
			("CSI_QuantityCalcEdit", 4, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantityDropEdit", 5, typeof(ZDropEdit)),
			("CSI_UnitOfQuantity2DropEdit", 6, typeof(ZDropEdit)),
			("CSI_ValueCalcEdit", 7, typeof(ZCalcEdit)),
			("CSI_RX_NKCurrencyCodeFindBox", 8, typeof(ZCodeFindBox)),
			("CSI_DateOfIssueDateEdit", 9, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 10, typeof(ZDateEdit)),
			("CSI_AdditionalDescriptionTextBox", 11, typeof(ZTextBox)),
			("CSI_ItemNumberCalcEdit", 12, typeof(ZCalcEdit))
		};
	}
}
