using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class DependentObjectControllerHelper
	{
		public DependentObjectControllerHelper(IFormPresenter presenter)
		{
			this.presenter = presenter;
		}

		public void EditExisting(BusinessObject bo)
		{
			presenter.ShowEdit(ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment, bo);
		}

		public void EditExisting(BusinessObject bo, ControllerID controllerID)
		{
			presenter.ShowEdit(controllerID, bo);
		}

		readonly IFormPresenter presenter;
	}
}
