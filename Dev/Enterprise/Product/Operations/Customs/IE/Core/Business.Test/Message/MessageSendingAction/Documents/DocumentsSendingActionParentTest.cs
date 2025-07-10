using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DocumentsSendingActionParent))]
	class DocumentsSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<DocumentsSendingActionParent, DocumentsSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(DocumentsSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentsSendingActionParent(Factory.New<JobDeclaration>(), AESOutgoingMessageTypeList.Codes.DocumentUpload);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("When MessageType is null", () => new DocumentsSendingActionParent(Factory.New<JobDeclaration>(), null));
			AssertExceptionThrown<ArgumentException>("When MessageType is empty", () => new DocumentsSendingActionParent(Factory.New<JobDeclaration>(), ""));
		}
	}
}
