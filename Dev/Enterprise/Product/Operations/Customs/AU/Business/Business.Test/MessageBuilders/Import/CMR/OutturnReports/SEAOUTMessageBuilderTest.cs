using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAOUTMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEM_MessageType()
		{
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.SEAOUT, Builder.EM_MessageType);
		}

		public void TestTypeOfMessage()
		{
			AssertEquals("TypeOfMessage", typeof(CMRSEAOUTMessage), Builder.TypeOfMessage);
		}

		public void TestResponsiblePartyID()
		{
			ReportHeader.ResponsiblePartyID = "123456";
			AssertMessageContains("NAD+VW+123456::95'");
		}

		public void TestResponsiblePartyIDDefault()
		{
			ReportHeader.ResponsiblePartyID = ZString.Empty;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "123 456 789 AB";
			AssertMessageContains("NAD+VW+123456789AB::95'");

			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "short";
			AssertEquals("no id when abn invalid", false, Builder.MessageText.Contains("NAD+VW"));
		}

		public void TestResponsiblePartyIDNotSentInWithdrawMessage()
		{
			var seaBuilder = Builder;
			seaBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			ReportHeader.ResponsiblePartyID = "123 123 123 123 12";
			AssertEquals("no id when withdraw message", false, seaBuilder.MessageText.Contains("NAD+VW"));
		}

		public void TestTransportDetails()
		{
			AssertMessageContains("TDT+20+123++11++++8811924::11'");
		}

		public void TestOutturnEstablishment()
		{
			ReportHeader.EstablishmentID = "12345";
			AssertMessageContains("LOC+4+12345::95'");
		}

		public void TestSplitMessageBGMSegment()
		{
			ReportHeader.ResponsiblePartyID = "41065894724";
			ReportHeader.VesselID = "4582391";
			ReportHeader.VoyageNumber = "123S";
			ReportHeader.EstablishmentID = "9920A";
			Assert("Split Message should have 'change' BGM segment, even for 'Original' message type'", SplitBuilder.MessageText.Contains("BGM+263:::SEAOUT+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+4"));
		}

		void AssertMessageContains(ZString text)
		{
			var messageText = Builder.MessageText;
			Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
		}

		readonly Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;
		SEAOUTMessageBuilder Builder
		{
			get
			{
				var result = new SEAOUTMessageBuilder(ReportHeader, ReportHeader.Lines, false, false);
				result.MessageSubType = messageSubType;
				result.Messages = new EDIMessageCollection(null, Factory);
				return result;
			}
		}

		SEAOUTMessageBuilder SplitBuilder
		{
			get
			{
				var result = new SEAOUTMessageBuilder(ReportHeader, ReportHeader.Lines, true, true);
				result.MessageSubType = messageSubType;
				result.Messages = new EDIMessageCollection(null, Factory);
				return result;
			}
		}

		TestHelperSeaOutturnReportHeaderInformation reportHeader;
		TestHelperSeaOutturnReportHeaderInformation ReportHeader
		{
			get
			{
				if (reportHeader == null)
				{
					reportHeader = new TestHelperSeaOutturnReportHeaderInformation();
					reportHeader.VoyageNumber = "123";
					reportHeader.VesselID = "8811924";
				}
				return reportHeader;
			}
		}

		sealed class TestHelperSeaOutturnReportHeaderInformation : ISeaOutturnReportHeaderInformation
		{
			public ZString VesselID { get; set; }

			public ZString VoyageNumber { get; set; }

			public IEnumerable<ISeaOutturnReportLineInformation> Lines { get; set; } = System.Array.Empty<ISeaOutturnReportLineInformation>();

			public IEnumerable<ISeaOutturnReportLineInformation> MessageLines { get; set; } = System.Array.Empty<ISeaOutturnReportLineInformation>();

			public IEDIMessageCollectionProvider MessagesProvider => null;

			public ZString ResponsiblePartyID { get; set; }

			public ZString EstablishmentID { get; set; }
		}
	}
}
