using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoMessagingTriggerActionProcessorForOutturnTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "1234";
			outturnHeader.C6_OutturningPremiseID = "1234";
			outturnHeader.C6_VoyageNum = "1234";
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "1234";
			var processor = new HouseBillsCargoMessageProcessor(new NotificationBuffer());
			AssertEquals(0, processor.GeneratedHouseMessages.Count());

			using (var job = new SeaCargoOutturnHeaderProcessorJob(outturnHeader))
			{
				processor.Process(job);
			}
			AssertEquals(1, processor.GeneratedHouseMessages.Count());
		}
	}
}
