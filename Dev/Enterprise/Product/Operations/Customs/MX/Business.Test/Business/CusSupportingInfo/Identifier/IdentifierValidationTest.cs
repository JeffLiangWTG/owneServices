using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	class IdentifierValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_Code = "AC";
			var question = identifier.Complement1Question;
			identifier.CSI_ReferenceNumber = "ABCDEFGHIJKLM";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_ReferenceNumberInfo, "Complement 1 exceeded the max size");

			identifier.CSI_ReferenceNumber = "ABCDE";
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_ReferenceNumberInfo, "Complement 1 exceeded the max size");

			identifier.CSI_ReferenceNumber = "XXXXXXXXXXXXXX";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_ReferenceNumberInfo, "Complement 1 exceeded the max size");

			question.XQ2_AnswerMaxLength = 0;
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_ReferenceNumberInfo, "Complement 1 exceeded the max size");
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_Code = "AI";
			var question = identifier.Complement2Question;
			question.XQ2_Note = "AAA";
			question.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			question.XQ2_AnswerMaxLength = 5;
			identifier.CSI_ReferenceNumber2 = "ABCDEFGHIJKLM";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_ReferenceNumber2Info, "Complement 2 exceeded the max size");

			identifier.CSI_ReferenceNumber2 = "ABCDE";
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_ReferenceNumber2Info, "Complement 2 exceeded the max size");

			identifier.CSI_ReferenceNumber2 = "XXXXXXXXXXXXXX";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_ReferenceNumber2Info, "Complement 2 exceeded the max size");

			question.XQ2_AnswerMaxLength = 0;
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_ReferenceNumber2Info, "Complement 2 exceeded the max size");
		}

		public void TestCheckCSI_Description()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_Code = "B2";
			var question = identifier.Complement3Question;
			question.XQ2_Note = "AAA";
			question.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			question.XQ2_AnswerMaxLength = 5;
			identifier.CSI_Description = "ABCDEFGHIJKLM";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_DescriptionInfo, "Complement 3 exceeded the max size");

			identifier.CSI_Description = "ABCDE";
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_DescriptionInfo, "Complement 3 exceeded the max size");

			identifier.CSI_Description = "XXXXXXXXXXXXXX";
			identifier.Validation.ValidateAll();
			AssertHasMessageErrorContaining(identifier.CSI_DescriptionInfo, "Complement 3 exceeded the max size");

			question.XQ2_AnswerMaxLength = 0;
			identifier.Validation.ValidateAll();
			AssertNoMessageErrorContaining(identifier.CSI_DescriptionInfo, "Complement 3 exceeded the max size");
		}
	}
}
