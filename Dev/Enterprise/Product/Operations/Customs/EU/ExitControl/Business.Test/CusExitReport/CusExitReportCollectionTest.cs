using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ExitControlBase.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportCollection<CusExitReport>))]
	class CusExitReportCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitReportCollection<CusExitReport>>
	{
		protected override CusExitReportCollection<CusExitReport> GetCollectionToTest()
		{
			var master = Factory.New<CusExitHeader>();
			return new CusExitReportCollection<CusExitReport>(master);
		}
	}
}
