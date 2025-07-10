using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Business.Testing
{
	[TestedType(typeof(TraxonInterchange))]
	class TraxonInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageNumbersAreRetrievedFromCommonAccessReferece()
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNA:+.? 'UNB+IATA:1+RHKAPT01HKGSTCR:RHKAGT801332881/HKG82:PIMA+110909:1646+2716+0'UNH+HMF8903429X057+CIMFMA:0+500'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'UNZ+1+2716'", EDIInterchange.ApplicationCodes.Traxon);
			AssertEquals(typeof(TraxonInterchange), interchange.GetType());

			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals("500", interchange.ContainedMessages[0].EM_MessageNum);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}
	}
}
