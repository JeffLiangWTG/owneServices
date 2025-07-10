using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DOAMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestMessageOwner()
		{
			AssertSame(header, sendingObject.MessagesOwner);
		}

		public void TestMessageType()
		{
			AssertEquals(MessageSubTypeList.Codes.DOA, sendingObject.MessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			dataObject = new DOADataObject();
			sendingObject = new DOAMessageSendingObject(header, dataObject);
		}

		DOAMessageSendingObject sendingObject;
		NctsHeader header;
		DOADataObject dataObject;
	}
}
