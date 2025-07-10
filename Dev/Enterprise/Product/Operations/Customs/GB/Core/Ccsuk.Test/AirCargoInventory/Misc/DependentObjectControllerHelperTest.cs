using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class DependentObjectControllerHelperTest : TestCase
	{
		public void TestEditExisting()
		{
			var presenter = new PresenterForTest();
			var dependentHelper = new DependentObjectControllerHelper(presenter);
			dependentHelper.EditExisting(null, ControllerIDs.RefNMFC);
			AssertEquals(ControllerIDs.RefNMFC, presenter.ControllerIdPassedForTest);
			dependentHelper.EditExisting(null);
			AssertEquals(ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment, presenter.ControllerIdPassedForTest);
		}
	}

	class PresenterForTest : IFormPresenter
	{
		public ControllerID ControllerIdPassedForTest { get; private set; }

		public void ShowEdit(ControllerID controllerId, BusinessObject bo)
		{
			ControllerIdPassedForTest = controllerId;
		}

		public void ShowError(string message, string caption)
		{
		}

		public void ShowNew(ControllerID controllerId, BusinessObject bo)
		{
		}

		public void ShowView(ControllerID controllerId, BusinessObject bo)
		{
		}
	}
}
