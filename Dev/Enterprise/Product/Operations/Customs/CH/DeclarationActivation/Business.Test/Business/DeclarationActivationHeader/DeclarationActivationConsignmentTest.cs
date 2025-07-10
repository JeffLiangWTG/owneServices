using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationConsignment))]
sealed class DeclarationActivationConsignmentTest : EnterpriseBusinessObjectTestCase
{
	public void TestCXC_MovementReference()
	{
		AssertEquals("MaxLength", 21, DeclarationActivationConsignment.CXC_MovementReferenceInfo.MaxLength);
	}

	public void TestCXC_ReferenceNumber()
	{
		AssertEquals("MaxLength", 11, DeclarationActivationConsignment.CXC_ReferenceNumberInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateDeclarationActivationConsignment(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateDeclarationActivationConsignment(Factory);

	DeclarationActivationConsignment DeclarationActivationConsignment => declarationActivationConsignment ??= CreateDeclarationActivationConsignment(Factory);
	DeclarationActivationConsignment declarationActivationConsignment;

	DeclarationActivationConsignment CreateDeclarationActivationConsignment(BusinessObjectFactory factory)
	{
		return factory.New<DeclarationActivationHeader>().Consignment;
	}
}
