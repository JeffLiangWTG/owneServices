using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IWineProduct
	{
		ZString WineProductCategory { get; set; }

		ZString WineGrowingZoneCode { get; set; }

		ZString ThirdCountryOfOrigin { get; set; }

		ZString OtherInformation { get; set; }

		IWineOperation WineOperation { get; set; }
	}
}
