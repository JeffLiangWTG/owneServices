using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.ExitControl.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class IE583MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE583MessageProvider>
	{
		#region IIE583Header Members
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestCustomsOfficeOfExport()
		{
			report.CER_OfficeOfExport = "IEDUB500";
			AssertEquals("Office of Export Reference Number", "IEDUB500", Provider.CustomsOfficeOfExport);
		}

		public void TestCustomsOfficeOfExitActual()
		{
			report.CER_OfficeOfExit = "IEARK100";
			AssertEquals("Office of Exit Reference Number", "IEARK100", Provider.CustomsOfficeOfExitActual);
		}

		public void TestExitCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CAR000";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CARRIEREORICODE");
			var carrierAddress = carrier.MainAddress;
			carrierAddress.OA_Address1 = "555 Carrier Road";
			carrierAddress.Postcode = "D00 ZZ11";
			carrierAddress.OA_City = "Cork";
			carrierAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			carrierAddress.CompanyName = "Carrier Name";
			header.CXH_OA_Carrier = carrierAddress.PK;

			var exitCarrier = Provider.ExitCarrier;
			AssertEquals("exitCarrier.Id", "IECARRIEREORICODE", exitCarrier.Id);
			AssertEquals("exitCarrier.Name", "Carrier Name", exitCarrier.Name);
			var address = exitCarrier.Address;
			AssertEquals("Address Street", "555 Carrier Road", address.StreetAndNumber);
			AssertEquals("Address Postcode", "D00 ZZ11", address.Postcode);
			AssertEquals("Address City", "Cork", address.City);
			AssertEquals("Address Country", "IE", address.Country);
		}

		public void TestAlternativeEvidence()
		{
			var alternateEvidence1 = report.AlternativeEvidences.AddNew();
			alternateEvidence1.CY_Code = "20";
			var alternateEvidence2 = report.AlternativeEvidences.AddNew();
			alternateEvidence2.CY_Code = "13";
			AssertContainsExactElementsInAnyOrder("AlternativeEvidence", new ZString[] { "20", "13" }, Provider.AlternativeEvidence.Select(x => x.Type));
		}

		public void TestDeclarant()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Declarant";
			org.OH_Code = "DEC555";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "123 Shipping Lane";
			mainAddress.Postcode = "D00 ZZ11";
			mainAddress.OA_City = "Dublin";
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			report.DeclarantOrgPK = org.PK;
			report.DeclarantAddressPK = mainAddress.PK;

			var declarant = Provider.Declarant;
			AssertEquals("Declarant.Id", "IEREG222", declarant.Id);
			AssertEquals("Declarant.Name", "Test Declarant", declarant.Name);
			var address = declarant.Address;
			AssertEquals("Address Street", "123 Shipping Lane", address.StreetAndNumber);
			AssertEquals("Address Postcode", "D00 ZZ11", address.Postcode);
			AssertEquals("Address City", "Dublin", address.City);
			AssertEquals("Address Country", "IE", address.Country);
		}

		public void TestRepresentative()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Test Representative";
			orgContact.OC_Phone = "555-1234";
			orgContact.OC_Email = "rep@example.com";
			var allocation = orgContact.Allocations.AddNew();
			allocation.PC_Type = "CUS";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REPCODE");
			report.CER_DeclarantType = "IND";
			report.RepresentativeOrgPK = org.PK;

			var representative = Provider.Representative;
			AssertEquals("Representative Identification Number", "IEREPCODE", representative.Id);
			AssertEquals("Representative Status", IE.Business.Constants.RepresentationTypeList.Indirect, representative.Status);
			var contact = representative.Contact;
			AssertEquals("Test Representative", contact.Name);
			AssertEquals("555-1234", contact.PhoneNumber);
			AssertEquals("rep@example.com", contact.EmailAddress);
		}
		#endregion

		#region IE583ExportOperation Members
		public void TestEnquiryInformationCode()
		{
			report.CER_EnquiryInformationCode = "E";
			AssertEquals("Enquiry Information Code", "E", Provider.EnquiryInformationCode);
		}

		public void TestExitDate()
		{
			report.CER_DateTime = new ZDateTimeOffset(2022, 11, 15, 14, 45, 36, 345, TimeSpan.Zero);
			AssertEquals("Exit Date", new DateTime(2022, 11, 15, 14, 45, 36, 345), Provider.ExitDate);
		}

		public void TestExitDate_Empty()
		{
			report.CER_DateTime = ZDateTimeOffset.Empty;
			AssertEquals("Exit Date", DateTime.MinValue, Provider.ExitDate);
		}

		public void TestMRN()
		{
			consignment.CXC_MovementReference = "MRN2468";
			AssertEquals("MRN", "MRN2468", Provider.MRN);
		}
		#endregion

		protected override IE583MessageProvider GetProvider() => new IE583MessageProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			(report, consignment, header) = CusExitReportTest.GetNewBusinessObject(Factory);
		}
		CusExitReport report;
		CusExitConsignment consignment;
		CusExitHeader header;
	}
}
