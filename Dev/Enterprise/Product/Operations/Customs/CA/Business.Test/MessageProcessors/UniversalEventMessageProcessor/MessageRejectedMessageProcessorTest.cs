using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageRejectedMessageProcessorTest : CAUniversalEventMessageProcessorTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShouldUpdateStatuesToRejected()
		{
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			GetMessageProcesser().Process();
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEquals(B3EntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
		}

		protected override (string ExpectedEmail, string ExpectedSubject) SetupRegistryForTest()
		{
			var newGroup3 = Factory.New<GlbGroup>();
			newGroup3.GG_Code = "NG3";

			var newStaff3 = newGroup3.Staff.AddNew();
			newStaff3.GS_Code = "NS3";
			newStaff3.GS_LoginName = "NS3";
			newStaff3.GS_EmailAddress = "ns3@cargowise.com";

			CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ESG");
			CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup3.PK.ToGuid());

			Factory.Save();
			return ("ns3@cargowise.com", MessageTypeDescription);
		}

		protected override string[] ExpectedStatusList => new[] { MessageStatusList.Codes.ErrorOriginal, MessageStatusList.Codes.ErrorChange,
			MessageStatusList.Codes.ErrorReplace, MessageStatusList.Codes.ErrorDelete };

		protected override string UniversalEventFileName => "MessageRejected.xml";

		protected override string MessageInterpretationFileName => "MessageRejected.html";

		protected override string MessageTypeDescription => "Message Rejected";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MessageRejectedMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}
