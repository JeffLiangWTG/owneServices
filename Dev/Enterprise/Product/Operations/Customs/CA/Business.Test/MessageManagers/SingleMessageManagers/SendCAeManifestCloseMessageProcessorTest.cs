using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SendCAeManifestCloseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ"))
			{
				var cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
				var house = cusCAeMHMaster.HouseBills.AddNew();
				house.BW_HouseCCN = "8123CCN1";
				var house2 = cusCAeMHMaster.HouseBills.AddNew();
				house2.BW_HouseCCN = "8123CCN2";
				house.Items.AddNew();
				house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
				house2.Items.AddNew();
				house2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
				Assert("Pre-condition", cusCAeMHMaster.Messages.Count == 0);
				Factory.Save();

				var processor = new SendCAeManifestCloseMessageProcessor(cusCAeMHMaster);
				processor.Process(new Notifications());
				Assert("One message created", cusCAeMHMaster.Messages.Count == 1);
			}
		}
	}
}
