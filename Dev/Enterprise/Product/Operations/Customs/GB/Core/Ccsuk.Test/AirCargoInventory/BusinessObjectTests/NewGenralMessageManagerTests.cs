using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class NewGenralMessageManagerTests : TestCaseWithFactory
	{
		public void TestExecution()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			npbo.Pima = "PIMA";
			npbo.Payload = "PAYLOAD";
			var manager = new NewGenralMessageManager(npbo);
			manager.ExecuteMakingRealEdiMessageFromNonPersistentHelper();
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull(message);
			AssertEquals("PIMA", message.EM_ApplicationReference);
		}
	}
}
