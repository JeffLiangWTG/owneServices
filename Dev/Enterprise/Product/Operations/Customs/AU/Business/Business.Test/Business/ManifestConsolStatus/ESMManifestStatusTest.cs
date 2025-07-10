using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ESMManifestStatus))]
	sealed class ESMManifestStatusTest : AUCustomsManifestStatusTest
	{
		public void TestCCRNIsReadonlyIfWeHaveACRN()
		{
			var consol = Factory.New<ForwardingConsol>();
			var status = new ESMManifestStatusForTest(consol);
			Assert(!status.ContingencyCANInfo.ReadOnly);

			consol = Factory.New<ForwardingConsol>();
			status = new ESMManifestStatusForTest(consol);
			var entryNum = new FreightConsolWrapper(consol).CreateCustomsAuthorityNumber();
			entryNum.CE_EntryNum = "12345";
			entryNum.CE_EntryType = CusEntryNumberTypes.Australia.CRN;

			status = new ESMManifestStatusForTest(consol);
			Assert(status.ContingencyCANInfo.ReadOnly);
		}

		public void TestContingencyCAN()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());
			AssertEquals(ZString.Empty, status.ContingencyCAN);
			status.ContingencyCAN = "123";
			AssertEquals("123", status.ContingencyCAN);
			status.ContingencyCAN = "456";
			AssertEquals("456", status.ContingencyCAN);
			status.ContingencyCAN = ZString.Empty;
			AssertEquals(ZString.Empty, status.ContingencyCAN);
		}

		public void TestWeArentWaitingIfOutgoingMessageRejected()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());

			AddValidMessage(status.ManifestProvider);
			AssertEquals("WaitingForResponse", ManifestStatus.AwaitingResponse, status.LastMessageStatus);
			status.ManifestProvider.Messages[0].EM_Status = EDIMessage.Status.Rejected;
			Assert("NotWaitingForResponse", ManifestStatus.AwaitingResponse != status.LastMessageStatus);
		}

		public override void TestCustomsEntryNumber()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());
			AssertEquals("EntryNum", "#-#-#-#-#", status.E2_CustomsEntryNumber);
			var entryNum = new FreightConsolWrapper(status.ManifestProvider).CreateCusEntryNumber();
			entryNum.CE_EntryNum = "123456789";
			AssertEquals("EntryNum", "123456789", status.E2_CustomsEntryNumber);
		}

		public void TestResetToOriginal()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());

			AddValidMessage(status.ManifestProvider);
			var entryNum = new FreightConsolWrapper(status.ManifestProvider).CreateCusEntryNumber();
			AssertEquals("WaitingForResponse", ManifestStatus.AwaitingResponse, status.LastMessageStatus);
			var sender = new SendsMessagesToCustomsShutterUpperer();

			sender.AnswerToContinueWithAction = false;
			status.ResetToOriginal(sender);

			AssertEquals("WaitingForResponse", ManifestStatus.AwaitingResponse, status.LastMessageStatus);
			AssertEquals("Awaiting Response", status.E2_MessageStatus);
			sender.AnswerToContinueWithAction = true;
			status.ResetToOriginal(sender);

			Assert("NotWaitingForResponse", ManifestStatus.AwaitingResponse != status.LastMessageStatus);
			AssertEquals("Not Sent", status.E2_MessageStatus);
		}

		public void TestResetToOriginalAfterResponseReceived()
		{
			var consol = Factory.New<ForwardingConsol>();
			var status = new ESMManifestStatusForTest(consol);
			status = new ESMManifestStatusForTest(consol); // to fix the WrappedAndInnerPropertyNameStaticDictionaryForUI intermittent test failure

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
			}

			AddValidMessage(status.ManifestProvider);
			AddValidResponse(status.ManifestProvider);
			var entryNum = new FreightConsolWrapper(status.ManifestProvider).CreateCusEntryNumber();
			entryNum.CE_EntryNum = "123456789";
			status.ManifestProvider.HasChanges = false;
			AssertEquals("EntryNum", "123456789", status.E2_CustomsEntryNumber);
			AssertEquals(ManifestStatus.Cleared, status.LastMessageStatus);

			var consolWrapper = new FreightConsolWrapper(consol);
			consolWrapper.UpdatePreliminaryLinesToManifestedLines();
			var wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1 - HasSubManifestLineNumber", true, wrappedShipment.HasSubManifestLineNumber);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2 - HasSubManifestLineNumber", true, wrappedShipment.HasSubManifestLineNumber);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			status.ResetToOriginal(sender);

			Assert("NotWaitingForResponse", ManifestStatus.AwaitingResponse != status.LastMessageStatus);
			AssertEquals("EntryNum", ESMManifestStatus.EmptyCustomsEntryNumberString, status.E2_CustomsEntryNumber);
			AssertEquals("Not Sent", status.E2_MessageStatus);
			Assert("ConsolHasChanges", status.ManifestProvider.HasChanges);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1 - SubManifestLineNumber should have been deleted", false, wrappedShipment.HasSubManifestLineNumber);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2 - SubManifestLineNumber should have been deleted", false, wrappedShipment.HasSubManifestLineNumber);
		}

		public void TestPremisesID()
		{
			var consol = Factory.New<ForwardingConsol>();
			var org = OrgHeader.New(Factory);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			org.OH_RL_NKClosestPort = "AUSYD";
			var address1 = org.Addresses[0];
			address1.OA_Address1 = "test1";
			address1.OA_Address2 = "test2";
			consol.JK_OA_PackDepotAddress = address1.PK;
			consol.PackDepotAddress.LocalControlledPremisesID = "123987";
			AssertEquals(consol.PackDepotAddress.LocalControlledPremisesID, "123987");
			var status = new ESMManifestStatusForTest(consol);
			AssertEquals(consol.PackDepotAddress.LocalControlledPremisesID, status.PremisesID);

			var ediMessage = consol.Messages.AddNew(typeof(EDIMessage));
			ediMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			ediMessage.EM_MessageType = "ESM";
			ediMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_MessageNum = "0000001";
			ediMessage.EM_MessageText = "test";

			var entryNum = new FreightConsolWrapper(status.ManifestProvider).CreateCusEntryNumber();
			entryNum.CE_EntryNum = "123456789";
			status.ManifestProvider.HasChanges = false;
			AssertEquals("EntryNum", "123456789", status.E2_CustomsEntryNumber);
			AssertEquals("", status.PremisesID);
		}

		public void TestNewCreateOrReplaceMessageBuilder()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), status.NewCreateOrReplaceMessageBuilder().GetType());
		}

		public void TestNewMessageBuilder()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), status.NewMessageBuilders(Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Create)[0].GetType());
		}

		public void TestLastMessageStatusReallyIsForLastMessage()
		{
			var manifestProvider = NewManifestProvider();
			AddValidMessage(manifestProvider);

			var errorResponse = CreateErrorResponse(manifestProvider);
			Factory.Save();
			System.Threading.Thread.Sleep(1000);
			AddValidResponse(manifestProvider);
			manifestProvider.Messages.Add(errorResponse);
			Factory.Save();
			manifestProvider.Messages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Ascending);
			var testCustomsManifestStatus = NewCustomsManifestStatus(manifestProvider);
			AssertEquals("ManifestStatus", ManifestStatus.Cleared, testCustomsManifestStatus.LastMessageStatus);
		}

		public void TestNewMessageBuilderForWithdraw()
		{
			var status = new ESMManifestStatusForTest(Factory.New<ForwardingConsol>());
			var builders = status.NewMessageBuilders(Common.MessageBuilders.MessageSubTypes.Withdraw);
			AssertEquals("Builders.Length", 2, builders.Length);
			AssertEquals("MessageSubType[0]", Common.MessageBuilders.MessageSubTypes.Replace, ((ESMMessageBuilder)builders[0]).MessageSubType);
			AssertEquals("DontSendAnyLines[0]", true, ((ESMMessageBuilder)builders[0]).DontSendAnyLines);
			AssertEquals("VersionOffset[0]", 0, ((ESMMessageBuilder)builders[0]).VersionOffset);

			AssertEquals("MessageSubType[1]", Common.MessageBuilders.MessageSubTypes.Withdraw, ((ESMMessageBuilder)builders[1]).MessageSubType);
			AssertEquals("SetStatusToPending[1]", true, ((ESMMessageBuilder)builders[1]).SetStatusToPending);
			AssertEquals("VersionOffset[1]", 1, ((ESMMessageBuilder)builders[1]).VersionOffset);
		}

		public void TestIdleMessage()
		{
			var consol = Factory.New<ForwardingConsol>();
			var status = new ESMManifestStatusForTest(consol);

			AddValidMessage(consol);
			AddValidResponse(consol);

			var idlMessage = Factory.New<CMRIDLMessage>();
			idlMessage.EM_ReceiveTransmit = CMRIDLMessage.Direction.Receive;
			consol.Messages.Add(idlMessage);

			AssertEquals(ManifestStatus.Idle.AsString, status.E2_MessageStatus);
		}

		protected override EDIMessage AddValidMessage(IManifestProvider manifestProvider)
		{
			var result = manifestProvider.Messages.AddNew(typeof(TestHelperCMRESMMessage));
			result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.EM_Status = EDIMessage.Status.Sent;
			result.EM_MessageText = ValidESMMEssage.Replace("\r\n", "");

			return result;
		}

		protected override EDIMessage AddValidResponse(IManifestProvider manifestProvider)
		{
			var result = manifestProvider.Messages.AddNew(typeof(TestHelperCMRESMMessage));
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_Status = EDIMessage.Status.Received;
			result.EM_MessageText = ValidESMResponse.Replace("\r\n", "");

			return result;
		}

		EDIMessage CreateErrorResponse(IManifestProvider manifestProvider)
		{
			var result = Factory.New<TestHelperCMRESMMessage>();
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_Status = EDIMessage.Status.Received;
			result.EM_MessageText = ValidESMResponse.Replace("\r\n", "").Replace("CLEAR", "ERROR");
			return result;
		}

		protected override IManifestProvider NewManifestProvider()
		{
			return Factory.New<ForwardingConsol>();
		}

		protected override ZString ExpectedCustomsEntryNumber
		{
			get { return "AAAAGP9YS"; }
		}

		#region Test Helper Classes

		class TestHelperCMRESMMessage : CMRESMMessage
		{
			public TestHelperCMRESMMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
			}
		}

		#endregion

		const string ValidESMMEssage = @"UNH+1+CUSCAR:D:99B:UN'
BGM+87:::ESM+K00001021/1:1+9'
TDT+20+++11'
DTM+136:20040805:102'
GIS+S:121:95'
CNT+36:2'
CNI+1'
CNT+36:2'
RFF+TN:AAAAGPGHL'
GID+1'
UNT+11+1'";

		const string ValidESMResponse = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::ESMR+34GG 29B2 564:001+11'
FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'
NAD+MR+41065894724::95'
RFF+ABO:K00001021/1::001'
RFF+ACW:ESM'
RFF+AFM:9'
RFF+AIZ:AAAAGP9YS'
DOC+1'
RFF+TN:AAAAGPGHL'
CST+0001'
FTX+AHN+++CLEAR'
CNT+5:0001'
UNT+14+000001'";
	}

	class ESMManifestStatusForTest : ESMManifestStatus
	{
		public ESMManifestStatusForTest(ForwardingConsol consol) : base(consol)
		{
		}

		new internal IManifestMessageBuilder[] NewMessageBuilders(Common.MessageBuilders.MessageSubTypes messageSubType) => base.NewMessageBuilders(messageSubType);
	}
}
