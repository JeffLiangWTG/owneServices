using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Module.Testing
{
	public class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestIEAdditionalColumnsExist()
		{
			var declarations = new EU.Business.Declaration.JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				AssertNotNull("CustomsDocStatus column", grid.GetColumnStyle(JobDeclaration.Schema.CustomsDocStatus));
				AssertNotNull("CustomsDocStatusDesc column", grid.GetColumnStyle(JobDeclaration.Schema.CustomsDocStatusDesc));
				AssertNotNull("DeclarationType column", grid.GetColumnStyle(JobDeclaration.Schema.DeclarationType));
				Assert("JE_EntryAuthorisationDate", grid.GetColumnStyle("JE_EntryAuthorisationDate").IsUnavailable);
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
