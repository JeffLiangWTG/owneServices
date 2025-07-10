using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ShipnetFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestGetCorrectRowsForHeaderCode()
		{
			AssertRowType(typeof(FileHeaderDataRow), "HDR");
			AssertRowType(typeof(BookingHeaderDataRow), "BHD");
			AssertRowType(typeof(PartyDetailsDataRow), "SPR");
			AssertRowType(typeof(PartyDetailsDataRow), "CNE");
			AssertRowType(typeof(PartyDetailsDataRow), "NP1");
			AssertRowType(typeof(PartyDetailsDataRow), "FWD");
			AssertRowType(typeof(PartyDetailsDataRow), "CUS");
			AssertRowType(typeof(UltimateConsigneeDataRow), "UCN");
			AssertRowType(typeof(CargoGroupDataRow), "CGR");
			AssertRowType(typeof(CargoDescriptionsDataRow), "CGD");
			AssertRowType(typeof(CargoDescriptionsDataRow), "CID");
			AssertRowType(typeof(CargoGroupItemsDataRow), "CIT");
			AssertRowType(typeof(EquipmentDetailsDataRow), "EQD");
			AssertRowType(typeof(ChargeDetailDataRow), "CHL");
			AssertRowType(typeof(SummaryDataRow), "CNT");

			AssertNull(format.ConvertToRow("FOO"));
		}

		protected override FlatFileFormat GetFlatFileFormat() => format;

		protected override void SetUp()
		{
			base.SetUp();
			format = new ShipnetFlatFileFormat();
		}
		ShipnetFlatFileFormat format;

		void AssertRowType(Type rowType, ZString headerCode)
		{
			AssertEquals(rowType, format.ConvertToRow(headerCode).GetType());
		}
	}
}
