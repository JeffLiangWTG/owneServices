using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class MawbAndNewOrFoundFlag
	{
		public MawbAndNewOrFoundFlag(CusMAWB mawb, bool createdNewRatherThanFoundExisting)
		{
			IsCreatedNewRatherThanFoundExisting = createdNewRatherThanFoundExisting;
			Mawb = mawb;
		}
		public readonly CusMAWB Mawb;
		public readonly bool IsCreatedNewRatherThanFoundExisting;
	}
}
