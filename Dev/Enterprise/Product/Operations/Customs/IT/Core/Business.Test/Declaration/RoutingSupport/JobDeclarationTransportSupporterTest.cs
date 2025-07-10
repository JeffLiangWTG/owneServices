using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationTransportSupporterTest : TestCaseWithFactory
{
	public void TestTransportValidatorType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationTransportSupporterTest = new JobDeclarationTransportSupporter(declaration);

		AssertType<JobDeclarationTransportValidation>("TransportValidatorType", declarationTransportSupporterTest.GetNewTransportValidator(Factory.New<Transport>()));
	}
}
