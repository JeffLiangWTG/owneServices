using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class OMANLineTest : MessageLineTestCase
	{
		[TestDate(2004, 6, 12)]
		public void TestLineAsString()
		{
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
		}

		[TestDate(2004, 6, 12)]
		public void TestLineAsStringWhenPortCodesAreEmpty()
		{
			Consol.JK_RL_NKLoadPort = "";
			Consol.JK_RL_NKDischargePort = "";
			AssertEquals(ExpectedLineAsStringWhenPortCodesAreEmpty, Line.LineAsString);
		}

		[TestDate(2004, 6, 12)]
		public void TestLineAsString_ConsolReferenceIsTrimmed()
		{
			Consol.JK_UniqueConsignRef = "12345678901234567890";
			AssertEquals(TestLineAsStringWhenConsolReferenceIsTrimmed, Line.LineAsString);
		}

		[TestDate(2004, 6, 12)]
		public void TestLineAsString_ShouldNotThrowExceptionIfTransportDoesNotExist()
		{
			Consol.Transports.RemoveAll();
			AssertEquals(TestLineAsStringWhenTransportDoesNotExist, Line.LineAsString);
		}

		public void TestPortNameMaxLenght()
		{
			string errorMessage = "Port Name should never exceed the JAS max length, if the schema is changed, then the PortName fields need to be trimmed";
			Assert(errorMessage, RefUNLOCOSchema.RL_PortName.MaxLength <= JXCConstants.OMANFieldBoundaries.PortNameMaxLength);
		}

		public void TestVesselNameMaxLength()
		{
			string errorMessage = "Vessel Name should never exceed the JAS max length, if the schema is changed, then the VesselName fields need to be trimmed";
			Assert(errorMessage, JobConsolTransportSchema.JW_Vessel.MaxLength <= JXCConstants.OMANFieldBoundaries.VesselNameMaxLength);
		}

		#region Implementation
		protected override int ExpectedFieldCount
		{
			get
			{
				return 13;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "OMAN";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new OMANLine(Consol);
		}

		#region Data for Test
		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
					PopulateConsolData();
				}

				return fConsol;
			}
		}

		void PopulateConsolData()
		{
			PopulateConsolReference();
			PopulatePortOfLoadingAndDischarge();
			PopulateRelevantDates();
			PopulateVesselAndVoyageData();
		}

		void PopulateConsolReference()
		{
			Consol.JK_UniqueConsignRef = "CO101";
		}

		void PopulateVesselAndVoyageData()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_VoyageFlight = "V123";
			transport.JW_Vessel = "RAGNAROK";
		}

		void PopulatePortOfLoadingAndDischarge()
		{
			Consol.JK_RL_NKLoadPort = "AUBNE";
			Consol.JK_RL_NKDischargePort = "ITMIL";
		}

		void PopulateRelevantDates()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2004, 06, 11);
			transport.JW_ETA = new ZDateTime(2004, 07, 3);
		}

		JASForwardingConsol fConsol;
		#endregion
		#region ExpectedLineAsString
		const string ExpectedLineAsString = "OMAN3100;N;CO101;RAGNAROK;V123;Brisbane;Milano;AUBNE;AUBNE;ITMIL;ITMIL;12/06/2004;11/06/2004;03/07/2004";
		const string TestLineAsStringWhenConsolReferenceIsTrimmed = "OMAN3100;N;12345678901234567890;RAGNAROK;V123;Brisbane;Milano;AUBNE;AUBNE;ITMIL;ITMIL;12/06/2004;11/06/2004;03/07/2004";
		const string ExpectedLineAsStringWhenPortCodesAreEmpty = "OMAN3100;N;CO101;RAGNAROK;V123;;;;;;;12/06/2004;11/06/2004;03/07/2004";
		const string TestLineAsStringWhenTransportDoesNotExist = "OMAN3100;N;CO101;;;Brisbane;Milano;AUBNE;AUBNE;ITMIL;ITMIL;12/06/2004;;";
		#endregion
		#endregion
	}
}
