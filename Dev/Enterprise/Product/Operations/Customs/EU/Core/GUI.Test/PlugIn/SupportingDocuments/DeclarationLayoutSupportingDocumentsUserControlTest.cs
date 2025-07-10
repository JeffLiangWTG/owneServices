using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class DeclarationLayoutSupportingDocumentsUserControlTest : LayoutSupportingDocumentsUserControlAbstractTest<DeclarationLayoutSupportingDocumentsUserControl, JobDeclaration>
	{
		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndExportAvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_ItemNumber), typeof(ZCalcEditColumnStyle))
		};

		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedNonUCC6AvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Status), typeof(ZDropEditColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity2), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity2), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfIssue), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
		};

		public override void TestSupportingDocumentsFieldsControlBindingString()
		{
			using (var control = new DeclarationLayoutSupportingDocumentsUserControlForTest())
			{
				AssertEquals(".SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingStringExposed());
			}
		}

		class DeclarationLayoutSupportingDocumentsUserControlForTest : DeclarationLayoutSupportingDocumentsUserControl
		{
			public string GetSupportingDocumentsFieldsControlBindingStringExposed()
				=> base.GetSupportingDocumentsFieldsControlBindingString();
		}
	}
}
