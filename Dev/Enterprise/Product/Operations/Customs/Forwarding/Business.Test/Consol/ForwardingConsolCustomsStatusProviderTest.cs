using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.UnitedKingdom)]
	class ForwardingConsolCustomsStatusProviderTest : TestCaseWithFactory
	{
		public void TestGetsTheRightForwardingConsolCustomsStatusProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			var statusProvider = ForwardingConsolCustomsStatusProvider.New(consol);
			NUnit.Framework.Assert.That(statusProvider, Is.Not.EqualTo(default(ForwardingConsolCustomsStatusProvider)), "ForwardingConsolCustomsStatusProvider.New(consol) - should not be [null]");
			NUnit.Framework.Assert.That(statusProvider.GetType(), Is.EqualTo(ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICcsukForwardingConsolCustomsStatusProvider>()), "ForwardingConsolCustomsStatusProvider.New(consol).GetType()");
			AssertNoExceptionThrown(() => statusProvider.CustomsCargoStatus());
		}
	}
}
