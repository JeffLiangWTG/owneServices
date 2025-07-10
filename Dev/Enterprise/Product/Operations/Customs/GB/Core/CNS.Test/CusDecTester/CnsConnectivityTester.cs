//// This will be in the stability checker one day

//using System;
//using System.Collections.Generic;
//using System.Text;
//using CargoWise.EntityFramework.Testing; 
//using Enterprise.Customs.GB.CNS.WebServices.CnsChiefEDI;
//using Enterprise.Customs.GB.CNS.WebServices.CnsPrints;

//namespace Enterprise.Customs.GB.CNS.CusDec.Testing
//{
//	public class CnsConnectivityTester : TestCaseWithFactory
//	{
//		[DeveloperOnlyTest]
//		public void TestCnsGettingPrints()
//		{
//			string url = "https://195.171.138.110/ws/Mailbox/MailBoxPortImpl";
//			GB.Registry.GBCustomsDataRegistry.Instance.CnsPrintsUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, url);			
//			MailBox prints = new MailBox();
//			AssertEquals(url, prints.Url);
//			AcknowledgeEdifactPrints printsToAck = new AcknowledgeEdifactPrints();
//			printsToAck.device = "CGWEXTSWH01";
//			printsToAck.batchId = 69696969;
//			//This will actually connect to CNS and on to chief!
//			AcknowledgeEdifactPrintsResponse printsResp = prints.AcknowledgeEdifactPrints(printsToAck);   // If this fails it may mean we cannot reach CNS.  Don't worry this does not execute on DAT.  
//			AssertEquals("No unacknowledged Edifact Prints found for device: CGWEXTSWH01 and Batch ID: 69696969", printsResp.messageText);
//		}

//		[DeveloperOnlyTest]
//		public void TestCnsSendingCusdecs()
//		{
//			string url = "https://195.171.138.110/ws/CCMI/ChiefEDI";
//			GB.Registry.GBCustomsDataRegistry.Instance.CnsUploadUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, url);
//			ChiefEDIPortQSService cusdecSender = new ChiefEDIPortQSService();
//			AssertEquals(url, cusdecSender.Url);			
//			// Look over there ----->----->----->----->----->----->----->----->----->----->----->----->----->---9GB999999999000-B00001143  <-- that's the DUCR that does not exist
//			string sampleEdifact = @"UNH+2278+CUSDEC:D:04A:UN:109760+2K00MW3YQ'BGM+XTC::109++1'CST++IMA'RFF+ABO:9GB999999999000-B00001143'UNS+D'UNS+S'UNT+7+2278'";
//			// This will actually connect to CNS and on to chief
//			string result = cusdecSender.processEDIMessage(sampleEdifact, "AAW", true);  // If this fails it may mean we cannot reach CNS.  Don't worry this does not execute on DAT.  
//			AssertContains("E408 Unique Consignment reference does not exist", result);
//		}
//	}
//}
