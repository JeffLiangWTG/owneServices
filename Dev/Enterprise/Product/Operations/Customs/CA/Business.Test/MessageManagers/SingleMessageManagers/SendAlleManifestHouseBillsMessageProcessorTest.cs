using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SendAlleManifestHouseBillsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ"))
			{
				var cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
				var house = cusCAeMHMaster.HouseBills.AddNew();
				house.BW_HouseCCN = "8123CCN1";
				house.BW_Weight = 1;
				var house2 = cusCAeMHMaster.HouseBills.AddNew();
				house2.BW_HouseCCN = "8123CCN2";
				house2.BW_Weight = 1;

				house.Items.AddNew();
				house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
				house2.Items.AddNew();
				house2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
				Assert("Pre-condition", cusCAeMHMaster.HouseBills[0].Messages.Count == 0);
				Assert("Pre-condition2", cusCAeMHMaster.HouseBills[1].Messages.Count == 0);
				Factory.Save();

				var processor = new SendAlleManifestHouseBillsMessageProcessor(cusCAeMHMaster);
				processor.Process(new Notifications());
				Assert("One message created", cusCAeMHMaster.HouseBills[0].Messages.Count == 1);
				Assert("Two message created", cusCAeMHMaster.HouseBills[1].Messages.Count == 1);
				Assert(cusCAeMHMaster.HouseBills[0].Messages[0].EM_SendWithMessageErrors);
				Assert(cusCAeMHMaster.HouseBills[1].Messages[0].EM_SendWithMessageErrors);
			}
		}
	}
}
