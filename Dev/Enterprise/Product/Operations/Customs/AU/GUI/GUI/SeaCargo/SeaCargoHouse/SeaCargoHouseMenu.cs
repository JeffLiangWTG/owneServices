using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoHouseMenu : SeaCargoMenu
	{
		public SeaCargoHouseMenu(CusSCAHouseMessageManager messageManager) : base(messageManager)
		{
			houseBill = messageManager.HouseBill;
		}

		readonly CusSCAHouse houseBill;

		protected override ZArchitecture.Data.Mutex.ZGlobalMutex GetMutex() => houseBill?.Mutex;
	}
}
