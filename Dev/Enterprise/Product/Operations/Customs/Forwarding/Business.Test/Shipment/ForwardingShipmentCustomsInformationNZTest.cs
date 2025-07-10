using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Constants.CountryCodes.NewZealand)]
	class ForwardingModuleShipmentNZSpecificTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCustomsStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var mawb = (BusinessObject)Factory.New<Integration.Customs.NZ.ICusMAWB>();
			mawb[CusMAWBSchema.CM_CustomsStatus] = "ERR";
			var hawb = (BusinessObject)Factory.New<Integration.Customs.NZ.ICusHAWB>();
			hawb[CusHAWBSchema.CS_CM] = mawb.PK;
			hawb[CusHAWBSchema.CS_CustomsStatus] = "WOF";
			hawb[CusHAWBSchema.CS_JS] = shipment.PK;
			Factory.Save();

			var customsInformation = new ForwardingShipmentCustomsInformation(shipment);

			NUnit.Framework.Assert.That(customsInformation.CustomsMessageStatus, Is.EqualTo("ICR/CRE in Error, Check Consignments for Status").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(customsInformation.CustomsCargoStatus, Is.EqualTo("Consignment Written Off/Cleared").Using(CustomComparers.TypeComparison));
		}
	}
}
