using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class SupportingDocumentMetaDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MetaDatasMandatory", "Metadatas Mandatory", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);

			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "MetaDatasMandatory", "18", false);
			factory.Save();

			const string expectedWhenEmpty = "A meta data value is mandatory for this type of supporting document.";
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "380";

			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.AddNew();
			var info = supportingDocumentMetaData.CY_DataInfo;

			supportingDocumentMetaData.CY_Code = "14";
			supportingDocumentMetaData.CY_Data = "Val1";
			AssertNoMessageErrorContaining("Entered if Mandatory is false", info, expectedWhenEmpty);

			supportingDocumentMetaData.CY_Data = ZString.Empty;
			AssertNoMessageErrorContaining("Not entered if Mandatory is false", info, expectedWhenEmpty);

			supportingDocumentMetaData.CY_Code = "18";
			supportingDocumentMetaData.CY_Data = "Val1";
			AssertNoMessageErrorContaining("Entered if Mandatory is true", info, expectedWhenEmpty);

			supportingDocumentMetaData.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining("Not entered if Mandatory is true", info, expectedWhenEmpty);
		}

		public void TestCheckCY_CodeList()
		{
			const string expected = "Enter a valid Code.";

			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("HE", "Hebrew");
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);

			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			var attributeName2Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName: attributeName2, languageCode: "HE", localLanguageName: null, localLanguageDescription: "לקוח", localLanguageColumnCaption: "");
			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);

			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			var attributeName3Language = helper.CreateCusCodeListAttributeNameLanguage(attributeName: attributeName3, languageCode: "HE", localLanguageName: null, localLanguageDescription: "יצואן", localLanguageColumnCaption: "");
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);
			factory.Save();

			var date = ZDateTime.Today.AddMonths(-1);

			var supportingDocument = factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "380";

			AssertEquals(2, supportingDocument.SupportingDocumentMetadataItems.Count);
			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems[0];
			var values = supportingDocumentMetaData.Lookups.CY_CodeList;

			AssertEquals("380 have two CusCodeListAttribute", 2, values.Count);
			Assert("380 contain 14 and 18", values.ContainsCode("14") && values.ContainsCode("18"));

			supportingDocumentMetaData.CY_Code = "18";
			var info = supportingDocumentMetaData.CY_CodeInfo;
			AssertNoErrorContaining("When Code is valid", info, expected);

			supportingDocumentMetaData.CY_Code = "NotValid";
			info = supportingDocumentMetaData.CY_CodeInfo;
			AssertHasErrorContaining("When Code is not valid", info, expected);
		}
	}
}
