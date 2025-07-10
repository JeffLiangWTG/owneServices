using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.CA.DIF.GUI.Testing
{
	sealed class DISPreFormActionRegistrarTest : TestCaseWithFactory
	{
		public void TestGetDISPreFormActionRunner()
		{
			var declaration = (IDISHost)Factory.New<Integration.Customs.CA.IJobDeclaration>();
			var runner = new DISPreFormActionRegistrar().GetDISPreFormActionRunner(declaration);
			AssertType<JobDeclarationDISPreFormActionRunner>(runner);
		}
	}
}
