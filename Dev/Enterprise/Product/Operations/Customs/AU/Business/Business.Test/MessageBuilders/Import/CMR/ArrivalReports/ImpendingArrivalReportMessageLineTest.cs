using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSREP;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ImpendingArrivalReportMessageLineTest : TestCaseWithFactory
	{
		public void TestGetNewSegmentGroup()
		{
			var message = new CUSREPMessage();
			var firstGroup = (SegmentGroup9)Builder.GetNewSegmentGroup(message);
			var secondGroup = (SegmentGroup9)Builder.GetNewSegmentGroup(message);
			AssertEquals("FirstGroup", message.Group8[0].Group9[0], firstGroup);
			AssertEquals("SecondGroup", message.Group8[0].Group9[1], secondGroup);
		}

		public void TestPopulate()
		{
			var group9 = new SegmentGroup9();
			Builder.Populate(group9, "I");
			var result = group9.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Result", "LOC+60+AUSYD::6'DTM+132:20050530:102'DTM+132:1159:401'NAD+TR+12345::95'NAD+UP+54321::95'STS++Y:63:95'FTX+LIN+I'", result);
		}

		public void TestUniqueIdentifier()
		{
			AssertEquals("UniqueIdentifier", "ARRIVALPORT=AUSYDETA=20050530DISCHARGE=TrueDISCHARGECTO=12345", Builder.UniqueIdentifier);
		}

		ImpendingArrivalReportMessageLine Builder
		{
			get
			{
				var mock = new Mock<IImpendingArrivalReportLineInformation>();
				mock.Setup(m => m.PortOfArrival).Returns((ZString)"AUSYD");
				mock.Setup(m => m.EstimatedDateTimeOfArrivalUTC).Returns(new ZDateTime(2005, 5, 30, 11, 59, 00));
				mock.Setup(m => m.DischargeCTOEstablishmentID).Returns((ZString)"12345");
				mock.Setup(m => m.StevedoreID).Returns((ZString)"54321");
				mock.Setup(m => m.DischargeIndicator).Returns(true);
				return new ImpendingArrivalReportMessageLine(mock.Object);
			}
		}
	}
}
