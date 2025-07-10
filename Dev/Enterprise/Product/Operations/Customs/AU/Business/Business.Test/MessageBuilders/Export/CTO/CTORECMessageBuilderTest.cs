using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTORECMessageBuilderTest : CTOMessageBuilderTest
	{
		public void TestLOCCountryOfDestination()
		{
			line.CustomsAuthorityNumber = "BLAH";
			line.CountryOfDestination = "NZ";
			AssertLine("LOC+28+NZ::5'", 5);
		}

		public void TestGISOffloadIndicator()
		{
			line.CustomsAuthorityNumber = "BLAH";
			line.OffloadIndicator = true;
			AssertLine("GIS+OFF:109:95'", 5);
		}

		public void TestTDTVoyageNumber()
		{
			line.IsSea = true;
			line.ContainerNumber = "foo";
			line.VoyageNumber = "42S";
			AssertLine("TDT+20+42S'", 5);
		}

		public void TestTDTCarrierPartyID()
		{
			line.IsSea = true;
			line.ContainerNumber = "foo";
			line.CarrierPartyID = "4321";
			AssertLine("TDT+20++++4321::95'", 5);
		}

		public void TestTDTVesselID()
		{
			line.IsSea = true;
			line.ContainerNumber = "foo";
			line.VesselID = "1234567";
			AssertLine("TDT+20+++++++1234567::11'", 5);
		}

		public void TestNADGoodsOwnerPartyID()
		{
			line.GoodsOwnerPartyID = "5555A";
			AssertLine("NAD+GO+5555A::95'", 4);
			AssertLine("GID+1'", 5);
		}

		public void TestNADOwnerName()
		{
			line.OwnerName = "Test Owner";
			AssertLine("NAD+GO+++TEST OWNER'", 4);
			AssertLine("GID+1'", 5);
		}

		public void TestDTMProposedDepartureDate()
		{
			line.CustomsAuthorityNumber = "BLAH";
			line.ProposedDateOfDeparture = new ZDate(2007, 02, 28);
			AssertLine("DTM+133:20070228:102'", 5);
			AssertLine("GID+1'", 6);
		}

		public void TestGIDTrigger()
		{
			line.GoodsDescription = "whee";
			AssertLine("GID+1'", 4);
		}

		public void TestFTXGoodsDescription()
		{
			line.GoodsDescription = "spam, spam, eggs and spam";
			AssertLine("FTX+AAA+++SPAM, SPAM, EGGS AND SPAM'", 5);
		}

		public void TestUNTEnd()
		{
			AssertLine("UNT+6+<<MSGNO PLACEHOLDER>>'", 5);
		}

		public void TestUNTEndWithExtraLine()
		{
			line.CTOEstablishmentID = "foo";
			AssertLine("UNT+7+<<MSGNO PLACEHOLDER>>'", 6);
		}

		public void TestAir()
		{
			line.CTOEstablishmentID = "CTOEST";
			line.IsAir = true;
			line.AirWaybill = "12345";
			line.CarrierPartyID = "SEA ONLY";
			line.ContainerNumber = "SEA ONLY";
			line.CountryOfDestination = "NZ";
			line.CustomsAuthorityNumber = "CANCAN";
			line.GoodsDescription = "GOODSDESC";
			line.GoodsOwnerPartyID = "GOWNER";
			line.NonContainerisedIdentifier = "SEA ONLY";
			line.OffloadIndicator = true;
			line.OwnerName = "OWNER NAME";
			line.ProposedDateOfDeparture = new ZDate(2007, 02, 11);
			line.VesselID = "SEA ONLY";
			line.VoyageNumber = "SEA ONLY";

			var message = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+44:::CTOREC+" + GetReference(0) + @":1'
LOC+202+CTOEST::95'
TDT+20+++6'
CNI+1+:::I'
RFF+AWB:12345'
GID+1'
RFF+TN:CANCAN'
LOC+28+NZ::5'
GIS+OFF:109:95'
NAD+GO+GOWNER::95++OWNER NAME'
DTM+133:20070211:102'
GID+1'
FTX+AAA+++GOODSDESC'
UNT+15+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals("message", message, builder.MessageText.Replace("'", "'\r\n"));
		}

		public void TestAirValidatedOn20070502()
		{
			line.CTOEstablishmentID = "9122P";
			line.IsAir = true;
			line.AirWaybill = "08112345616";
			line.CustomsAuthorityNumber = "AAACFFH9X";
			line.OwnerName = "TEST OWNER";
			line.ProposedDateOfDeparture = new ZDate(2007, 05, 01);

			var message = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+44:::CTOREC+" + GetReference(0) + @":1'
LOC+202+9122P::95'
TDT+20+++6'
CNI+1+:::I'
RFF+AWB:08112345616'
GID+1'
RFF+TN:AAACFFH9X'
NAD+GO+++TEST OWNER'
DTM+133:20070501:102'
GID+1'
UNT+12+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals("message", message, builder.MessageText.Replace("'", "'\r\n"));
		}

		public void TestSea()
		{
			line.IsAir = false;
			line.CTOEstablishmentID = "CTOEST";

			line.IsSea = true;
			line.AirWaybill = "AIR ONLY";
			line.CarrierPartyID = "CARRIERID";
			line.ContainerNumber = "AAAA1111113";
			line.CountryOfDestination = "NZ";
			line.CustomsAuthorityNumber = "CANCAN";
			line.GoodsDescription = "GOODSDESC";
			line.GoodsOwnerPartyID = "GOWNER";
			line.OffloadIndicator = true;
			line.OwnerName = "OWNER NAME";
			line.ProposedDateOfDeparture = new ZDate(2007, 02, 11);
			line.VesselID = "767676";
			line.VoyageNumber = "42S";

			var message = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+44:::CTOREC+" + GetReference(0) + @":1'
LOC+202+CTOEST::95'
TDT+20+++11'
CNI+1+:::I'
RFF+AAQ:AAAA1111113'
GID+1'
RFF+TN:CANCAN'
LOC+28+NZ::5'
GIS+OFF:109:95'
TDT+20+42S+++CARRIERID::95+++767676::11'
NAD+GO+GOWNER::95++OWNER NAME'
DTM+133:20070211:102'
GID+1'
FTX+AAA+++GOODSDESC'
UNT+16+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals("message", message, builder.MessageText.Replace("'", "'\r\n"));
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new CTORECMessageBuilder(line);

		protected override string MessageName => "CTOREC";

		protected override string MessageCode => DocumentNameCodeList.TransportStatusReport;
	}
}
