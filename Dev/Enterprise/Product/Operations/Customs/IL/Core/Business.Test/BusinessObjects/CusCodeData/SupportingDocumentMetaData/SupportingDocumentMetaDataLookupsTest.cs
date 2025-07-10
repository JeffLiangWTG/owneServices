using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class SupportingDocumentMetaDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetCY_CodeListInHebrew()
		{
			var factory = Factory;

			GlbStaff.CurrentUser.GS_WorkingLanguage = "HE-IL";

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("HE", "Hebrew");
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code830 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "830", ZDateTime.BrettsBirthday, ZDateTime.Today);

			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("3", "Invoice Country", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			var attributeName1Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName: attributeName1, languageCode: "HE", localLanguageName: null, localLanguageDescription: "ארץ חשבון", localLanguageColumnCaption: "");
			helper.CreateCusCodeListAttribute(code830.PK, "3", "", false);

			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			var attributeName2Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName: attributeName2, languageCode: "HE", localLanguageName: null, localLanguageDescription: "לקוח", localLanguageColumnCaption: "");
			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);

			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			var attributeName3Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName: attributeName3, languageCode: "HE", localLanguageName: null, localLanguageDescription: "יצואן", localLanguageColumnCaption: "");
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);

			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MetaDatasMandatory", "MetaDatas Mandatory", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code380.PK, "MetaDatasMandatory", "18", false);

			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var date = ZDateTime.Today.AddMonths(-1);

			var supportingDocument = newFactory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "380";

			AssertEquals(2, supportingDocument.SupportingDocumentMetadataItems.Count);
			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems[0];
			var values = supportingDocumentMetaData.Lookups.CY_CodeList;

			AssertEquals("380 have two CusCodeListAttribute", 2, values.Count);
			Assert("380 contain 14 and 18", values.ContainsCode("14") && values.ContainsCode("18"));
			AssertEquals("18 in Hebrew", "יצואן", values.GetDescriptionFromCode("18"));

			supportingDocument.CSI_Code = "830";
			AssertEquals(1, supportingDocument.SupportingDocumentMetadataItems.Count);
			supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems[0];
			values = supportingDocumentMetaData.Lookups.CY_CodeList;
			AssertEquals("830 have one CusCodeListAttribute", 1, values.Count);
			Assert("830 contain 3", values.ContainsCode("3"));
			AssertEquals("3 in Hebrew", "ארץ חשבון", values.GetDescriptionFromCode("3"));
		}
	}
}
