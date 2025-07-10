using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRCINInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestInterchangePopulatedCorrectly()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CIN-SENDER-UT");
			branch.GB_IsActive = false;

			var msg1 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg1.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg1.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg1.EM_GB = GlbBranch.CurrentBranch.PK;
			msg1.EM_MessageText = "BLA,Bla MSG0001";

			var msg2 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg2.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			msg2.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg2.EM_GB = branch.PK;
			msg2.EM_MessageText = "BLA,Bla MSG0002";

			var msg3 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg3.EM_ApplicationCode = EDIInterchangeTypeList.Codes.AUCustoms;
			msg3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg3.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg3.EM_GB = branch.PK;
			msg3.EM_MessageText = "BLA,Bla MSG0003";
			var msg4 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg4.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg4.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg4.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg4.EM_GB = branch.PK;
			msg4.EM_MessageText = "BLA,Bla MSG0004";
			var msg5 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg5.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg5.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg5.EM_Status = EDIMessageStatusList.Codes.Cancelled;
			msg5.EM_GB = branch.PK;
			msg5.EM_MessageText = "BLA,Bla MSG0005";

			var msg6 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg6.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg6.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg6.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg6.EM_GB = branch.PK;
			msg6.EM_MessageText = "BLA,Bla MSG0001 CIN";
			msg6.EM_MessageType = MessageTypeList.Codes.CIN;

			Factory.Save();

			var processor = new FRCINOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg1.Reload();
			msg2.Reload();
			msg3.Reload();
			msg4.Reload();
			msg5.Reload();
			msg6.Reload();

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals(1, interchanges.Length);

			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg6.EM_EI);
			AssertEquals("EI_ApplicationCode", "GMD", interchange1.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "FRT", interchange1.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange1.EI_ReceiveTransmit);
			AssertEquals("EI_Status", "HQU", interchange1.EI_Status);
			AssertEquals("Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange1.EI_From);
			AssertEquals("Recepient", "EASYLO2TEST_EAD", interchange1.EI_To);
			AssertEquals("EI_BodyText", "BLA,Bla MSG0001 CIN", interchange1.EI_BodyText);
			AssertContains("EI_HeaderText", "</SenderID><RecipientID>EASYLO2TEST_EAD</RecipientID><InterchangeType>FRT</InterchangeType><InterchangeNumber>", interchange1.EI_HeaderText);
			AssertEquals(interchange1.PK, msg6.EM_EI);

			AssertEquals("QUE", msg1.EM_Status);
			AssertEquals("QUE", msg2.EM_Status);
			AssertEquals("QUE", msg3.EM_Status);
			AssertEquals("QUE", msg4.EM_Status);
			AssertEquals("CAN", msg5.EM_Status);
			AssertEquals("SNT", msg6.EM_Status);
		}
	}
}
