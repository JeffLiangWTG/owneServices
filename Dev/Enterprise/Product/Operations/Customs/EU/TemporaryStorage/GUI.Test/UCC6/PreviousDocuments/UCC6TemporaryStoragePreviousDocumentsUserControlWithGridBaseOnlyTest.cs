using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStoragePreviousDocumentsUserControlWithGridBaseOnlyTest : UCC6TemporaryStoragePreviousDocumentsUserControlWithGridAbstractTest<UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid, TemporaryStorageHeader>
	{
		public override void TestPreviousDocumentsFieldsControlBinding()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsUserControlWithGridForTest())
			{
				AssertEquals("BindingMember", "Bills.PreviousDocuments", control.GetPreviousDocumentsFieldsControlBindingStringExposed());
			}
		}

		[RequiresSTA]
		public void TestLineNoColumnStyleMaxValue()
		{
			using (var form = new ZForm(TemporaryStorage))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();

				var lineNoColumn = control.PreviousDocumentsGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_LineNo");
				AssertEquals("MaxValue", 99999m, lineNoColumn.MaxValue);
			}
		}

		public void TestLineNoVisibility_Transfer()
		{
			using (var form = new ZForm(TemporaryStorage))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();
				TemporaryStorage.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;

				var lineNoColumn = control.PreviousDocumentsGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_LineNo");
				AssertEquals("Line No visible-property", true, lineNoColumn.IsUnavailable);
			}
		}

		public void TestLineNoVisibility_Deconsolidation()
		{
			using (var form = new ZForm(TemporaryStorage))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();
				TemporaryStorage.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;

				var lineNoColumn = control.PreviousDocumentsGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_LineNo");
				AssertEquals("Line No visible-property", true, lineNoColumn.IsUnavailable);
			}
		}

		public void TestColumnsWidth()
		{
			using (var form = new ZForm(TemporaryStorage))
			using (var control = CreateControl())
			{
				form.Controls.Add(control);
				form.Show();

				var columnStyles = control.PreviousDocumentsGrid.ColumnStyles;
				var codeColumn = columnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().Single(x => x.ColumnName == "CSI_Code");
				AssertEquals("CSI_Code", 40, codeColumn.Width);
				var referenceNumberColumn = columnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(x => x.ColumnName == "CSI_ReferenceNumber");
				AssertEquals("CSI_ReferenceNumber", 100, referenceNumberColumn.Width);
				var lineNoColumn = columnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == "CSI_LineNo");
				AssertEquals("CSI_LineNo", 110, lineNoColumn.Width);
			}
		}

		protected override IEnumerable<(string, Type)> GetOrderedGridColumns()
		{
			return new[]
			{
				(TemporaryStoragePreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
				(TemporaryStoragePreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyle)),
				(TemporaryStoragePreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			};
		}
	}

	sealed class UCC6TemporaryStoragePreviousDocumentsUserControlWithGridForTest : UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid
	{
		public string GetPreviousDocumentsFieldsControlBindingStringExposed() => UCC6TemporaryStorageBillPreviousDocumentsBingdingMemberName;
	}
}
