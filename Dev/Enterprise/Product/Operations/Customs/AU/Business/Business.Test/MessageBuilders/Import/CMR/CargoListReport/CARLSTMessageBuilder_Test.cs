using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CARLSTMessageBuilder_Test : TestCaseWithFactory
	{
		public void TestUNHSegment()
		{
			AssertMessageContains("UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'");
		}

		public void TestBGMSegment()
		{
			AssertMessageContains("BGM+259:::CARLST+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'");
		}

		public void TestNADSegment()
		{
			AssertMessageContains("NAD+GC+12345::95'");
		}

		public void TestTDTSegment()
		{
			AssertMessageContains("TDT+20+123++11++++12345678::11'");
		}

		public void TestLOCSegment()
		{
			AssertMessageContains("LOC+12+AUSYD::6'");
		}

		public void TestLineDetailsContained()
		{
			AssertMessageContains("CNI++:::I'");
		}

		public void TestUNTSegment()
		{
			AssertMessageContains("UNT+7+<<MSGNO PLACEHOLDER>>'");
		}

		public void TestDocumentName()
		{
			AssertEquals("DocumentName", "CARLST", Builder.DocumentName);
		}

		public void TestDocumentNameCode()
		{
			AssertEquals("DocumentNameCode", DocumentNameCodeList.CargoMovementEventLog, Builder.DocumentNameCode);
		}

		public void TestEM_MessageType()
		{
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.CARLST, Builder.EM_MessageType);
		}

		public void TestTypeOfMessage()
		{
			AssertEquals("TypeOfMessage", typeof(CMRCARLSTMessage), Builder.TypeOfMessage);
		}

		Mock<ICargoListReportHeader> headerMock;
		Mock<ICargoListReportHeader> HeaderMock
		{
			get
			{
				if (headerMock == null)
				{
					headerMock = new Mock<ICargoListReportHeader>();
					HeaderMock.Setup(m => m.CargoResponsiblePartyID).Returns(new ZString("12345"));
					HeaderMock.Setup(m => m.LloydsNumber).Returns(new ZString("12345678"));
					HeaderMock.Setup(m => m.VoyageNumber).Returns(new ZString("123"));
					HeaderMock.Setup(m => m.DischargePort).Returns(new ZString("AUSYD"));
					HeaderMock.Setup(m => m.Lines).Returns(new[] { LineMock.Object });
				}
				return headerMock;
			}
		}

		Mock<ICargoListReportLine> lineMock;
		Mock<ICargoListReportLine> LineMock
		{
			get
			{
				if (lineMock == null)
				{
					lineMock = new Mock<ICargoListReportLine>();
					lineMock.Setup(m => m.CargoCode).Returns(ZString.Empty);
					lineMock.Setup(m => m.CargoIdentifier).Returns(ZString.Empty);
					lineMock.Setup(m => m.PortOfDestination).Returns(ZString.Empty);
					lineMock.Setup(m => m.PortOfLoading).Returns(ZString.Empty);
					lineMock.Setup(m => m.ImportCargoType).Returns(ZString.Empty);
					lineMock.Setup(m => m.NumberOfPackages).Returns(ZInt.Zero);
					lineMock.Setup(m => m.PackageType).Returns(ZString.Empty);
				}
				return lineMock;
			}
		}

		void AssertMessageContains(ZString text)
		{
			var messageText = Builder.MessageText;
			Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
		}

		CARLSTMessageBuilder Builder
		{
			get
			{
				var builder = new CARLSTMessageBuilder(HeaderMock.Object)
				{
					MessageSubType = messageSubType,
					Messages = new EDIMessageCollection(Factory.New<DummyBusinessObject>(), Factory)
				};
				return builder;
			}
		}

		readonly Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;
	}
}
