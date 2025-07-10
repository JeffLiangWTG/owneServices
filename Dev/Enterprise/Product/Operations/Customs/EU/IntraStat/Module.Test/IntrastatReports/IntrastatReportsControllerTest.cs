using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Module.Testing
{
	[TestedType(typeof(IntrastatReportsController))]
	sealed class IntrastatReportsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.IntrastatReports;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatGroupWithValidData();
			Factory.Save();
			return report;
		}
	}
}
