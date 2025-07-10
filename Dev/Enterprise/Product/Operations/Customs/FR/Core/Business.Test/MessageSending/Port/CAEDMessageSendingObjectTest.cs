using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class CAEDMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestMessageOwner()
		{
			AssertSame(header, sendingObject.MessagesOwner);
		}

		public void TestMessageType()
		{
			AssertEquals(MessageSubTypeList.Codes.CAED, sendingObject.MessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			dataObject = new CAEDDataObject();
			sendingObject = new CAEDMessageSendingObject(header, dataObject);
		}

		CAEDMessageSendingObject sendingObject;
		NctsHeader header;
		CAEDDataObject dataObject;
	}
}
