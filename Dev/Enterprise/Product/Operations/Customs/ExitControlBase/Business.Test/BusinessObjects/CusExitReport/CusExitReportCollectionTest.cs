using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitReportCollection<CusExitReport>))]
sealed class CusExitReportCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitReportCollection<CusExitReport>>
{
	protected override CusExitReportCollection<CusExitReport> GetCollectionToTest()
	{
		var master = Factory.New<CusExitHeader>();
		return new CusExitReportCollection<CusExitReport>(master);
	}
}
