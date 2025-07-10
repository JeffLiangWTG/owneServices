using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaTOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestPopulateWithQuerySatisfiedAndActiveBranches()
		{
			var msg1 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0001: F15 & All Satisfied",
				MessageTypeList.Codes.DTF15,
				"DT"
				);

			var msg2 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				deBranch.PK,
				"MSG0002: F15 & Another branch",
				MessageTypeList.Codes.DTF15,
				"DT"
				);

			var msg3 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				EDIInterchangeTypeList.Codes.AUCustoms,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0003: F15 & Customs is AUCustoms",
				MessageTypeList.Codes.DTF15,
				"DT"
				);

			var msg4 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Receive,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0004: F15 & Direction is Receive",
				MessageTypeList.Codes.DTF15,
				"DT"
				);

			var msg5 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Cancelled,
				branch.PK,
				"MSG0005: F15 & Status is Cancelled",
				MessageTypeList.Codes.DTF15,
				"DT"
				);

			var msg6 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0006: COD",
				MessageTypeList.Codes.COD,
				"DT"
				);

			var msg7 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0007: CIN",
				MessageTypeList.Codes.CIN,
				"DT"
				);

			var msg8 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0008: F15: MessageSubType is empty",
				MessageTypeList.Codes.DTF15,
				string.Empty
				);

			Factory.Save();

			var processor = new DeltaTOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg1.Reload();
			msg2.Reload();
			msg3.Reload();
			msg4.Reload();
			msg5.Reload();
			msg6.Reload();
			msg7.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("MSG0001 sent because it's F15 and all conditions are satified.", "SNT", msg1.EM_Status);
				AssertEquals("MSG0002 not processed because it's owner branch is sharing same company with current branch.", "SNT", msg2.EM_Status);
				AssertEquals("MSG0003 not processed because the ApplicationCode is not FRC.", "QUE", msg3.EM_Status);
				AssertEquals("MSG0004 not processed because it's a received message", "QUE", msg4.EM_Status);
				AssertEquals("MSG0005 not processed because the Status is cancelled.", "CAN", msg5.EM_Status);
				AssertEquals("MSG0006 not processed because it's a COD message, not included in a DeltaTOutgoingProcessor targets.", "QUE", msg6.EM_Status);
				AssertEquals("MSG0007 not processed because it's a CIN message, not included in a DeltaTOutgoingProcessor targets.", "QUE", msg7.EM_Status);
				AssertEquals("MSG0008 not processed because the MessageSubType is empty, while requiring DT", "QUE", msg8.EM_Status);
			});

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals("Only MSG0001 and MSG002 are processed.", 2, interchanges.Length);

			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg1.EM_EI);
			MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchange1,
				expectedApplicationCode: "GMD",
				expectedInterchangeType: "FRC",
				expectedReceiveTransmit: "TRX",
				expectedStatus: "HQU",
				expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				expectedTo: "ABCDEFG",
				expectedBodyText: "MSG0001: F15 & All Satisfied",
				expectedHeaderText: "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>FRC</InterchangeType><InterchangeNumber>"
				);

			var interchange2 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg2.EM_EI);
			MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchange2,
				expectedApplicationCode: "GMD",
				expectedInterchangeType: "FRC",
				expectedReceiveTransmit: "TRX",
				expectedStatus: "HQU",
				expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				expectedTo: "ABCDEFG",
				expectedBodyText: "MSG0002: F15 & Another branch",
				expectedHeaderText: "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>FRC</InterchangeType><InterchangeNumber>"
				);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-UNIT-TEST");

			deBranch = Factory.New<GlbBranch>();
			deBranch.GB_RN_NKCountryCode = "DE";
			deBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			deBranch.GB_Code = "DEB";
			deBranch.GB_IsActive = false;
		}
		GlbBranch branch;
		GlbBranch deBranch;
	}
}
