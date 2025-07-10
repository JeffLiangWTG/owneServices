using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class OrganisationDocumentaryWrapperTest : TestCaseWithFactory
	{
		public void TestOrganisationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var errorCollector = new ErrorCollector();
			var wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals(ZString.Empty, wrapper.OrganisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1111");
			wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals("", wrapper.OrganisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals("", wrapper.OrganisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);
		}

		public void TestOrganisationNumberEoriOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var errorCollector = new ErrorCollector();
			var wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals(ZString.Empty, wrapper.OrganisationNumberEoriOnly);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1111");
			wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals("FR1111", wrapper.OrganisationNumberEoriOnly);
			AssertEquals(0, errorCollector.ErrorCount);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals("", wrapper.OrganisationNumberEoriOnly);
			AssertEquals(0, errorCollector.ErrorCount);
		}

		public void TestOrganisationDocumentaryWrappertTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterDocumentaryAddress.Address1 = "address1";
			declaration.ImporterDocumentaryAddress.City = "city";
			declaration.ImporterDocumentaryAddress.Postcode = "33000";
			declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode = "fr";
			declaration.ImporterDocumentaryAddress.CompanyName = "name";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var errorCollector = new ErrorCollector();
			var wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader);
			AssertEquals("address1", wrapper.Address);
			AssertEquals("city", wrapper.City);
			AssertEquals("33000", wrapper.PostCode);
			AssertEquals("FR", wrapper.CountryCode);
			AssertEquals("name", wrapper.FullName);
			AssertEquals(0, errorCollector.ErrorCount);

			wrapper = new OrganisationDocumentaryWrapper(entryHeader, declaration.ImporterDocumentaryAddress, orgHeader, "QR");
			AssertEquals("QR", wrapper.CountryCode);
		}

		public void TestOrganisationDocumentaryWrapperConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("EntryHeader", () => new OrganisationDocumentaryWrapper(null, Factory.New<FRJobDocAddress>(), Factory.New<OrgHeader>()));
				AssertExceptionThrown<ArgumentNullException>("FRJobDocAddress", () => new OrganisationDocumentaryWrapper(Factory.New<CusEntryHeader>(), null, Factory.New<OrgHeader>()));
			});
		}
	}
}
