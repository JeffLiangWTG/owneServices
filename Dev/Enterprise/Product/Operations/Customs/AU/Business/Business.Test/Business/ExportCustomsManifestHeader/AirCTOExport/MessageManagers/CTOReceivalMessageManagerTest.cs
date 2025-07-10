using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOReceivalMessageManagerTest : TestCaseWithFactory
	{
		public void TestCTOReceivalMessageManager()
		{
			var message = (CMRCTORECMessage)line.Messages.AddNew(typeof(CMRCTORECMessage));
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CTOREC;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Factory.Save();
			AssertEquals(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, line.CTORECStatus.Code);
			AssertEquals(line.CTORECStatus.Code, manager.GetStatus());
		}

		public void TestMessages()
		{
			var message = line.Messages.AddNew();
			AssertEquals(1, manager.GetMessages(line).Count);
			AssertEquals(message, manager.GetMessages(line)[0]);

			var line2 = header.Lines.AddNew();
			AssertEquals(line2.Messages, manager.GetMessages(line2));
		}

		public void TestGetAmendmentManager()
		{
			line.EL_CAN = "SPAMSPAM";
			var messages = manager.GetAmendmentManager(line).GenerateAmendmentMessageSet();
			AssertEquals(1, messages.Length);
			AssertContains("SPAMSPAM", messages[0].EM_MessageText);

			var line2 = header.Lines.AddNew();
			line2.EL_CAN = "EGGSEGGS";
			messages = manager.GetAmendmentManager(line2).GenerateAmendmentMessageSet();
			AssertEquals(1, messages.Length);
			AssertNotContains("doesn't contain text from the first line", "SPAMSPAM", messages[0].EM_MessageText);
			AssertContains("EGGSEGGS", messages[0].EM_MessageText);
		}

		public void TestGetMessageBuilder()
		{
			line.EL_CAN = "SPAMSPAM";
			var builder = manager.GetBuilder(line);
			var message = builder[0].PopulateMessagesReturningResult();
			AssertContains("SPAMSPAM", message.EM_MessageText);

			var line2 = header.Lines.AddNew();
			line2.EL_CAN = "EGGSEGGS";
			builder = manager.GetBuilder(line2);
			message = builder[0].PopulateMessagesReturningResult();
			AssertNotContains("doesn't contain text from the first line", "SPAMSPAM", message.EM_MessageText);
			AssertContains("EGGSEGGS", message.EM_MessageText);
		}

		public void TestMessageFriendlyName()
		{
			line.EL_CAN = "whoopwhoop";

			var typeList = new CANTypeList();
			Assert(typeList.Count > 2);
			AssertCollectionContains(CANType.ContingencyCustomsAuthorityNumber, typeList);
			AssertCollectionContains(CANType.CustomsAuthorityNumber, typeList);
			AssertCollectionContains(CANType.Exemptions.EXDC, typeList);
			foreach (CodeDescriptionPair type in typeList)
			{
				line.EL_TypeOfCAN = type.Code;
				if (type == CANType.ContingencyCustomsAuthorityNumber || type == CANType.CustomsAuthorityNumber)
				{
					AssertEquals("CTO Receival Report - #1 - whoopwhoop", manager.MessageFriendlyName);
				}
				else
				{
					AssertEquals("CTO Receival Report - #1 - " + type.Code, manager.MessageFriendlyName);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<AirCTOExportCustomsManifestHeader>();
			line = header.Lines.AddNew();
			manager = new CTOReceivalMessageManager(line);
		}

		AirCTOExportCustomsManifestHeader header;
		ExportCustomsManifestLines line;
		CTOReceivalMessageManager manager;
	}
}
