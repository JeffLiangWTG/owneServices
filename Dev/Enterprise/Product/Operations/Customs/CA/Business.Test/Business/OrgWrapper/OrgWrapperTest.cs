using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class OrgWrapperTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertNull(OrgWrapper.New(null));
			AssertNotNull(OrgWrapper.New(org));
		}

		public void TestIDLMOrganisationProperties()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "DUMMY TEST COMPANY";
			organisation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "AA1111");
			organisation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RM0001");
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.MainAddress.OA_Address1 = "Address 1";
			organisation.MainAddress.OA_Address2 = "Address 2";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_Phone = "+61 (2) 9025 1100";
			organisation.MainAddress.OA_Fax = "+61 (2) 9025 1199";

			IDLMOrganisation dlmOrganisation = OrgWrapper.New(organisation);
			AssertEquals("AuthorizationId", "AA1111", dlmOrganisation.AuthorizationId);
			AssertEquals("BusinessNumberForImportExport", "123456789RM0001", dlmOrganisation.BusinessNumberForImportExport);
			AssertEquals("CompanyName", "DUMMY TEST COMPANY", dlmOrganisation.CompanyName);
			AssertEquals("Street", "Address 1 Address 2", dlmOrganisation.Street);
			AssertEquals("City", "Sydney", dlmOrganisation.City);
			AssertEquals("ProvinceState", "New South Wales", dlmOrganisation.ProvinceState);
			AssertEquals("Country/Region", "Australia", dlmOrganisation.Country);
			AssertEquals("PostalZipCode", "2000", dlmOrganisation.PostalZipCode);
			AssertEquals("Telephone", "1290251100", dlmOrganisation.Telephone);
			AssertEquals("TelephoneExtension", ZString.Empty, dlmOrganisation.TelephoneExtension);
			AssertEquals("Fax", "1290251199", dlmOrganisation.Fax);
		}

		public void TestBusinessNumberForImportExportWithBusinessNumberForExportOnly()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "DUMMY TEST2 COMPANY";
			organisation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "AA2222");
			organisation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForExport, "123456789RM0002");
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.MainAddress.OA_Address1 = "Address 1";
			organisation.MainAddress.OA_Address2 = "Address 2";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_Phone = "+61 (2) 9025 1100";
			organisation.MainAddress.OA_Fax = "+61 (2) 9025 1199";

			IDLMOrganisation dlmOrganisation = OrgWrapper.New(organisation);
			AssertEquals("BusinessNumberForImportExport", "123456789RM0002", dlmOrganisation.BusinessNumberForImportExport);
		}

		public void TestBusinessNumberForImportExportWithNull()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "DUMMY TEST3 COMPANY";
			organisation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "AA3333");
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.MainAddress.OA_Address1 = "Address 1";
			organisation.MainAddress.OA_Address2 = "Address 2";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_Phone = "+61 (2) 9025 1100";
			organisation.MainAddress.OA_Fax = "+61 (2) 9025 1199";

			IDLMOrganisation dlmOrganisation = OrgWrapper.New(organisation);
			AssertEquals("BusinessNumberForImportExport", ZString.Empty, dlmOrganisation.BusinessNumberForImportExport);
		}
	}
}
