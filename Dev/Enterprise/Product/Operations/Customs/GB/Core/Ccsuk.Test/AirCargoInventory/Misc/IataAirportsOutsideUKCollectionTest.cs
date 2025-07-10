using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(IataAirportsOutsideUKCollection))]
	class IataAirportsOutsideUKCollectionTest : ActiveBusinessObjectCollectionTestCase<IataAirportsOutsideUKCollection>
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_IATA = "DAN";
			loco.RL_Code = "USAT1";
			return loco;
		}
	}
}
