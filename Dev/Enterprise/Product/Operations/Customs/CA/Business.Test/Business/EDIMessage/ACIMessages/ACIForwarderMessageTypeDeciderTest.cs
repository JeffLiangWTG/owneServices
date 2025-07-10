using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ACIForwarderMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestACIForwarderMessageTypeDecider()
		{
			AssertMessageType(MessageTypeList.Codes.ACIForwarderClose, typeof(ACIForwarderCloseMessage));
			AssertMessageType(MessageTypeList.Codes.ACIHouseBill, typeof(ACIHouseBillMessage));
			AssertMessageType(ZString.Empty, typeof(ACIForwarderMessage));
		}

		void AssertMessageType(ZString typeToCheck, Type typeExpected)
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = typeToCheck;
			Factory.Save();
			var messageReload = new BusinessObjectFactory().Load<ACIForwarderMessage>(message.PK);
			AssertEquals("Type", typeExpected, messageReload.GetType());
		}
	}
}
