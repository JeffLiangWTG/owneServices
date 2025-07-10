using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaManifestHeaderEntryInstructionDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaManifestHeaderEntryInstructionData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var usAirLocalPort1 = help.GetAirLocalPort1("US");
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var carrierCode1 = "OTD1";
			var usFirstArrival = new UNLOCO() { Code = usAirLocalPort1.RL_Code };
			var zaFirstArrival = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction1 = help.SetupCountryHeaderEntryInstruction(1, usFirstArrival, carrierCode1, "US", "MA1", "NT1");
			headerEntryInstruction1.AddInfoCollection.Add(new AddInfo() { Key = GenAddOnHelper.PlaceOfExitCode, Value = "POE" });
			headerEntryInstruction1.AddInfoCollection.Add(new AddInfo() { Key = "CycleDate", Value = ZDateTime.Today.ToISO8601String() });
			headerEntryInstruction1.AddInfoCollection.Add(new AddInfo() { Key = "CycleNumber", Value = "1" });
			var headerBO = new AsycudaManifestHeaderEntryHeaderDataObjectReader(headerEntryHeader1, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			var countryEntryInsitructionBO1 = new AsycudaManifestHeaderEntryInstructionDataObjectReader(headerEntryInstruction1, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();

			AssertNotNull(countryEntryInsitructionBO1);
			AssertEquals(headerBO.PK, countryEntryInsitructionBO1.PK);
			AssertEquals("countryEntryInsitructionBO1.Header.AMA_ManifestType", "MA1", countryEntryInsitructionBO1.AMA_ManifestType);
			AssertEquals("countryEntryInsitructionBO1.Header.AMA_Nature", "NT1", countryEntryInsitructionBO1.AMA_Nature);
			AssertEquals("countryEntryInsitructionBO1.Header.AMA_DateAtCustomsOffice", ZDateTime.Today, countryEntryInsitructionBO1.AMA_DateAtCustomsOffice);
			AssertEquals("countryEntryInsitructionBO1.Header.AMA_CarrierCode", carrierCode1, countryEntryInsitructionBO1.AMA_CarrierCode);
		}
	}
}
