using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class PreviousDocumentsFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestCodeDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var codeDropEdit = control.CodeDropEdit;
				AssertNotNull("CodeDropEdit", codeDropEdit);
				AssertEquals("CodeDropEdit BindTo", nameof(PreviousDocument.CSI_Code), codeDropEdit.BindTo);
			}
		}

		public void TestReferenceTextBox()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var referenceTextBox = control.ReferenceTextBox;
				AssertNotNull("ReferenceTextBox", referenceTextBox);
				AssertEquals("ReferenceTextBox BindTo", nameof(PreviousDocument.CSI_ReferenceNumber), referenceTextBox.BindTo);
			}
		}

		public void TestProcedureDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var procedureDropEdit = control.ProcedureDropEdit;
				AssertNotNull("ProcedureDropEdit", procedureDropEdit);
				AssertEquals("ProcedureDropEdit BindTo", nameof(PreviousDocument.CSI_Procedure), procedureDropEdit.BindTo);
			}
		}

		public void TestReference2TextBox()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var reference2TextBox = control.Reference2TextBox;
				AssertNotNull("Reference2TextBox", reference2TextBox);
				AssertEquals("Reference2TextBox BindTo", nameof(PreviousDocument.CSI_ReferenceNumber2), reference2TextBox.BindTo);
			}
		}

		public void TestIssueDateEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var issueDateEdit = control.IssueDateEdit;
				AssertNotNull("IssueDateEdit", issueDateEdit);
				AssertEquals("IssueDateEdit BindTo", nameof(PreviousDocument.CSI_DateOfIssue), issueDateEdit.BindTo);
			}
		}

		public void TestLineNoCalcEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var lineNoCalcEdit = control.LineNoCalcEdit;
				AssertNotNull("LineNoCalcEdit", lineNoCalcEdit);
				AssertEquals("LineNoCalcEdit BindTo", nameof(PreviousDocument.CSI_LineNo), lineNoCalcEdit.BindTo);
				AssertEquals("LineNoCalcEdit Decimals", 0, lineNoCalcEdit.Decimals);
				AssertEquals("LineNoCalcEdit MaxValue", 99999m, lineNoCalcEdit.MaxValue);
			}
		}

		public void TestCustomsOfficeCodeFindBox()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var customsOfficeCodeFindBox = control.CustomsOfficeCodeFindBox;
				AssertNotNull("CustomsOfficeCodeFindBox", customsOfficeCodeFindBox);
				AssertEquals("CustomsOfficeCodeFindBox BindTo", nameof(PreviousDocument.CSI_CustomsOffice), customsOfficeCodeFindBox.BindTo);
			}
		}

		public void TestQuantityCalcDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var quantityCalcDropEdit = control.QuantityCalcDropEdit;
				AssertNotNull("QuantityCalcDropEdit", quantityCalcDropEdit);
				AssertEquals("QuantityCalcDropEdit BindToAmount", nameof(PreviousDocument.CSI_Quantity), quantityCalcDropEdit.BindToAmount);
				AssertEquals("QuantityCalcDropEdit BindToUnit", nameof(PreviousDocument.CSI_UnitOfQuantity), quantityCalcDropEdit.BindToUnit);
			}
		}

		public void TestQuantity3CalcDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var quantity3CalcDropEdit = control.Quantity3CalcDropEdit;
				AssertNotNull("Quantity3CalcDropEdit", quantity3CalcDropEdit);
				AssertEquals("Quantity3CalcDropEdit BindToAmount", nameof(PreviousDocument.CSI_Quantity3), quantity3CalcDropEdit.BindToAmount);
				AssertEquals("Quantity3CalcDropEdit BindToUnit", nameof(PreviousDocument.CSI_UnitOfQuantity3), quantity3CalcDropEdit.BindToUnit);
			}
		}

		public void TestPackageQuantityCalcDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var packageQuantityCalcDropEdit = control.PackageQuantityCalcDropEdit;
				AssertNotNull("PackageQuantityCalcDropEdit", packageQuantityCalcDropEdit);
				AssertEquals("PackageQuantityCalcDropEdit BindToAmount", nameof(PreviousDocument.CSI_PackQty), packageQuantityCalcDropEdit.BindToAmount);
				AssertEquals("PackageQuantityCalcDropEdit BindToUnit", nameof(PreviousDocument.CSI_PackType), packageQuantityCalcDropEdit.BindToUnit);
				AssertEquals("PackageQuantityCalcDropEdit MaxValue", 99999999m, packageQuantityCalcDropEdit.MaxValue);
			}
		}

		public void TestItemNumberCalcEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var itemNumberCalcEdit = control.ItemNumberCalcEdit;
				AssertNotNull("ItemNumberCalcEdit", itemNumberCalcEdit);
				AssertEquals("ItemNumberCalcEdit BindTo", nameof(PreviousDocument.CSI_ItemNumber), itemNumberCalcEdit.BindTo);
				AssertEquals("ItemNumberCalcEdit Decimals", 0, itemNumberCalcEdit.Decimals);
				AssertEquals("ItemNumberCalcEdit MaxValue", 99999m, itemNumberCalcEdit.MaxValue);
			}
		}

		public void TestSubTypeDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var subTypeDropEdit = control.SubTypeDropEdit;
				AssertNotNull("SubTypeDropEdit", subTypeDropEdit);
				AssertEquals("ItemNumberCalcEdit BindTo", nameof(PreviousDocument.CSI_SubType), subTypeDropEdit.BindTo);
			}
		}
	}
}
