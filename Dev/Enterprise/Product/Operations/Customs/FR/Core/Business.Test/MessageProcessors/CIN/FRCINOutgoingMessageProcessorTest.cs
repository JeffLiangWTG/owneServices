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
	public class FRCINOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessorFilters()
		{
			GlbCompany.CurrentCompany.SetCountry("FR");
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-SENDER-UT");

			var deBranch = Factory.New<GlbBranch>();
			deBranch.GB_RN_NKCountryCode = "DE";
			deBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			deBranch.GB_Code = "DEB";
			deBranch.GB_IsActive = false;

			var msg1 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg1.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg1.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg1.EM_GB = branch.PK;
			msg1.EM_MessageText = "BLA,Bla MSG0001";

			var msg2 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg2.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			msg2.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg2.EM_GB = deBranch.PK;
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

			var msg7 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg7.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg7.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg7.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg7.EM_GB = branch.PK;
			msg7.EM_MessageText = "BLA,Bla MSG0001 CIN 745";
			msg7.EM_MessageType = MessageTypeList.Codes.CIN745;

			var msg8 = Factory.NewWithValidTestData<TestEDIMessage>();
			msg8.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			msg8.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			msg8.EM_Status = EDIMessageStatusList.Codes.Queued;
			msg8.EM_GB = branch.PK;
			msg8.EM_MessageText = "BLA,Bla MSG0001 CIN 755";
			msg8.EM_MessageType = MessageTypeList.Codes.CIN755;

			Factory.Save();

			var processor = new FRCINOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg1.Reload();
			msg2.Reload();
			msg3.Reload();
			msg4.Reload();
			msg5.Reload();
			msg6.Reload();
			msg7.Reload();
			msg8.Reload();
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals(3, interchanges.Length);
			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg6.EM_EI);
			AssertEquals("EI_ApplicationCode", "GMD", interchange1.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "FRT", interchange1.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange1.EI_ReceiveTransmit);
			AssertEquals("EI_Status", "HQU", interchange1.EI_Status);
			AssertEquals("Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange1.EI_From);
			AssertEquals("Recepient", "ABCDEFG", interchange1.EI_To);
			AssertEquals("EI_BodyText", "BLA,Bla MSG0001 CIN", interchange1.EI_BodyText);
			AssertContains("EI_HeaderText", "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>FRT</InterchangeType><InterchangeNumber>", interchange1.EI_HeaderText);
			AssertEquals(interchange1.PK, msg6.EM_EI);

			AssertEquals("QUE", msg1.EM_Status);
			AssertEquals("QUE", msg2.EM_Status);
			AssertEquals("QUE", msg3.EM_Status);
			AssertEquals("QUE", msg4.EM_Status);
			AssertEquals("CAN", msg5.EM_Status);
			AssertEquals("SNT", msg6.EM_Status);
			AssertEquals("SNT", msg7.EM_Status);
			AssertEquals("SNT", msg8.EM_Status);
		}
	}
}
