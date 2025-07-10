using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	sealed class JobDeclarationShipmentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.JobDeclarationPluggedIntoShipment;

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_JS = shipment.PK;
			Factory.Save();
			return testDec;
		}
	}
}
