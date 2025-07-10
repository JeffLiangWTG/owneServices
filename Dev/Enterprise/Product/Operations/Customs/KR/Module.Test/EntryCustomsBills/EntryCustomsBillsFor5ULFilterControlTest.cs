using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryCustomsBillsFor5ULFilterControl))]
	sealed class EntryCustomsBillsFor5ULFilterControlTest : ZFilterStripControlTest
	{
		public void Test5ULFilterControl()
		{
			var filterObject = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			var query = new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL();
			var gridCollection = new KREntryCustomsBillsViewCollection(Factory, query, GlbCompany.CurrentCompany.PK);
			using (var form = new ZForm())
			using (var control = new EntryCustomsBillsFor5ULFilterControl(gridCollection, filterObject))
			{
				form.Controls.Add(control);
				form.Show();

				var index = 0;
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_ImportEntryNum));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_ImportIssueDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_PaymentAuthorizationDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_OH_Importer));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_BranchPK));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_EntryReleaseDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_CustomsFeesTotal));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_PrintDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_ProcessDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_DueDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_CustomsDisbursementBillNumber));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_Duty));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_ValueAddedTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_LiquorTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_AgricultureTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_SpecialConsumptionTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_TransportationTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_EducationTax));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_PenaltyAndInterest));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_LatePenalty));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(KREntryCustomsBillsView.KEB_ValueForVAT));
			}
		}
	}
}
