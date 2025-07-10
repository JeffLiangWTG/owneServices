using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDEC751MessageBuilderTest : ILMessageBuilderBaseTest
	{
		protected override IMessageBuilder GetMessageBuilder() => new ILDEC751MessageBuilder(entryHeader);

		protected override string GetExpectedMessageSubType() => "751";

		protected override string GetExpectedMessageText() => ZString.Empty;

		protected override string GetExpectedMessageType() => "DEC";

		protected override BusinessObject GetLinkedObject() => entryHeader;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => entryHeader.Messages;

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		}

		JobDeclaration jobDeclaration;
		CusEntryHeader entryHeader;
	}
}
