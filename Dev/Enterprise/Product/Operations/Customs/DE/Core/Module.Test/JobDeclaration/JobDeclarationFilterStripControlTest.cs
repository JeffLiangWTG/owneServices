using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Module.Testing
{
	sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestAdditionalColumns()
		{
			var declarations = new EU.Business.Declaration.JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.ZG_PresentationEndDate));
			}
		}

		public void TestSupportsExitControl()
		{
			var declarations = new EU.Business.Declaration.JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				Assert(userControl.SupportsExitControl);
			}
		}
	}
}
