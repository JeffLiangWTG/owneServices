using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SupportingDocumentLookups))]
sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		ReferenceDataTestHelper.AssertSupportDocumentTypeCollection(
			() => supportingDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection);
	}

	public void TestOrganisationAddressList()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		AssertType<OrganisationsFindBoxCollection>(supportingDocument.Lookups.OrganizationList);
	}

	public void TestRegistrationCodeList()
	{
		var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		var supportingDocument = entryInstruction.SupportingDocuments.AddNew();

		var (addressAU, addressIN) = RefDataSetupTestHelper.SetupCusCodes(Factory);

		supportingDocument.OrganizationAddress.OrganisationPK = Guid.Empty;
		AssertEquals("OrganizationPK is empty", string.Empty, supportingDocument.Lookups.RegistrationCodeList.CodesAsString);

		supportingDocument.OrganizationAddress.E2_OA_Address = addressAU.PK;
		AssertEquals("Address without any CusCode setup", string.Empty, supportingDocument.Lookups.RegistrationCodeList.CodesAsString);

		supportingDocument.OrganizationAddress.E2_OA_Address = addressIN.PK;
		var regCodeList = supportingDocument.Lookups.RegistrationCodeList;
		AssertNotEquals("Address with CusCode setup, with wrong Order", "ADH, PAS, IEC", regCodeList.CodesAsString);
		AssertEquals("Address with CusCode setup, with right order", "ADH, IEC, PAS", regCodeList.CodesAsString);

		AssertSame("Cached", regCodeList, supportingDocument.Lookups.RegistrationCodeList);

		addressIN.Header.CustomsCodes.AddNew("AEO", "235349", "IN").OK_OA_PremisesAddress = addressIN.PK;
		Factory.Save();
		AssertEquals("Loopkups cache updated after address updated and save", "ADH, AEO, IEC, PAS", supportingDocument.Lookups.RegistrationCodeList.CodesAsString);
	}
}
