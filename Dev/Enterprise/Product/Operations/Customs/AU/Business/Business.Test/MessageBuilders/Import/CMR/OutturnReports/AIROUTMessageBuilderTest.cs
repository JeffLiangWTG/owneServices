using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIROUTMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEM_MessageType()
		{
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.AIROUT, Builder.EM_MessageType);
		}

		public void TestTypeOfMessage()
		{
			AssertEquals("TypeOfMessage", typeof(CMRAIROUTMessage), Builder.TypeOfMessage);
		}

		public void TestDateTimeOfOutturn()
		{
			ReportHeader.DateTimeOfOutturn = new ZDateTime(2005, 6, 20, 19, 21, 0);
			AssertMessageContains("DTM+570:20050620:102'");
			AssertMessageContains("DTM+570:1921:401'");
		}

		public void TestResponsiblePartyID()
		{
			ReportHeader.ResponsiblePartyID = "123456";
			AssertMessageContains("NAD+VW+123456::95'");
		}

		public void TestTransportDetails()
		{
			ReportHeader.FlightNumber = "QF439";
			AssertMessageContains("TDT+20+439++6+QF::3'");

			ReportHeader.FlightNumber = "5X991";
			AssertMessageContains("TDT+20+991++6+5X::3'");

			ReportHeader.FlightNumber = "QF-12 3";
			AssertMessageContains("TDT+20+123++6+QF::3'");
		}

		public void TestOutturnEstablishment()
		{
			ReportHeader.EstablishmentID = "12345";
			AssertMessageContains("LOC+59+12345::95'");
		}

		public void TestEstimatedDateOfArrival()
		{
			ReportHeader.EstimatedDateOfArrival = new ZDateTime(2005, 1, 27);
			AssertMessageContains("DTM+132:20050127:102'");
		}

		public void TestWithdrawEndToEnd()
		{
			ReportHeader.DateTimeOfOutturn = new ZDateTime(2005, 1, 27);
			ReportHeader.ResponsiblePartyID = "41065894724";
			ReportHeader.FlightNumber = "QF123";
			ReportHeader.EstablishmentID = "9920A";
			ReportHeader.EstimatedDateOfArrival = new ZDateTime(2005, 1, 27);

			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertMultilineEquals("MessageText", @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+263:::AIROUT+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'
DTM+570:20050127:102'
DTM+570:0000:401'
NAD+VW+41065894724::95'
TDT+20+123++6+QF::3'
LOC+59+9920A::95'
DTM+132:20050127:102'
UNT+9+<<MSGNO PLACEHOLDER>>'".Replace("\r\n", ""), Builder.MessageText, '\'');
		}

		public void TestSplitMessageBGMSegment()
		{
			ReportHeader.DateTimeOfOutturn = new ZDateTime(2012, 3, 27);
			ReportHeader.ResponsiblePartyID = "41065894724";
			ReportHeader.FlightNumber = "QF123";
			ReportHeader.EstablishmentID = "9920A";
			ReportHeader.EstimatedDateOfArrival = new ZDateTime(2012, 3, 27);
			Assert("Split Message should have 'change' BGM segment, even for 'Original' message type'", SplitBuilder.MessageText.Contains("BGM+263:::AIROUT+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+4"));
		}

		void AssertMessageContains(ZString text)
		{
			ZString messageText = Builder.MessageText;
			Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
		}

		Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;

		AIROUTMessageBuilder Builder
		{
			get
			{
				var result = new AIROUTMessageBuilder(ReportHeader, ReportHeader.Lines, false, false);
				result.MessageSubType = messageSubType;
				result.Messages = new EDIMessageCollection(null, Factory);
				return result;
			}
		}

		AIROUTMessageBuilder SplitBuilder
		{
			get
			{
				var result = new AIROUTMessageBuilder(ReportHeader, ReportHeader.Lines, true, true);
				result.MessageSubType = messageSubType;
				result.Messages = new EDIMessageCollection(null, Factory);
				return result;
			}
		}

		AirOutturnReportHeaderInformationForTest reportHeader;
		AirOutturnReportHeaderInformationForTest ReportHeader
		{
			get
			{
				if (reportHeader == null)
				{
					reportHeader = new AirOutturnReportHeaderInformationForTest();
					reportHeader.FlightNumber = "QF123";
				}
				return reportHeader;
			}
		}

		sealed class AirOutturnReportHeaderInformationForTest : IAirOutturnReportHeaderInformation
		{
			public ZDateTime DateTimeOfOutturn { get; set; }

			public ZString FlightNumber { get; set; }

			public ZDateTime EstimatedDateOfArrival { get; set; }

			public IAirOutturnReportLineInformation[] Lines { get; set; } = System.Array.Empty<IAirOutturnReportLineInformation>();

			public IAirOutturnReportLineInformation[] DatabaseLines { get; set; } = System.Array.Empty<IAirOutturnReportLineInformation>();

			public ZString ResponsiblePartyID { get; set; }

			public ZString EstablishmentID { get; set; }
		}
	}
}
