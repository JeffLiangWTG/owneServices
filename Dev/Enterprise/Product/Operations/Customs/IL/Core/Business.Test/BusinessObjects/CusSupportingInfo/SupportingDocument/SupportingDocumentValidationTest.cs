using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(SupportingDocumentValidation))]
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			const string expectedWhenEmpty = "You have not entered";
			const string expectedWhenInvalid = "The code you have selected is not in the list.";

			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeType(code: "ILDOC", desc: "ILDOC", dataGrouping: Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ILDOC", code: "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			var info = supportingDocument.CSI_CodeInfo;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", info, expectedWhenEmpty);

			supportingDocument.CSI_Code = "code1";
			AssertNoMessageErrorContaining("When the code is valid, the empty message should not be shown.", info, expectedWhenEmpty);
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", info, expectedWhenInvalid);

			supportingDocument.CSI_Code = "BAD";
			AssertNoMessageErrorContaining("When the code is invalid, the empty message should not be shown.", info, expectedWhenEmpty);
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", info, expectedWhenInvalid);
		}
	}
}
