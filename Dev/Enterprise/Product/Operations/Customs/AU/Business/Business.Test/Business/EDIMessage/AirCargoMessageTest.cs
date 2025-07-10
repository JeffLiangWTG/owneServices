using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCargoMessage))]
	public class AirCargoMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAirCargoSubMessageTypeList()
		{
			AirCargoMessage message = Factory.New<AirCargoMessage>();
			AssertEquals("Subtypelist", typeof(AirCargoMessage.AirCargoMessageSubTypeList), message.MessageSubTypeListInternal.GetType());
		}

		public void TestMessageSubTypeDescription()
		{
			AirCargoMessage message = Factory.New<AirCargoMessage>();
			message.EM_MessageSubType = AirCargoMessage.MessageSubType.Original;
			AssertEquals("Description for this", AirCargoMessage.MessageSubTypeDescription.Original, message.EM_MessageSubTypeDescription);
		}

		public void TestDefaultValues()
		{
			bool isTestMode = Env.Registry.AUCustomsAirCargoTestMode;
			AirCargoMessage message1 = Factory.New<AirCargoMessage>();
			AssertEquals("TestMessage", isTestMode, message1.EM_IsTestMessage);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			AirCargoMessage result = (AirCargoMessage)base.GetNewBusinessObjectForDeleteTest(factory);
			result.EM_MessageText = AirCargoExampleMessage;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return result;
		}

		#region TestMessages
		protected const string AirCargoExampleMessage = "UNH+<<MSGNO PLACEHOLDER>>+CIREPT:1:0:AC'BTM+30000000000:AW+08100000011:MB'LOC+10+LHR'LOC+8+SYD'SQD+10+24'MEA+PD+04+KG:56.400'NAD+CZ+++DONNELLY MIRRORS+1079 FARRINGTON DR+CHICAGO+IL+60646+US'NAD+CN+++RAINSFORD PTY LTD+450 EUSTON RD+BROOKVALE+NSW+2100+AU'CPI+++P'MON+CV+745.00:USD'UNS+D'SQD+10+21'MEA+PD+04+KG:56.400'IMD++++10 AUTOMOTIVE MIRRORS CHROME COATED'TRD+2+QF 1+40+::USER REF NUMBER'DTM+901+030423++054'LOC+6+LHR'LOC+12+SYD'SQD+10+22'SQD+10+23'UNS+S'UNT+22+<<MSGNO PLACEHOLDER>>'";
		#endregion

	}
}
