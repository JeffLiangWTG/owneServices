using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPackageCollection<CusExitConsignmentPackage>))]
sealed class CusExitConsignmentPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitConsignmentPackageCollection<CusExitConsignmentPackage>>
{
	protected override CusExitConsignmentPackageCollection<CusExitConsignmentPackage> GetCollectionToTest()
	{
		var master = Factory.New<CusExitHeader>();
		return new CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(master);
	}
}
