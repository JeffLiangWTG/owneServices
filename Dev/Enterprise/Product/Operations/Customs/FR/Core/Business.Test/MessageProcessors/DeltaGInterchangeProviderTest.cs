using System;
using System.Data;
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
	sealed class DeltaGInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestPopulateInterchangeCorrectly()
		{
			var msg1 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0001: COD & All Satisfied",
				MessageTypeList.Codes.COD,
				string.Empty
				);

			var msg2 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Internal,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0004: COD & Direction is Internal",
				MessageTypeList.Codes.COD,
				string.Empty
				);

			var msg3 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				EDIInterchangeTypeList.Codes.AUCustoms,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0003: COD & Customs is AUCustoms",
				MessageTypeList.Codes.COD,
				string.Empty
				);

			var msg4 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Receive,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0004: COD & Direction is Receive",
				MessageTypeList.Codes.COD,
				string.Empty
				);

			var msg5 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Cancelled,
				branch.PK,
				"MSG0005: COD & Status is Cancelled",
				MessageTypeList.Codes.COD,
				string.Empty
				);

			var msg6 = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				"MSG0006: CIN",
				MessageTypeList.Codes.CIN,
				string.Empty
				);

			Factory.Save();

			var processor = new DeltaGOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg1.Reload();
			msg2.Reload();
			msg3.Reload();
			msg4.Reload();
			msg5.Reload();
			msg6.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("MSG0001 sent because it's COD and all conditions are satified.", "SNT", msg1.EM_Status);
				AssertEquals("MSG0002 not processed because it's owner branch is sharing same company with current branch.", "QUE", msg2.EM_Status);
				AssertEquals("MSG0003 not processed because the ApplicationCode is not FRC.", "QUE", msg3.EM_Status);
				AssertEquals("MSG0004 not processed because it's a received message", "QUE", msg4.EM_Status);
				AssertEquals("MSG0005 not processed because the Status is cancelled.", "CAN", msg5.EM_Status);
				AssertEquals("MSG0006 not processed because it's a CIN message, not included in a DeltaGOutgoingProcessor targets.", "QUE", msg6.EM_Status);
			});

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals("Only MSG0001 is processed", 1, interchanges.Length);

			var interchange1 = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg1.EM_EI);
			MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchange1,
				expectedApplicationCode: "GMD",
				expectedInterchangeType: "FRC",
				expectedReceiveTransmit: "TRX",
				expectedStatus: "HQU",
				expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				expectedTo: "ABCDEFG",
				expectedBodyText: "MSG0001: COD & All Satisfied",
				expectedHeaderText: "</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>FRC</InterchangeType><InterchangeNumber>"
				);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			branch.GB_IsActive = false;
		}
		GlbBranch branch;
	}
}
