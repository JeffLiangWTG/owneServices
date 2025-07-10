using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterStripControl))]
	sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestAdditionalColumnsExist()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_LocationOfGoods));
				AssertNotNull(grid.GetColumnStyle("SubLocation"));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.ZG_ImportClearanceStatusICS));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.ZG_ImportClearanceStatusICSDescription));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_GBRouteOfEntry));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_GBRouteOfEntryDescription));
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.JE_MasterUCR));
			}
		}
	}
}
