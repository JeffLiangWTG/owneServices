using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Module.Testing
{
	public class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
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
