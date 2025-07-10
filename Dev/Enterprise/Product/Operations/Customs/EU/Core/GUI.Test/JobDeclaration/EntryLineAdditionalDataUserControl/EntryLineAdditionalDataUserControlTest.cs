using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestDutyAndTaxDetailsUserControl()
		{
			using (var form = new Form())
			using (var userControl = new EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals(typeof(EntryLineTaxAndFeeUserControl), userControl.DutyAndTaxDetails.UserControlType);
			}
		}

		public void TestEntryLineSupportingDocumentsGrid_Type()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_Code, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_Description()
		{
			var columnName = ReadOnlySupportingDocument.Schema.CSI_CodeDescription;
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(columnName, 80);
			var columnStyle = entryLineAdditionalDataUserControl.EntryLineSupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
			AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		}

		public void TestEntryLineSupportingDocumentsGrid_Reference()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_ReferenceNumber, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_Availability()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_Status, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_Quantity()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_Quantity, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_UnitOfQuantity()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_UnitOfQuantity, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_2ndQuantity()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_Quantity2, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_2ndUnitOfQuantity()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_UnitOfQuantity2, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_Value()
		{
			AssertColumnStyle<ZCalcEditColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_Value, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_Currency()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_RX_NKCurrency, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_DateOfIssue()
		{
			AssertColumnStyle<ZDateEditColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_DateOfIssue, 80);
		}

		public void TestEntryLineSupportingDocumentsGrid_DateOfExpiry()
		{
			AssertColumnStyle<ZDateEditColumnStyleInfo>(ReadOnlySupportingDocument.Schema.CSI_DateOfExpiry, 80);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControlForTest();
		}
		EntryLineAdditionalDataUserControlForTest entryLineAdditionalDataUserControl;

		protected override void TearDown()
		{
			entryLineAdditionalDataUserControl?.Dispose();
			base.TearDown();
		}

		void AssertColumnStyle<T>(string columnName, int width) where T : ZGridColumnInfo
		{
			var columnStyle = entryLineAdditionalDataUserControl.EntryLineSupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}

		sealed class EntryLineAdditionalDataUserControlForTest : EntryLineAdditionalDataUserControl
		{
			protected override void OnAfterFirstBinding(EventArgs e)
			{
				base.OnAfterFirstBinding(e);
				SupportingDocumentsTabPage.TabVisible = true;
			}
		}
	}
}
