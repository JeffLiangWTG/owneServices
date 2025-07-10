using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class JobDeclarationTransportParentTest : TestCaseWithFactory
	{
		public void TestTransportSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationTransportSupporter>(((Freight.Business.ITransportParent)declaration).TransportSupporter);
		}
	}
}
