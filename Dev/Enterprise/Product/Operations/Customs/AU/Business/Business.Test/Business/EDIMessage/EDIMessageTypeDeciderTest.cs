using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestCOLSMessageType()
		{
			var message = Factory.New<COLSMessage>();
			message.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			Factory.Save();

			CombineAssertions(() =>
			{
				var messageInNewFactory = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
				AssertType<COLSMessage>("For not XT Message", messageInNewFactory);

				message.EM_MessageType = AUCOLSMessageTypeList.Codes.XtMessageError;
				Factory.Save();

				messageInNewFactory = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
				AssertType<COLSXtErrorResponseMessage>("For XT Message", messageInNewFactory);
			});
		}
	}
}
