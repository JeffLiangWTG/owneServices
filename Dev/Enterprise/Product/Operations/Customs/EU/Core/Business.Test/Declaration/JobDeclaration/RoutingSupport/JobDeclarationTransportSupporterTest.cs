using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobDeclarationTransportSupporterTest : TestCaseWithFactory
	{
		public void TestTransportSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = ((ITransportParent)declaration).TransportSupporter;
			AssertEquals(typeof(JobDeclarationTransportSupporter), supporter.GetType());
		}
	}
}
