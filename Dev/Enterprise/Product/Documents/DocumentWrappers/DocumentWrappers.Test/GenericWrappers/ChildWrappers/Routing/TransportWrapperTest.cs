using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(TransportWrapper))]
	sealed class TransportWrapperTest : GenericWrapperTest
	{
		readonly ZString air = TransportModeList.Codes.Airfreight;
		readonly ZString sea = TransportModeList.Codes.Seafreight;
		readonly ZString road = TransportModeList.Codes.Road;
		readonly ZString rail = TransportModeList.Codes.Rail;

		readonly ZDateTime date0 = ZDateTime.Empty;
		readonly ZDateTime date1 = new ZDateTime(2006, 1, 1);
		readonly ZDateTime date2 = new ZDateTime(2006, 1, 2);
		readonly ZDateTime date3 = new ZDateTime(2006, 1, 3);
		readonly ZDateTime date4 = new ZDateTime(2006, 1, 4);

		public void TestWrapperMappingsAirInCzech()
		{
			var czechCulture = Culture.GetCultureForLanguage(Enterprise.Core.Constants.Languages.Czech);
			using (Culture.SetTemporarily(czechCulture))
			{
				var wrapperAir = new TransportWrapper(air, "", "QF253", date3, date2, Factory);
				AssertEquals("Flight / Date", wrapperAir.ReferenceLabel);
				var expectedDepartureDateFormatted = date2.ToString("dd-MMM", czechCulture);
				AssertEquals($"QF253 / {expectedDepartureDateFormatted}", wrapperAir.Reference);
			}
		}

		public void TestWrapperMappingsAirInFrench()
		{
			var originalCulture = Culture.Current;
			try
			{
				var czechCulture = Culture.GetCultureForLanguage(Enterprise.Core.Constants.Languages.French);
				Culture.Set(czechCulture);

				var wrapperAir = new TransportWrapper(air, "", "QF253", date3, date2, Factory);
				AssertEquals("Flight / Date", wrapperAir.ReferenceLabel);
				AssertEquals("QF253 / 02-janv.", wrapperAir.Reference);
			}
			finally
			{
				Culture.Set(originalCulture);
			}
		}

		public override void TestWrapperMappingsEmpty()
		{
			TransportWrapper wrapperEmpty = new TransportWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
			AssertEquals(ZString.Empty, wrapperEmpty.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperEmpty.FlightDate);
			AssertEquals(ZString.Empty, wrapperEmpty.FlightNo);
			AssertEquals(ZString.Empty, wrapperEmpty.LloydsNo);
			AssertEquals(ZString.Empty, wrapperEmpty.Reference);
			AssertEquals(ZString.Empty, wrapperEmpty.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperEmpty.VesselName);
			AssertEquals(ZString.Empty, wrapperEmpty.VoyageNo);
		}

		public void TestWrapperMappingsAir()
		{
			TransportWrapper wrapperAir = new TransportWrapper(air, "", "QF253", date3, date2, Factory);
			AssertEquals("AIR - Air Freight", wrapperAir.Mode.ToString());
			AssertEquals(date2, wrapperAir.FlightDate);
			AssertEquals("QF253", wrapperAir.FlightNo);
			AssertEquals(ZString.Empty, wrapperAir.LloydsNo);
			AssertEquals("QF253 / 02-Jan", wrapperAir.Reference);
			AssertEquals("Flight / Date", wrapperAir.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperAir.VesselName);
			AssertEquals(ZString.Empty, wrapperAir.VoyageNo);
			AssertEquals("QF253", wrapperAir.VoyageFlight);
		}

		public void TestWrapperMappingsSea()
		{
			RefVessel testSeaVessel = Factory.New<RefVessel>();
			testSeaVessel.RV_Code = "JOLLY LODGER";
			testSeaVessel.RV_LloydsNumber = "ALLOYDS";

			TransportWrapper wrapperSea = new TransportWrapper(sea, testSeaVessel.RV_Code, "454", date1, date2, Factory);
			AssertEquals("SEA - Sea Freight", wrapperSea.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperSea.FlightDate);
			AssertEquals(ZString.Empty, wrapperSea.FlightNo);
			AssertEquals("ALLOYDS", wrapperSea.LloydsNo);
			AssertEquals("JOLLY LODGER / 454 / ALLOYDS", wrapperSea.Reference);
			AssertEquals("Vessel / Voyage / IMO(Lloyds)", wrapperSea.ReferenceLabel);
			AssertEquals("JOLLY LODGER", wrapperSea.VesselName);
			AssertEquals("454", wrapperSea.VoyageNo);
			AssertEquals(date2, wrapperSea.VoyageDate);
			AssertEquals("454", wrapperSea.VoyageFlight);
		}

		public void TestWrapperMappingsRoad()
		{
			TransportWrapper wrapperRoad = new TransportWrapper(road, "ZXZ-543", "SYD-MEL", date0, date4, Factory);
			AssertEquals("ROA - Road", wrapperRoad.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperRoad.FlightDate);
			AssertEquals(ZString.Empty, wrapperRoad.FlightNo);
			AssertEquals(ZString.Empty, wrapperRoad.LloydsNo);
			AssertEquals("ZXZ-543 / SYD-MEL / 04-Jan", wrapperRoad.Reference);
			AssertEquals("Road Reference", wrapperRoad.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperRoad.VesselName);
			AssertEquals(ZString.Empty, wrapperRoad.VoyageNo);
			AssertEquals("SYD-MEL", wrapperRoad.VoyageFlight);
		}

		public void TestWrapperMappingsRail()
		{
			TransportWrapper wrapperRail = new TransportWrapper(rail, "WAGON 87", "V78332", date1, date0, Factory);
			AssertEquals("RAI - Rail", wrapperRail.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperRail.FlightDate);
			AssertEquals(ZString.Empty, wrapperRail.FlightNo);
			AssertEquals(ZString.Empty, wrapperRail.LloydsNo);
			AssertEquals("WAGON 87 / V78332 / 01-Jan", wrapperRail.Reference);
			AssertEquals("Rail Reference", wrapperRail.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperRail.VesselName);
			AssertEquals(ZString.Empty, wrapperRail.VoyageNo);
			AssertEquals("V78332", wrapperRail.VoyageFlight);
		}

		public void TestWrapperMappingsRoad_ShowVesseVoyage()
		{
			TransportWrapper wrapperRoad = new TransportWrapper(road, "ZXZ-543", "SYD-MEL", date0, date4, Factory, true);
			AssertEquals("ROA - Road", wrapperRoad.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperRoad.FlightDate);
			AssertEquals(ZString.Empty, wrapperRoad.FlightNo);
			AssertEquals(ZString.Empty, wrapperRoad.LloydsNo);
			AssertEquals("ZXZ-543 / SYD-MEL / 04-Jan", wrapperRoad.Reference);
			AssertEquals("Vessel / Voyage", wrapperRoad.ReferenceLabel);
			AssertEquals("ZXZ-543", wrapperRoad.VesselName);
			AssertEquals("SYD-MEL", wrapperRoad.VoyageNo);
			AssertEquals("SYD-MEL", wrapperRoad.VoyageFlight);
		}

		public void TestWrapperMappingsUnknown()
		{
			TransportWrapper wrapperUnknown = new TransportWrapper("UNKNOWN", ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
			AssertEquals("UNKNOWN", wrapperUnknown.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperUnknown.FlightDate);
			AssertEquals(ZString.Empty, wrapperUnknown.FlightNo);
			AssertEquals(ZString.Empty, wrapperUnknown.LloydsNo);
			AssertEquals(ZString.Empty, wrapperUnknown.Reference);
			AssertEquals(ZString.Empty, wrapperUnknown.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperUnknown.VesselName);
			AssertEquals(ZString.Empty, wrapperUnknown.VoyageNo);
			AssertEquals(ZString.Empty, wrapperUnknown.VoyageFlight);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Transport                                   (Default Field: Reference)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Mode                                    CodeAndDescription
FlightDate                              DateTime
FlightNo                                String
LloydsNo                                String
Reference                               String
ReferenceLabel                          String
VesselName                              String
VoyageDate                              DateTime
VoyageFlight                            String
VoyageNo                                String";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Mode : AIR - Air Freight
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new TransportWrapper(air, "", "QF253", date3, date2, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new TransportWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}
	}
}
