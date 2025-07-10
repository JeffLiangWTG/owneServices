using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(AcceptabilityBandController))]
	class AcceptabilityBandControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AcceptabilityBand;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var acceptabilityBand = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			Factory.Save();

			return acceptabilityBand;
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
