using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using PreviousDocument = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class PreviousDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(PreviousDocument), control.BindingSource.DataSourceType);
		}

		public void TestTypeFindBox()
		{
			var typeCodeFindBox = control.TypeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", typeCodeFindBox);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_Code), typeCodeFindBox.BindTo);
			});
		}

		public void TestReferenceNumberTextBox()
		{
			var referenceNumberTextBox = control.ReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceNumberTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, referenceNumberTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_ReferenceNumber), referenceNumberTextBox.BindTo);
			});
		}

		public void TestItemNumberCalcEdit()
		{
			var itemNumberControl = control.ItemNumberCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", itemNumberControl);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_ItemNumber), itemNumberControl.BindTo);
			});
		}

		public void TestNumberOfPackagesControl()
		{
			var numOfPackagesControl = control.NumOfPackagesDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", numOfPackagesControl);
				AssertEquals("BindToAmount", nameof(PreviousDocument.CSI_Quantity2), numOfPackagesControl.BindToAmount);
				AssertEquals("BindToUnit", nameof(PreviousDocument.CSI_UnitOfQuantity2), numOfPackagesControl.BindToUnit);
			});
		}

		public void TestQuantityControl()
		{
			var quantityControl = control.QuantityDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", quantityControl);
				AssertEquals("BindToAmount", nameof(PreviousDocument.CSI_Quantity), quantityControl.BindToAmount);
				AssertEquals("BindToUnit", nameof(PreviousDocument.CSI_UnitOfQuantity), quantityControl.BindToUnit);
			});
		}
		public void TestComplementTextBox()
		{
			var complementTextBox = control.ComplementTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", complementTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, complementTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_ReferenceNumber2), complementTextBox.BindTo);
			});
		}

		public void TestStatusTextBox()
		{
			var statusTextBox = control.StatusTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", statusTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, statusTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_Status), statusTextBox.BindTo);
			});
		}

		public void TestLineNoCalcEdit()
		{
			var lineNumberCalcEdit = control.LineNoCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", lineNumberCalcEdit);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_LineNo), lineNumberCalcEdit.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new PreviousDocumentUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		PreviousDocumentUserControl control;
	}
}
