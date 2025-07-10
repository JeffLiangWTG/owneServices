using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestPresenterList()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var collection = manifestHeader.Lookups.PresenterList;

			AssertEquals("ES", collection.FilterBusinessObjectDefaults["Country/Region" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestCertificateNamesList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var unrelatedStaff = Factory.New<GlbStaff>();
			unrelatedStaff.GS_Code = "AP";
			unrelatedStaff.GS_LoginName = "aptest";

			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TESTCERT1";
			cert.GP_MailBoxID = "Test1";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff.PK;

			wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert2 = wrapper.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TESTCERT2";
			cert2.GP_MailBoxID = "Test2";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth2 = cert2.Authorisations.AddNew();
			auth2.GEA_GS_AuthorisedStaff = staff.PK;

			wrapper = ES.Business.GlbStaffWrapper.Get(unrelatedStaff);
			var unrelatedCert = wrapper.ESBPasswordCollection.AddNew();
			unrelatedCert.GP_Name = "TestCertNotLinkedToAH";
			unrelatedCert.GP_MailBoxID = "Test3";
			unrelatedCert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			unrelatedCert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth3 = unrelatedCert.Authorisations.AddNew();
			auth3.GEA_GS_AuthorisedStaff = unrelatedStaff.PK;

			header.AMA_GS_NKCustomsAgent = "AH";
			var list = header.Lookups.CertificateNames.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("Contains only related certificate", new[] { "TESTCERT1", "TESTCERT2" }, list);
		}

		public void TestAgentTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var agentTypeCodes = header.Lookups.AgentTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder("Should contain expected agent types", new[] { "DIR", "IND", "DCA", "ICA", "SEL" }, agentTypeCodes);
		}

		public void TestRegistrationStatusList()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var customCodes = manifestHeader.Lookups.RegistrationStatusList;

			AssertSame("Cached", customCodes, manifestHeader.Lookups.RegistrationStatusList);
			AssertEquals("RegistrationStatusList should be the same as ESH7AISEntryStatusList", new ESH7AISEntryStatusList().CodesAsString, manifestHeader.Lookups.RegistrationStatusList.CodesAsString);
		}

		public void TestTransportDocumentTypes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "TD44G", "444", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var collection = (CodeDescriptionPairList)manifestHeader.Lookups.TransportDocumentTypes;

			Assert("Should include the correct CusCode", collection.ContainsCode("444"));
			Assert("Should not include the incorrect CusCode", !collection.ContainsCode("555"));
		}

		public void TestG3MRNToRevokeList()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var unrelatedManifestHeader = Factory.New<AsycudaManifestHeader>();

			var bill1 = manifestHeader.Bills.AddNew();
			bill1.G3MovementReferenceNumber = "11111";
			bill1.G3RevokedMovementReferenceNumber = "88888";
			bill1.G3LocalReferenceNumber = "77777";
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.G3MovementReferenceNumber = "22222";
			var unrelatedBill = unrelatedManifestHeader.Bills.AddNew();
			unrelatedBill.G3MovementReferenceNumber = "999999";

			Factory.Save();
			var list =  manifestHeader.Lookups.G3MRNToRevokeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("Contains only related MRNs", new[] { "11111", "22222" }, list);
		}
	}
}
