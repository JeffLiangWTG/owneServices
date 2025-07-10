using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	sealed class ServiceTaskApplicationCodeListTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var list = new ServiceTaskApplicationCodeList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 10, list.Count);
				AssertEquals("Code 'DET'", "DE ATLAS Customs Message Retrieving", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEAMessageRetrieving));
				AssertEquals("Code 'DEE'", "DE AES Customs Message Retrieving", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEEMessageRetrieving));
				AssertEquals("Code 'DEV'", "DE EMCS Customs Message Retrieving", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEMMessageRetrieving));
				AssertEquals("Code 'DEI'", "DE Customs Acknowledgement Processing", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEIMessageProcessing));
				AssertEquals("Code 'DMS'", "DE EMCS Customs Message Sending", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEMMessageSending));
				AssertEquals("Code 'DXS'", "DE AES Customs Message Sending", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEEMessageSending));
				AssertEquals("Code 'DAS'", "DE ATLAS Customs Message Sending", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEAMessageSending));
				AssertEquals("Code 'DXP'", "DE AES Customs Message Processing", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEEMessageProcessing));
				AssertEquals("Code 'DMP'", "DE EMCS Customs Message Processing", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEMMessageProcessing));
				AssertEquals("Code 'DAP'", "DE ATLAS Customs Message Processing", list.GetDescriptionFromCode(ServiceTaskApplicationCodeList.Codes.DEAMessageProcessing));
			});
		}
	}
}
