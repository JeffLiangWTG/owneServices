using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class FlightDetailsLineTest : UPEDataLineTest
	{
		public void TestSetFlightNumber()
		{
			fMapper.SetFlightNumber("QF02938");
			AssertEquals("QF02938", Header.ED_FlightNumber);
		}

		public void TestSetFlightDepartureTimeStamp()
		{
			fMapper.SetFlightDepartureTimeStamp("0410030920");
			AssertEquals(new ZDateTime(2004, 10, 3, 9, 20, 0), Header.ED_DepartureDate);
			Header.ED_DepartureDate = ZDateTime.Empty;
			fMapper.SetFlightDepartureTimeStamp("041003");
			AssertEquals(new ZDateTime(2004, 10, 3), Header.ED_DepartureDate);
		}

		public void TestSetPortOfLoadingAndDischarge()
		{
			InsertRequiredDataToDB();
			fMapper.InternalLine = Line1;
			fMapper.Process();
			AssertEquals(ExpectedUNLOCO1, fMapper.PortOfLoading);
			AssertEquals(ExpectedUNLOCO2, fMapper.PortOfDischarge);
			fMapper.InternalLine = Line2;
			fMapper.Process();
			AssertEquals(ExpectedUNLOCO1, fMapper.PortOfLoading);
			AssertEquals(ExpectedUNLOCO2, fMapper.PortOfDischarge);
			fMapper.fPortOfDischarge = "-";
			fMapper.fPortOfLoading = "-";
			fMapper.Process();
			AssertEquals(ExpectedUNLOCO1, fMapper.PortOfLoading);
			AssertEquals(ZString.Empty, fMapper.PortOfDischarge);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Mapper = new FlightDetailsLine(null, Header, null);
			fMapper = (FlightDetailsLine)Mapper;
		}

		FlightDetailsLine fMapper;
		protected new const string Line1 = "AU0000      04081308143395855               100000 AU9639040813NZ8989QF011       040813                                                                              QF011                                                                                                                                                                                                                ";
		protected new const string Line2 = "AU0000      04091419300638864               100000 AU9639040914      AUSB        040913                                                                              AUSB                                                                                                                                                                                                                 ";
		#endregion
	}
}
