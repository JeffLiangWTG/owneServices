using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.Module.Testing
{
	sealed class EntryLineDetailsFor5ULFilterControlTest : ZFilterStripControlTest
	{
		public void Test5ULFilterControl()
		{
			var filterObject = new EntryLineDetailsFor5ULFilterStripBusinessObject();
			var gridCollection = new KREntryLineDetailsViewCollection(Factory);
			using (var form = new ZForm())
			using (var control = new EntryLineDetailsFor5ULFilterControl(gridCollection, filterObject))
			{
				form.Controls.Add(control);
				form.Show();

				var index = 0;
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_EntryNum);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_LineNumber);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_EntryNumIssueDate);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_OH_DutyPayer);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_AdValoremTariff);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_CustomsValue);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_Description);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_ValueForVAT);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryLineDetailsView.Schema.KEL_InvoiceLineCount);
			}
		}
	}
}
