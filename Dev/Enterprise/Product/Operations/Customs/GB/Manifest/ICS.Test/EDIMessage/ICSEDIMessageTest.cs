using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	[TestedType(typeof(IcsSsGreatBritainEDIMessage))]
	class IcsSsGreatBritainEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<IcsSsGreatBritainEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.GbMessageICSGreatBritain, message.EM_ApplicationCode);
		}
	}

	[TestedType(typeof(IcsNorthernIrelandEDIMessage))]
	class IcsNorthernIrelandEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<IcsNorthernIrelandEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland, message.EM_ApplicationCode);
		}
	}

	class GBICSEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestLoadSS()
		{
			RunTypeTest<IcsSsGreatBritainEDIMessage>(EDIMessage.ApplicationCodes.GbMessageICSGreatBritain);
		}

		public void TestLoadNI()
		{
			RunTypeTest<IcsNorthernIrelandEDIMessage>(EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland);
		}

		void RunTypeTest<T>(string appCode)
			where T : EDIMessage
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = appCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			EDIMessage messageReloaded = newFactory.Load<GB.Business.Declaration.GbEDIMessage>(message.PK);
			AssertType("Type decision using express type decision (GbEDIMessage)", typeof(T), messageReloaded);
			messageReloaded = newFactory.Load<EDIMessage>(message.PK);
			AssertType("Type decision using delegated type decision (base EDIMessage)", typeof(T), messageReloaded);
		}
	}
}
