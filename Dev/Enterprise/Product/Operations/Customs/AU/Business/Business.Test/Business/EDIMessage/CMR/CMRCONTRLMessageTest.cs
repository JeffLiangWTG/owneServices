using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCONTRLMessage))]
	public class CMRCONTRLMessageTest : CMRIncomingMessageTest
	{
		public void TestIncomingInterchangeCreatesMessageOfCorrectType()
		{
			string incomingInterchangeString = incomingInterchange;
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingInterchangeString);
			AssertEquals("Created Message Type", ExpectedBusinessObjectType, interchange.ContainedMessages[0].GetType());
		}

		public void TestDefaultValues2()
		{
			CMRCONTRLMessage message = Factory.New<CMRCONTRLMessage>();
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.CONTRL, message.EM_MessageType);
		}

		readonly string incomingInterchange = "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAA347M+041210:1319+00000000000001'UNH+000001+CONTRL:D:3:UN'UCI+00000000261664+AAA374M::AAA374M+AAA336C+4+27+UNB'UNT+3+000001'UNZ+1+00000000000001'";

		public void TestGetReport()
		{
			CMRCONTRLMessage message = (CMRCONTRLMessage)EDIInterchange.CreateNewInterchangeFromString(Factory, incomingInterchange).ContainedMessages[0];
			message.EM_Status = EDIMessage.Status.Error;
			AssertEquals("No outgoing report", expectedNoOutgoingReprt, message.GetReport());

			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, iCSInterchange);
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_LinkUniqueID = interchange.PK;
			message.EM_LinkTable = EDIInterchange.Schema.TableName;
			AssertMultilineASCIIEquals("No errors report", expectedNoErrorReport, message.GetReport());

			message.EM_Status = EDIMessage.Status.Error;
			AssertMultilineASCIIEquals("Had errors report", expectedErrorReport, message.GetReport());
		}

		public void TestGetReportWhenNotLinked()
		{
			var message = (CMRCONTRLMessage)EDIInterchange.CreateNewInterchangeFromString(Factory, incomingInterchange).ContainedMessages[0];
			message.EM_Status = EDIMessage.Status.Error;
			AssertEquals("No outgoing report", expectedNoOutgoingReprt, message.GetReport());

			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, iCSInterchange, EDIMessage.ApplicationCodes.CMR);
			message.EM_Status = EDIMessage.Status.Received;
			AssertMultilineASCIIEquals("No errors report", expectedNoErrorReport, message.GetReport());
		}

		readonly ZString expectedNoOutgoingReprt = @"Outgoing interchange not found.

";
		readonly string iCSInterchange = "UNA:+.? 'UNB+UNOC:3+AAA374M::AAA374M+AAA336C+041210:1354+00000000261664++++1++1'UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+CCF_AAA374M_160476_ACR_1:1+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00039433/1::1'DTM+310:20041210025409:204'ERP+1'ERC+CCFERROR:80:95'ERC+573:6:95'FTX+AAO+++The mandatory field TYPEOFPAYMENTIND is missing'CNT+55:1'UNT+13+000001'UNZ+1+00000000261664'";
		readonly ZString expectedNoErrorReport = "UNA:+.? " + @"
UNB+UNOC:3+AAA374M::AAA374M+AAA336C+041210:1354+00000000261664++++1++1 << This level and all lower levels rejected - Security function not supported
UNH+000001+CUSRES:D:99B:UN
BGM+961:::AIRCRR+CCF_AAA374M_160476_ACR_1:1+11
NAD+MR+AAA374M:110:95
RFF+ACW:AIRCR
RFF+AFM:9
RFF+ABO:S00039433/1::1
DTM+310:20041210025409:204
ERP+1
ERC+CCFERROR:80:95
ERC+573:6:95
FTX+AAO+++The mandatory field TYPEOFPAYMENTIND is missing
CNT+55:1
UNT+13+000001
UNZ+1+00000000261664'";
		readonly ZString expectedErrorReport = @"The outgoing interchange had errors:

UNA:+.? " + @"
UNB+UNOC:3+AAA374M::AAA374M+AAA336C+041210:1354+00000000261664++++1++1 << This level and all lower levels rejected - Security function not supported
UNH+000001+CUSRES:D:99B:UN
BGM+961:::AIRCRR+CCF_AAA374M_160476_ACR_1:1+11
NAD+MR+AAA374M:110:95
RFF+ACW:AIRCR
RFF+AFM:9
RFF+ABO:S00039433/1::1
DTM+310:20041210025409:204
ERP+1
ERC+CCFERROR:80:95
ERC+573:6:95
FTX+AAO+++The mandatory field TYPEOFPAYMENTIND is missing
CNT+55:1
UNT+13+000001
UNZ+1+00000000261664'";
	}
}
