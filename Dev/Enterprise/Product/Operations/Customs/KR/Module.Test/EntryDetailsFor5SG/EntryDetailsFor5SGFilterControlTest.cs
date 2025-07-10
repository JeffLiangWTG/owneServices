using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.Module.Testing
{
	sealed class EntryDetailsFor5SGFilterControlTest : ZFilterStripControlTest
	{
		public void Test5SGFilterControl()
		{
			var filterObject = new EntryDetailsFor5SGFilterStripBusinessObject();
			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var gridCollection = new KREntryHeaderDetailsViewCollection(Factory, KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			using (var form = new ZForm())
			using (var control = new EntryDetailsFor5SGFilterControl(gridCollection, filterObject))
			{
				form.Controls.Add(control);
				form.Show();

				var index = 0;
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryHeaderDetailsView.FormattedEntryNum));
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_EntryReleaseDate);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_OH_Payer);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_PayerName);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_TotalCustomsValueInKRW);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_TotalPaid);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_EstimatedDateOfFinalPrice);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_ContractExpirationDate);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_ProvAdditionalRate);
				AssertEquals(control.Grid.Columns[index++].ColumnName, KREntryHeaderDetailsView.Schema.KEH_TotalProvAdditionalAmount);
			}
		}
	}
}
