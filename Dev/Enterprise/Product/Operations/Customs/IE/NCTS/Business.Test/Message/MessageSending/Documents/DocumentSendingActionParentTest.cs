using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingActionParent))]
	class DocumentSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			AssertSame("Header", nctsHeader, sendingActionParent.Header);
		}

		public void TestMessageType()
		{
			AssertEquals("MessageType", "083", sendingActionParent.MessageType);
		}

		public void TestTopLevelBusinessObject()
		{
			AssertSame("TopLevelBusinessObject", nctsHeader, sendingActionParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals("SecurityCheckpointToSendWithMessageError", Env.Security.CustomsDeclarationSendWithMessageErrors, sendingActionParent.SecurityCheckpointToSendWithMessageError);
		}

		protected override BusinessObject GetNewBusinessObject() => new DocumentSendingActionParent(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			sendingActionParent = new DocumentSendingActionParent(nctsHeader);
		}
		NctsHeader nctsHeader;
		DocumentSendingActionParent sendingActionParent;
	}
}
