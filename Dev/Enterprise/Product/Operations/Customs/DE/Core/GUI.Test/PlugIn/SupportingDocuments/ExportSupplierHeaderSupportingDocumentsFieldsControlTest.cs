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
	class ExportSupplierHeaderSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(ExportSupplierHeaderSupportingDocumentsFieldsControl), typeof(JobDeclaration));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(typeof(ExportSupplierHeaderSupportingDocumentsFieldsControl));
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(typeof(ExportSupplierHeaderSupportingDocumentsFieldsControl), FieldsDetails);
		}

		public void TestFieldsProperties()
		{
			using (var control = new ExportSupplierHeaderSupportingDocumentsFieldsControl())
			{
				var groupBox = SupportingDocumentsControlTestHelper.GetFieldsGroupBox(control);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_FullType: CharacterCasing", CharacterCasing.Normal, groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_FullTypeCodeFindBox").CodeBox.CharacterCasing);
					var csi_ReferenceNumberTextBox = groupBox.FindSingleOrDefault<ZTextBox>("CSI_ReferenceNumberTextBox");
					AssertEquals("CSI_ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumberTextBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberTextBox: MaxLength", 35, csi_ReferenceNumberTextBox.MaxLength);
					var csi_ReferenceNumberCodeFindBox = groupBox.FindSingleOrDefault<ZCodeFindBox>("CSI_ReferenceNumberCodeFindBox");
					AssertEquals("CSI_ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, csi_ReferenceNumberCodeFindBox.CodeBox.CharacterCasing);
					AssertEquals("CSI_ReferenceNumberCodeFindBox: MaxLength", 35, csi_ReferenceNumberCodeFindBox.MaxLength);
					var csi_AdditionalDescription = groupBox.FindSingleOrDefault<ZTextBox>("CSI_AdditionalDescriptionTextBox");
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, csi_AdditionalDescription.CharacterCasing);
					AssertEquals("CSI_AdditionalDescription: MaxLength", 70, csi_AdditionalDescription.MaxLength);
					AssertEquals("CSI_AdditionalDescription: CaptionResourceString", "Issuing Authority", csi_AdditionalDescription.CaptionResourceString.Caption);
					var csi_ItemNumber = groupBox.FindSingleOrDefault<ZCalcEdit>("CSI_ItemNumberCalcEdit");
					AssertEquals("CSI_ItemNumber: MaxLength", 5, csi_ItemNumber.MaxLength);
					AssertEquals("CSI_ItemNumber: Decimals", 0, csi_ItemNumber.Decimals);
					AssertEquals("CSI_ItemNumber: CaptionResourceString", "Document Line Item Number", csi_ItemNumber.CaptionResourceString.Caption);
				});
			}
		}

		IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_FullTypeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ReferenceNumberCodeFindBox", 1, typeof(ZCodeFindBox)),
			("CSI_DateOfIssueDateEdit", 2, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 3, typeof(ZDateEdit)),
			("CSI_AdditionalDescriptionTextBox", 4, typeof(ZTextBox)),
			("CSI_ItemNumberCalcEdit", 5, typeof(ZCalcEdit)),
		};
	}
}
