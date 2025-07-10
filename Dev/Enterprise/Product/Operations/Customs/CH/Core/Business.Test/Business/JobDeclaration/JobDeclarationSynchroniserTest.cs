using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobDeclarationSynchroniser))]
sealed class JobDeclarationSynchroniserTest : TestCaseWithFactory
{
	public void TestGetPackingSynchroniser()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		var synchroniser = new JobDeclarationSynchroniserForTest(declaration);
		AssertType<PackingSynchroniser>("GetPackingSynchroniser", synchroniser.GetPackingSynchroniserExposed());
	}
}

class JobDeclarationSynchroniserForTest : JobDeclarationSynchroniser
{
	public JobDeclarationSynchroniserForTest(JobDeclaration destination) : base(destination)
	{
	}

	public Customs.Business.PackingSynchroniser GetPackingSynchroniserExposed() => GetPackingSynchroniser();
}
