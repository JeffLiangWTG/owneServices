using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	class GbCDSExportHeaderTest : TestCaseWithFactory
	{
		public void TestJobType()
		{
			AssertEquals("E", header.JobType);
		}

		public void TestHasDeclarationUCRPartSuffix()
		{
			entryHeader.CH_BGMReference = "UCR1234";
			AssertEquals(expected: false, header.HasDeclarationUCRPartSuffix);
			entryHeader.CH_BGMReference = "UCR1234/1";
			AssertEquals(expected: true, header.HasDeclarationUCRPartSuffix);
		}

		public void TestMasterOpt()
		{
			AssertEquals(ZString.Empty, header.MasterOpt);
		}

		public void TestMovementReference()
		{
			declaration.ZG_LCPInspect = ZDateTime.Empty;
			AssertEquals(ZString.Empty, header.MovementReference);
			declaration.ZG_LCPInspect = new ZDateTime(2025, 5, 7, 15, 52, 12);
			AssertEquals("07May1552", header.MovementReference);
		}

		protected void TestCustomer()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, Core.Constants.CountryCodes.UnitedKingdom);
			supplier.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, "A12345678", Core.Constants.CountryCodes.UnitedKingdom);
			supplier.OH_Code = "C0DE1";
			supplier.OH_FullName = "Test Supplier";
			supplier.MainAddress.Address1 = "1 Street";
			supplier.MainAddress.Address2 = "The Village";
			supplier.MainAddress.City = "The City";
			supplier.MainAddress.State = "Perturbed";
			supplier.MainAddress.Postcode = "N1 1ZZ";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Finland;
			supplier.MainAddress.OA_Phone_Formatted = "+358123456";
			supplier.MainAddress.OA_Fax_Formatted = "+358987654";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			AssertEquals("CustomerAccountNumber", "A12345678", header.CustomerAccountNumber);
			AssertEquals("C0DE1", header.CustomerShortCode);
			AssertEquals("Test Supplier", header.CustomerName);
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
			AssertType<GbCDSExportLine>(line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			header = new GbCDSExportHeader(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		GbCDSExportHeader header;
	}
}
