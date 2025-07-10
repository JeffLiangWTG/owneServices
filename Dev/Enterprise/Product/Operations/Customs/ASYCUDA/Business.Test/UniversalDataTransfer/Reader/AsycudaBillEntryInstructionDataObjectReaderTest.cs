using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaBillEntryInstructionDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaBillEntryInstructionData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var bill = header.Bills.AddNew();
			var usAirLocalPort1 = help.GetAirLocalPort1("SG");
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var billIssuerCode1 = "OTD1";
			var usFirstArrival = new UNLOCO() { Code = usAirLocalPort1.RL_Code };
			var zaFirstArrival = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var billCountryEntryHeader1 = help.SetupCountryBillEntryHeader("SG", 1, "CLR", "Sender Reference1");
			var billCountryEntryInstruction1 = help.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", "SG", billIssuerCode1, 200.01m, 300.01m, "Q", "A", "TEST", new ZDateTime(2018, 6, 6), "6");
			var countryBO = new AsycudaBillEntryHeaderDataObjectReader(billCountryEntryHeader1, Logger, Factory, bill, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(countryBO);
			var countryEntryInsitructionBO1 = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)new AsycudaBillEntryInstructionDataObjectReader(billCountryEntryInstruction1, Logger, Factory, bill, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(countryEntryInsitructionBO1);
			AssertEquals(countryBO.PK, countryEntryInsitructionBO1.PK);
			AssertEquals("countryEntryInsitructionBO1.ABL_LocationInformation", "Location Information1", countryEntryInsitructionBO1.ABL_LocationInformation);
			AssertEquals("countryEntryInsitructionBO1.ABL_GoodsLocation", "GOODS LOCATION1", countryEntryInsitructionBO1.ABL_GoodsLocation);
			AssertEquals("countryEntryInsitructionBO1.ABL_ShipmentType", "IMP", countryEntryInsitructionBO1.ABL_ShipmentType);
			AssertEquals("countryEntryInsitructionBO1.ABL_BillIssuer", billIssuerCode1, countryEntryInsitructionBO1.ABL_BillIssuer);
			AssertEquals("countryEntryInsitructionBO1.DutyAmount", 200.01m, countryEntryInsitructionBO1.DutyAmount);
			AssertEquals("countryEntryInsitructionBO1.TaxAmount", 300.01m, countryEntryInsitructionBO1.TaxAmount);
			AssertEquals("countryEntryInsitructionBO1.SG_PartyID", "TEST", countryEntryInsitructionBO1.SG_PartyID);
			AssertEquals("countryEntryInsitructionBO1.SG_PayeeIndicator", "Q", countryEntryInsitructionBO1.SG_PayeeIndicator);
			AssertEquals("countryEntryInsitructionBO1.SG_PartyStatus", "A", countryEntryInsitructionBO1.SG_PartyStatus);
			AssertEquals("countryEntryInsitructionBO1.CycleDate", new ZDateTime(2018, 6, 6), countryEntryInsitructionBO1.CycleDate);
			AssertEquals("countryEntryInsitructionBO1.CycleNumber", "6", countryEntryInsitructionBO1.CycleNumber);
		}
	}
}
