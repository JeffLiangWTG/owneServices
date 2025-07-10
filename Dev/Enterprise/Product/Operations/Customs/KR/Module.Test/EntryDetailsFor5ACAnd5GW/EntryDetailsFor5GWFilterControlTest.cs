using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.Module.Testing
{
	sealed class EntryDetailsFor5GWFilterControlTest : ZFilterStripControlTest
	{
		public void Test5GWFilterControl()
		{
			var filterObject = new EntryDetailsFor5GWFilterStripBusinessObject();
			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			var gridCollection = new KREntryHeaderDetailsViewCollection(Factory, KRJobMessageTypeList.Codes.Export, query, GlbCompany.CurrentCompany.PK);
			using (var form = new ZForm())
			using (var control = new EntryDetailsFor5GWFilterControl(gridCollection, filterObject))
			{
				form.Controls.Add(control);
				form.Show();

				var index = 0;
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryHeaderDetailsView.FormattedEntryNum));
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_BillNum);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_CargoManagementNumber);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_OH_Payer);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_PayerName);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_HSDescription);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_TotalCustomsValueInKRW);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_ImportPackQty);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_TotalWeightInKG);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_BondedAreaCode);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_EntryCreatedLocalTime);
			}
		}
	}
}
