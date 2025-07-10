using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Module.Testing
{
	public class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestInitializeAdditionalColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var form = new ZForm())
			using (var filterStrip = new JobDeclarationFilterStripControl(module, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;

				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.AdminstrativeStatus]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.AdminstrativeStatusDescription]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.CargoStatus]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.CargoStatusDescription]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.ClearanceDateAsString]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.EntrySubmitDateAsString]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.EntryIssueDateAsString]);
				AssertNull(filteredGrid.Columns[JobDeclaration.Schema.JE_EntrySubmittedDate]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.RiskChannel]);
				AssertNotNull(filteredGrid.Columns[JobDeclaration.Schema.RiskChannelDescription]);
			}
		}
	}
}
