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
	class ImportInvoiceLineSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ImportInvoiceLineSupportingDocumentsFieldsControl), typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(ImportInvoiceLineSupportingDocumentsFieldsControl));
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(ImportInvoiceLineSupportingDocumentsFieldsControl), FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new ImportInvoiceLineSupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_CodeCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox").CharacterCasing);
					AssertEquals("CSI_ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_ReferenceNumberCodeFindBox").CodeBox.CharacterCasing);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_StatusDropEdit").CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, groupBox.FindSingleOrDefault<ZDropEdit>("CSI_UnitOfQuantityDropEdit").CharacterCasing);
					AssertEquals("CSI_Quantity: Decimals", 3, groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_QuantityCalcEdit").Decimals);
				});
			}
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ReferenceNumberCodeFindBox", 1, typeof(ZCodeFindBox)),
			("CSI_StatusDropEdit", 2, typeof(ZDropEdit)),
			("CSI_QuantityCalcEdit", 3, typeof(ZCalcEdit)),
			("CSI_UnitOfQuantityDropEdit", 4, typeof(ZDropEdit)),
			("CSI_DateOfIssueDateEdit", 5, typeof(ZDateEdit))
		};
	}
}

