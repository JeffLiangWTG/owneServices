using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class UnitMeasurementTextOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitMeasurementList()
		{
			AssertEquals("DT - Decitons\r\nG - Grams\r\nHG - Hectograms\r\nKG - Kilograms\r\nKT - Kilotons\r\nLB - Pounds\r\nLT - Pounds Troy\r\nMC - Metric Carat\r\nMG - Milligrams\r\nOT - Ounces Troy\r\nOZ - Ounces\r\nT - Tonnes\r\nTL - Long Tons (2240 lb)\r\nTN - Short Tons (2000 lb)\r\nCC - Cubic Centimeters\r\nCF - Cubic Feet\r\nCI - Cubic Inches\r\nCY - Cubic Yards\r\nD3 - Cubic Decimeters\r\nGA - US Gallons\r\nGI - Imperial Gallons\r\nL - Liter\r\nM3 - Cubic Meters\r\nML - Mega Liter\r\nTE - Tea Chest\r\nCN - Container\r\nHB - House Bill\r\nLW - Lowest Bill\r\nPK - Package\r\nKM - Kilometer\r\nMI - Mile\r\nHR - Hour\r\nDY - Day\r\nWK - Week\r\nSV - Service Occurrence\r\nTU - Twenty foot equivalent unit\r\nLM - Loading Meter\r\nBAG - Bag\r\nBBG - Bulk Bag\r\nBBK - Break Bulk\r\nBLC - Bale, Compressed\r\nBLU - Bale, Uncompressed\r\nBND - Bundle\r\nBOT - Bottle\r\nBOX - Box\r\nBSK - Basket\r\nCAS - Case\r\nCOI - Coil\r\nCRD - Cradle\r\nCRT - Crate\r\nCTN - Carton\r\nCYL - Cylinder\r\nDOZ - Dozen\r\nDRM - Drum\r\nENV - Envelope\r\nGRS - Gross\r\nKEG - Keg\r\nMIX - Mix\r\nPAI - Pail\r\nPCE - Piece\r\nPKG - Package\r\nPLT - Pallet\r\nREL - Reel\r\nRLL - Roll\r\nROR - Roll-on/roll-off\r\nSHT - Sheet\r\nSKD - Skid\r\nSPL - Spool\r\nTOT - Tote\r\nTUB - Tube\r\nUNT - Unit\r\nPackage - Package\r\nLoading Meters - Loading Meters\r\nUnit - Unit\r\nShipment - Shipment\r\nTEU - TEU", lookups.UnitMeasurementList.ElementsAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new UnitMeasurementTextOverrideLookups();
		}

		UnitMeasurementTextOverrideLookups lookups;
	}
}
