using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDOC271MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("messageOwner is must", () => new ILDOC271MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("supportingDocument is must", () => new ILDOC271MessageBuilder(Factory.New<JobDeclaration>(), null));
		}

		protected override IMessageBuilder GetMessageBuilder() => new ILDOC271MessageBuilder(jobDeclaration, supportingDocument);

		protected override string GetExpectedMessageSubType() => "271";

		protected override string GetExpectedMessageText() => string.Empty;

		protected override string GetExpectedMessageType() => "DOC";

		protected override BusinessObject GetLinkedObject() => jobDeclaration;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => jobDeclaration.Messages;

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
		}

		JobDeclaration jobDeclaration;
		SupportingDocument supportingDocument;
	}
}
