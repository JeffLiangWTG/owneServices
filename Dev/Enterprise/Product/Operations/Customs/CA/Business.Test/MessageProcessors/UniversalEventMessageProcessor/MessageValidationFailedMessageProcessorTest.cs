using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageValidationFailedMessageProcessorTest : CAUniversalEventMessageProcessorTest
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
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";

			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";

			CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ESG");
			CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.Save();
			return ("ns2@cargowise.com", MessageTypeDescription);
		}

		protected override string[] ExpectedStatusList => new[] { MessageStatusList.Codes.ErrorOriginal, MessageStatusList.Codes.ErrorChange,
			MessageStatusList.Codes.ErrorReplace, MessageStatusList.Codes.ErrorDelete };

		protected override string UniversalEventFileName => "MessageValidationFailed.xml";

		protected override string MessageInterpretationFileName => "MessageValidationFailed.html";

		protected override string MessageTypeDescription => "Message Validation Failed";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MessageValidationFailedMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}
