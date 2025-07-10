using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationRoutingSupportTest : TestCaseWithFactory
{
	public void TestITransportParentMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var transportParent = declaration as ITransportParent;
		AssertNotNull("IT.JobDeclaration as ITransportParent", transportParent);

		AssertType<DeclarationTransportCollection>("DeclarationTransportCollection", transportParent.Transports);
		AssertType<JobDeclarationTransportSupporter>("TransportSupporter", transportParent.TransportSupporter);
	}
}
