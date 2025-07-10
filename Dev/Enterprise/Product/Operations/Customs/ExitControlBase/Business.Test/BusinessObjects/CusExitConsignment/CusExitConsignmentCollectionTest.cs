using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentCollection<CusExitConsignment>))]
sealed class CusExitConsignmentCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitConsignmentCollection<CusExitConsignment>>
{
	protected override CusExitConsignmentCollection<CusExitConsignment> GetCollectionToTest()
	{
		var master = Factory.New<CusExitHeader>();
		return new CusExitConsignmentCollection<CusExitConsignment>(master);
	}
}
