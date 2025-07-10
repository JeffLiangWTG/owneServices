using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class SupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(SupportingDocumentsFieldsControlType, typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(SupportingDocumentsFieldsControlType, false);
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(SupportingDocumentsFieldsControlType, FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (SupportingDocumentsFieldsControl control = (SupportingDocumentsFieldsControl)Activator.CreateInstance(SupportingDocumentsFieldsControlType))
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("SupDocTypeCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("SupDocTypeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_StatusDropEdit").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantityTextBox").CharacterCasing);
					AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_RX_NKCurrencyCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("SupDocReferenceTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("SupDocReferenceTextBox").CharacterCasing);
					AssertEquals("SupDocReferenceCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("SupDocReferenceCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_UnitOfQuantityTextBox").CharacterCasing);
					AssertEquals("SupDocQuantityCalcEdit: Decimals", 5, groupBox.FindSingleOrDefault<ZCalcEdit>("SupDocQuantityCalcEdit").Decimals);
					AssertEquals("CSI_Quantity2: Decimals", 5, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_Quantity2CalcEdit").Decimals);
					AssertEquals("CSI_Value: Decimals", 5, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ValueCalcEdit").Decimals);
				});
			}
		}

		protected virtual Type SupportingDocumentsFieldsControlType => typeof(SupportingDocumentsFieldsControl);

		protected virtual IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("SupDocTypeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("SupDocReferenceTextBox", 1, typeof(ZTextBox)),
			("SupDocReferenceCodeFindBox", 1, typeof(ZCodeFindBox)),
			("CSI_StatusDropEdit", 2, typeof(ZDropEdit)),
			("SupDocQuantityCalcEdit", 3, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantityTextBox", 4, typeof(ZTextBox)),
			("CSI_Quantity2CalcEdit", 5, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantity2TextBox", 6, typeof(ZTextBox)),
			("CSI_ValueCalcEdit", 7, typeof(ZCalcEdit)),
			("CSI_RX_NKCurrencyCodeFindBox", 8, typeof(ZCodeFindBox)),
			("CSI_DateOfIssueDateEdit", 9, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 10, typeof(ZDateEdit))
		};
	}
}
