using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsLocation))]
	sealed class DocWhsLocationTest : DocumentWrapperTestCase
	{
		#region Properties

		#region ZString

		public void TestWarehouseName()
		{
			AssertEquals(Location.Row.Warehouse.WW_WarehouseName, DocWrapper.WarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Location.Row.Warehouse;
			AssertEquals("WHS", warehouse.WW_WarehouseName);
			AssertEquals("WarehouseName in English", "WHS", DocWrapper.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "WHS").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库"));
				AssertEquals("WarehouseName in Chinese", "仓库", DocWrapper.WarehouseName);
			}
		}

		public void TestLocationString()
		{
			AssertEquals(Location.ToLocationString(), DocWrapper.LocationString);
		}

		public void TestLocationString_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ABC", 2, 2, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 9, 9, 9);
			var location = row.Locations.Single(loc => loc.WLV_LocationString == "A050505");
			var docWrapper = DocWhsLocation.New(location, Factory);
			AssertNotNull("Wrapper not null", docWrapper);
			AssertEquals("A-05-05-05", docWrapper.LocationString);
		}

		public void TestLocationBarcode()
		{
			TextBarcode barcode = new TextBarcode(Location.ToLocationString());
			AssertEquals(barcode.TextAs128sFontString, DocWrapper.LocationBarcode);
		}

		public void TestLocationBarcode_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ABC", 2, 2, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 9, 9, 9);
			var location = row.Locations.Single(loc => loc.WLV_LocationString == "A050505");
			var docWrapper = DocWhsLocation.New(location, Factory);
			AssertNotNull("Wrapper not null", docWrapper);

			var barcode = new TextBarcode("A050505");
			AssertEquals(barcode.TextAs128sFontString, docWrapper.LocationBarcode);
		}

		public void TestLocationCheckDigitString_Empty()
		{
			TestLocationCheckDigitStringCore(WhsLocation.EmptyCheckDigit, string.Empty);
		}

		public void TestLocationCheckDigitString_Zero()
		{
			TestLocationCheckDigitStringCore(0, "00");
		}

		public void TestLocationCheckDigitString_SingleDigit()
		{
			TestLocationCheckDigitStringCore(5, "05");
		}

		public void TestLocationCheckDigitString_DoubleDigit()
		{
			TestLocationCheckDigitStringCore(15, "15");
		}

		void TestLocationCheckDigitStringCore(ZByte checkDigit, string expectedCheckDigitString)
		{
			var warehouse = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 1);
			var location = row.Locations.Single();
			location.WLV_CheckDigit = checkDigit;

			var docWrapper = DocWhsLocation.New(location, Factory);
			AssertNotNull("Wrapper not null", docWrapper);
			AssertEquals(expectedCheckDigitString, docWrapper.LocationCheckDigitString);
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Helper = new WhsTestHelperFunctionsEnv(Factory);

			var warehouse = Helper.CreateWarehouse("WHS");
			Helper.CreateRowAndGenerateLocations(warehouse, "ROW", 1, 1);
			Location = warehouse.Rows[0].Locations[0];
			DocWrapper = DocWhsLocation.New(Location, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsLocation Location;
		DocWhsLocation DocWrapper;
		WhsTestHelperFunctionsEnv Helper;

		#endregion
	}
}
