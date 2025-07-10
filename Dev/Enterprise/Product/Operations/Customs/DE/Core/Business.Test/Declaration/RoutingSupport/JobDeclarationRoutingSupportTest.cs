using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class JobDeclarationRoutingSupportTest : TestCaseWithFactory
	{
		public void TestTransportSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = ((Freight.Business.ITransportParent)declaration).TransportSupporter;
			AssertEquals(typeof(JobDeclarationTransportSupporter), supporter.GetType());
		}
	}
}
