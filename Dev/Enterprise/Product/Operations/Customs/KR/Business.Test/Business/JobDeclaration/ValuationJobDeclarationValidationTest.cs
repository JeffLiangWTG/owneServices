using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed partial class ValuationJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsOffice = "010";
			AssertNoMessageErrors(declaration.JE_CustomsOfficeInfo);
		}

		public void TestJE_CustomsDivision()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.Validation.ValidateJE_CustomsDivision();
			AssertHasMessageErrorContaining(declaration.JE_CustomsDivisionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsDivision = "XX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsDivisionInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsDivision = "20";
			AssertNoMessageErrors(declaration.JE_CustomsDivisionInfo);
		}

		public void TestJE_AuthorJobTitle()
		{
			declaration.Validation.ValidateJE_AuthorJobTitle();
			AssertHasMessageErrorContaining(declaration.JE_AuthorJobTitleInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorJobTitle = "실무팀장";
			AssertNoMessageErrors(declaration.JE_AuthorJobTitleInfo);
		}

		public void TestJE_AuthorName()
		{
			declaration.Validation.ValidateJE_AuthorName();
			AssertHasMessageErrorContaining(declaration.JE_AuthorNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorName = "실무자";
			AssertNoMessageErrors(declaration.JE_AuthorNameInfo);
		}

		public void TestJE_AuthorPhone()
		{
			declaration.Validation.ValidateJE_AuthorPhone();
			AssertHasMessageErrorContaining(declaration.JE_AuthorPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorPhone = "+82-123-4567";
			AssertNoMessageErrors(declaration.JE_AuthorPhoneInfo);
		}

		public void TestJE_AuditorJobTitle()
		{
			declaration.Validation.ValidateJE_AuditorJobTitle();
			AssertHasMessageErrorContaining(declaration.JE_AuditorJobTitleInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorJobTitle = "책임팀장";
			AssertNoMessageErrors(declaration.JE_AuditorJobTitleInfo);
		}

		public void TestJE_AuditorName()
		{
			declaration.Validation.ValidateJE_AuditorName();
			AssertHasMessageErrorContaining(declaration.JE_AuditorNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorName = "책임자";
			AssertNoMessageErrors(declaration.JE_AuditorNameInfo);
		}

		public void TestJE_AuditorPhone()
		{
			declaration.Validation.ValidateJE_AuditorPhone();
			AssertHasMessageErrorContaining(declaration.JE_AuditorPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorPhone = "+82-123-4567";
			AssertNoMessageErrors(declaration.JE_AuditorPhoneInfo);
		}

		public void TestJE_OH_DutyPayer()
		{
			var emptyBusinessOrgHeader = CreateOrganizationData("RK1", "", "", "", "");
			var emptyPersonOrgHeader = CreateOrganizationData("RK2", "", "", "", "", "NAT");
			var businessOrgHeader = CreateOrganizationData("RK3", "(주)레디코리아", "주소1", "주소2", "대표자명");
			TestOrgDataSetUpHelper.AddCustomsCode(businessOrgHeader, new IDNumberAndType[] { new IDNumberAndType() { Type = "GBR", Number = "사업자등록번호" } });
			var personOrgHeader = CreateOrganizationData("RK4", "(주)레디코리아", "주소1", "주소2", "대표자명", "NAT");
			TestOrgDataSetUpHelper.AddCustomsCode(personOrgHeader, new IDNumberAndType[] { new IDNumberAndType() { Type = "01", Number = "주민등록번호" } });

			declaration.JE_OH_DutyPayer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_DutyPayer = emptyBusinessOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_OH_DutyPayer = emptyPersonOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Identification ID for this organization. Please press F3 here and add a number of type '01', 'PAS', '03', '05' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_OH_DutyPayer = businessOrgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_DutyPayerInfo);

			declaration.JE_OH_DutyPayer = personOrgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_DutyPayerInfo);
		}

		public void TestJE_OH_Importer()
		{
			var emptyOrgHeader = CreateOrganizationData("RK1", "", "", "", "");
			var orgHeader = CreateOrganizationData("RK2", "(주)레디코리아", "주소1", "주소2", "대표자명");
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Importer = emptyOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, "The address of this company is missing. Press F3 here and enter the address 1, 2.");

			declaration.JE_OH_Importer = orgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_ImporterInfo);
		}

		public void TestJE_OH_Supplier()
		{
			var emptyOrgHeader = CreateOrganizationData("RK1", "", "", "", "", "");
			var orgHeader = CreateOrganizationData("RK2", "(주)레디코리아", "주소1", "주소2", "대표자명");
			orgHeader.MainAddress.OA_RN_NKCountryCode = "KR";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Supplier = emptyOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "There is no 'Country Code' for this organization. Please press F3 here and add Country/Region on the Organization form.");
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The address of this company is missing. Press F3 here and enter the address 1, 2.");

			declaration.JE_OH_Supplier = orgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_SupplierInfo);
		}

		public void TestJE_ContainerMode()
		{
			declaration.JE_ContainerMode = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_ContainerModeInfo);
		}

		OrgHeader CreateOrganizationData(string code, string companyName, string address1, string address2, string ceoName, string category = "BUS")
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, category, code, companyName);
			TestOrgDataSetUpHelper.AddOrgAddress(orgHeader.MainAddress, address1, address2);
			TestOrgDataSetUpHelper.AddOrgContact(orgHeader, ceoName, true);

			return orgHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
		}
		new JobDeclaration declaration => (JobDeclaration)base.declaration;
	}
}
