using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors.Testing
{
	[TestedType(typeof(NCTSCUSRESV921ESMessageHelper))]
	public class NCTSCUSRESV921ESMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		const string CUSRES_ResponseMessage = @"UNH+07140311062255+CUSRES:1:921:UN:ECS001'BGM+962+@EPRUEBA_CW01+11'NAD+EX+A12345678:167:148'NAD+2+ESA12345678:167:148'DTM+148:1906071403:201'DTM+268:20190905:102'GIS+4:117:148'GIS+24:116::A3'GIS+24:119:A2:1'RFF+ABT:19ES00999910003845'AUT+HDGM4EAWZSTJJ9XQ+LEVA'DTM+204:1906071403:201'UNT+12+07140311062255'";

		public void TestCustomsClearanceCriteria()
		{
			AssertEquals("A3", nctsDepartureAndTIRResponseResponse.CustomsClearanceCriteria);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = CUSRES_ResponseMessage;
			return NCTSCUSRESV921ESMessageHelper.New(testMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusresResponse = Factory.New<EDIMessage>();
			cusresResponse.EM_MessageText = CUSRES_ResponseMessage;
			nctsDepartureAndTIRResponseResponse = NCTSCUSRESV921ESMessageHelper.New(cusresResponse);
		}
		INctsDepartureAndTIRResponseMessageProvider nctsDepartureAndTIRResponseResponse;
	}
}
