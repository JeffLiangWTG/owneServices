using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ACIForwarderCloseMessage))]
	sealed class ACIForwarderCloseMessageTest : ACIForwarderMessageTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (ACIForwarderCloseMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ACIForwarderCloseMessage>();
		}

		public override void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAACI, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.ACIForwarderClose, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestNeedAutoCloseReport()
		{
			var testMessage = Factory.New<ACIForwarderCloseMessage>();
			testMessage.NeedAutoCloseReport = true;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var messageInNewFactory = factory.Load<ACIForwarderCloseMessage>(testMessage.PK);
			Assert("NeedAutoCloseReport", messageInNewFactory.NeedAutoCloseReport);
		}
	}
}
