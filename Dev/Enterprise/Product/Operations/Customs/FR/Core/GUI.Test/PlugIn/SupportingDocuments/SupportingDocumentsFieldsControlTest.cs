using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	class SupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNoDuplicateDataBindings()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);

			using (var supportingDocumentsUserControl = new SupportingDocumentsUserControl())
			{
				var control = supportingDocumentsUserControl.SupportingDocumentsFieldsControl;
				control.SetDataBinding(collection, "");
				control.SetDataBinding(null, "");
				control.SetDataBinding(collection, "");
			}
		}

		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(SupportingDocumentsFieldsControl), typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(SupportingDocumentsFieldsControl));
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(SupportingDocumentsFieldsControl), FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new SupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_CodeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
					AssertEquals("CSI_ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_ReferenceNumberCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_UnitOfQuantityTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantityTextBox").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantityDropEdit: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_UnitOfQuantityDropEdit").CharacterCasing);
					AssertEquals("CSI_LineNoCalcEdit: DecimalPlaces", 0, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_LineNoCalcEdit").DecimalPlaces);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantity2TextBox").CharacterCasing);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_AdditionalDescriptionTextBox").CharacterCasing);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_RX_NKCurrencyCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Value: Decimals", 4, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ValueCalcEdit").Decimals);
					var csi_Description = groupBox.FindSingleOrDefault<ZTextBox>("CSI_DescriptionTextBox");
					Assert("CSI_Description: Multiline", csi_Description.Multiline);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, csi_Description.CharacterCasing);
					AssertEquals("CSI_ItemNumberCalcEdit: DecimalPlaces", 0, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ItemNumberCalcEdit").Decimals);
				});
			}
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ReferenceNumberCodeFindBox", 1, typeof(ZCodeFindBox)),
			("CSI_DescriptionTextBox", 2, typeof(ZTextBox)),
			("CSI_QuantityCalcEdit", 3, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantityTextBox", 4, typeof(ZTextBox)),
			("CSI_UnitOfQuantityDropEdit", 4, typeof(ZDropEdit)),
			("CSI_LineNoCalcEdit", 13, typeof(ZCalcEdit)),
			("CSI_Quantity2CalcEdit", 6, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantity2TextBox", 7, typeof(ZTextBox)),
			("CSI_AdditionalDescriptionTextBox", 14, typeof(ZTextBox)),
			("CSI_ValueCalcEdit", 9, typeof(ZCalcEdit)),
			("CSI_RX_NKCurrencyCodeFindBox", 10, typeof(ZCodeFindBox)),
			("CSI_DateOfIssueDateEdit", 11, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 12, typeof(ZDateEdit)),
			("CSI_ItemNumberCalcEdit", 15, typeof(ZCalcEdit))
		};
	}
}
