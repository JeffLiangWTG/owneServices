using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStoragePreviousDocumentsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestTypeCodeFindBox()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var typeCodeFindBox = control.TypeCodeFindBox;
				AssertNotNull("TypeCodeFindBox", typeCodeFindBox);
				AssertEquals("TypeCodeFindBox BindTo", "CSI_Code", typeCodeFindBox.BindTo);
			}
		}

		public void TestReferenceNumberTextBox()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var referenceNumberTextBox = control.ReferenceNumberTextBox;
				AssertNotNull("ReferenceNumberTextBox", referenceNumberTextBox);
				AssertEquals("ReferenceNumberTextBox BindTo", "CSI_ReferenceNumber", referenceNumberTextBox.BindTo);
			}
		}

		public void TestGoodItemIdentifierCalcEdit()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var goodItemIdentifierCalcEdit = control.GoodItemIdentifierCalcEdit;
				AssertNotNull("GoodItemIdentifierCalcEdit", goodItemIdentifierCalcEdit);
				AssertEquals("GoodItemIdentifierCalcEdit BindTo", "CSI_LineNo", goodItemIdentifierCalcEdit.BindTo);
				AssertEquals("GoodItemIdentifierCalcEdit Decimals", 2, goodItemIdentifierCalcEdit.Decimals);
				AssertEquals("GoodItemIdentifierCalcEdit MaxValue", 99999m, goodItemIdentifierCalcEdit.MaxValue);
			}
		}

		public void TestPackageCalcDropEdit()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var packageCalcDropEdit = control.PackageCalcDropEdit;
				AssertNotNull("PackageCalcDropEdit", packageCalcDropEdit);
				AssertEquals("PackageCalcDropEdit BindToAmount", "CSI_PackQty", packageCalcDropEdit.BindToAmount);
				AssertEquals("PackageCalcDropEdit BindToUnit", "CSI_PackType", packageCalcDropEdit.BindToUnit);
				AssertEquals("PackageCalcDropEdit MaxValue", 99999999m, packageCalcDropEdit.MaxValue);
			}
		}

		public void TestQuantityCalcDropEdit()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl())
			{
				var quantityCalcDropEdit = control.QuantityCalcDropEdit;
				AssertNotNull("QuantityCalcDropEdit", quantityCalcDropEdit);
				AssertEquals("QuantityCalcDropEdit BindToAmount", "CSI_Quantity", quantityCalcDropEdit.BindToAmount);
				AssertEquals("QuantityCalcDropEdit BindToUnit", "CSI_UnitOfQuantity", quantityCalcDropEdit.BindToUnit);
				AssertEquals("QuantityCalcDropEdit MaxValue", 9999999999m, quantityCalcDropEdit.MaxValue);
			}
		}
	}
}
