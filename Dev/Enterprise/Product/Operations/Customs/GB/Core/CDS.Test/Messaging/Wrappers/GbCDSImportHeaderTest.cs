using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	class GbCDSImportHeaderTest : TestCaseWithFactory
	{
		public void TestJobType()
		{
			AssertEquals("I", header.JobType);
		}

		public void TestOSAirTransportLoad()
		{
			declaration.JE_IATALoadPort = "FLX";
			AssertEquals("FLX", header.OSAirTransportLoad);
		}

		public void TestCustomer()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, Core.Constants.CountryCodes.UnitedKingdom);
			importer.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, "A12345678", Core.Constants.CountryCodes.UnitedKingdom);
			importer.OH_Code = "C0DE1";
			importer.OH_FullName = "Test Importer";
			importer.MainAddress.Address1 = "1 Street";
			importer.MainAddress.Address2 = "The Village";
			importer.MainAddress.City = "The City";
			importer.MainAddress.State = "Perturbed";
			importer.MainAddress.Postcode = "N1 1ZZ";
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Finland;
			importer.MainAddress.OA_Phone_Formatted = "+358123456";
			importer.MainAddress.OA_Fax_Formatted = "+358987654";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("CustomerAccountNumber", "A12345678", header.CustomerAccountNumber);
			AssertEquals("C0DE1", header.CustomerShortCode);
			AssertEquals("Test Importer", header.CustomerName);
			AssertEquals("1 Street", header.CustomerAddress1);
			AssertEquals("The Village", header.CustomerAddress2);
			AssertEquals("The City", header.City);
			AssertEquals("Perturbed", header.State);
			AssertEquals("N1 1ZZ", header.PostCode);
			AssertEquals(Core.Constants.CountryCodes.Finland, header.CountryCode);
			AssertEquals("+358123456", header.CustomerTelephone);
			AssertEquals("+358987654", header.CustomerFax);
		}

		public void TestGetNewGbLine()
		{
			entryHeader.AllEntryLines.AddNew();
			var line = header.Lines.First();
			AssertType<GbCDSImportLine>(line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			header = new GbCDSImportHeader(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		GbCDSImportHeader header;
	}
}
