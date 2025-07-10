using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(Identifier))]
	class IdentifierTest : Customs.Business.Testing.CusSupportingInfoTest<Identifier>
	{
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<Identifier>();
			CombineAssertions(() =>
			{
				AssertEquals(CusSupportingInfoTypeList.Codes.Identifier, supporting.CSI_Type);
				AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
			});
		}

		public void TestQuestions()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			AssertEquals(0, identifier.Questions.Count());

			identifier.CSI_Code = "AI";
			AssertEquals(3, identifier.Questions.Count());
			AssertEquals("CO1", identifier.Complement1Question.XQ2_Code);
			AssertEquals("CO2", identifier.Complement2Question.XQ2_Code);
			AssertEquals("CO3", identifier.Complement3Question.XQ2_Code);
			AssertContainsExactElementsInAnyOrder(new[] { "CO1", "CO2", "CO3" }, identifier.Questions.Select(x => x.XQ2_Code));
			AssertContainsExactElementsInAnyOrder(new[] { "AI", "AI", "AI" }, identifier.Questions.Select(x => x.ProfileType.XXX_ProfileType));

			identifier.CSI_Code = "AC";
			AssertEquals(1, identifier.Questions.Count());
			AssertEquals("CO1", identifier.Questions.FirstOrDefault().XQ2_Code);
			AssertEquals("CO1", identifier.Complement1Question.XQ2_Code);
			AssertNull(identifier.Complement2Question);
			AssertNull(identifier.Complement3Question);
			AssertContainsExactElementsInAnyOrder(new[] { "AC" }, identifier.Questions.Select(x => x.ProfileType.XXX_ProfileType));
		}

		public void TestFillGuidance1()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			AssertEquals(ZString.Empty, identifier.FillGuidance1);

			identifier.CSI_Code = "AI";
			var question = identifier.Complement1Question;
			AssertEquals(question.XQ2_Note, identifier.FillGuidance1);
		}

		public void TestFillGuidance2()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			AssertEquals(ZString.Empty, identifier.FillGuidance2);

			identifier.CSI_Code = "AI";
			var question = identifier.Complement2Question;
			AssertEquals(question.XQ2_Note, identifier.FillGuidance2);
		}

		public void TestFillGuidance3()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			AssertEquals(ZString.Empty, identifier.FillGuidance3);

			identifier.CSI_Code = "B2";
			var question = identifier.Complement3Question;
			AssertEquals(question.XQ2_Note, identifier.FillGuidance3);
		}

		public void TestResetComplementsWhenSetCode()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_ReferenceNumber = "1";
			identifier.CSI_ReferenceNumber2 = "2";
			identifier.CSI_Description = "3";

			AssertEquals("1", identifier.CSI_ReferenceNumber);
			AssertEquals("2", identifier.CSI_ReferenceNumber2);
			AssertEquals("3", identifier.CSI_Description);

			identifier.CSI_Code = "AI";
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber);
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber2);
			AssertEquals(ZString.Empty, identifier.CSI_Description);
		}

		public void TestReadOnlyProperties()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();

			identifier.CSI_Code = "AI";
			Assert(!identifier.CSI_ReferenceNumberInfo.ReadOnly);
			Assert(!identifier.CSI_ReferenceNumber2Info.ReadOnly);
			Assert(!identifier.CSI_DescriptionInfo.ReadOnly);

			identifier.CSI_Code = "AC";
			Assert(!identifier.CSI_ReferenceNumberInfo.ReadOnly);
			Assert(identifier.CSI_ReferenceNumber2Info.ReadOnly);
			Assert(identifier.CSI_DescriptionInfo.ReadOnly);

			identifier.CSI_Code = "B2";
			Assert(identifier.CSI_ReferenceNumberInfo.ReadOnly);
			Assert(identifier.CSI_ReferenceNumber2Info.ReadOnly);
			Assert(!identifier.CSI_DescriptionInfo.ReadOnly);

			identifier.CSI_Code = "XX";
			Assert(identifier.CSI_ReferenceNumberInfo.ReadOnly);
			Assert(identifier.CSI_ReferenceNumber2Info.ReadOnly);
			Assert(identifier.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestComplementFieldType()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();

			identifier.CSI_Code = "AC";
			AssertEquals(Universal.Constants.ProfileQuestion.AnswerDataTypes.String, identifier.Complement1Question.XQ2_AnswerDataType);
			AssertEquals(nameof(FieldType.Text), identifier.Complement1FieldType);

			identifier.CSI_Code = "AI";
			AssertEquals(Universal.Constants.ProfileQuestion.AnswerDataTypes.List, identifier.Complement2Question.XQ2_AnswerDataType);
			AssertEquals(nameof(FieldType.TextDropEdit), identifier.Complement2FieldType);

			identifier.CSI_Code = "B2";
			AssertEquals(Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, identifier.Complement3Question.XQ2_AnswerDataType);
			AssertEquals(nameof(FieldType.Text), identifier.Complement3FieldType);
		}

		public void TestMaxSizeOfProperties()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();

			identifier.CSI_Code = "AI";
			AssertEquals(0, identifier.Complement1MaxLength);
			AssertEquals(0, identifier.Complement2MaxLength);
			identifier.CSI_Code = "B2";
			AssertEquals(2, identifier.Complement3DecimalPlaces);

			identifier.CSI_Code = "AC";
			AssertEquals(5, identifier.Complement1MaxLength);
			AssertEquals(0, identifier.Complement1DecimalPlaces);
		}

		public void TestGetAndSetNumberComplements()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			identifier.CSI_Code = "AI";

			var question1 = identifier.Complement1Question;
			question1.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question1.XQ2_AnswerDecimalPlaces = 2;

			var question2 = identifier.Complement2Question;
			question2.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question2.XQ2_AnswerDecimalPlaces = 2;

			var question3 = identifier.Complement3Question;
			question3.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question3.XQ2_AnswerDecimalPlaces = 2;

			identifier.CSI_ReferenceNumber = "AAAAAAAAAAAAA";
			identifier.CSI_ReferenceNumber2 = "ABCDEFGHIJKLM";
			identifier.CSI_Description = "TEST";
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber);
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber2);
			AssertEquals(ZString.Empty, identifier.CSI_Description);

			identifier.CSI_ReferenceNumber = "4.412345";
			identifier.CSI_ReferenceNumber2 = "2.12345";
			identifier.CSI_Description = "12.333333";
			AssertEquals("4.41", identifier.CSI_ReferenceNumber);
			AssertEquals("2.12", identifier.CSI_ReferenceNumber2);
			AssertEquals("12.33", identifier.CSI_Description);

			identifier.CSI_ReferenceNumber = ZString.Empty;
			identifier.CSI_ReferenceNumber2 = ZString.Empty;
			identifier.CSI_Description = ZString.Empty;
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber);
			AssertEquals(ZString.Empty, identifier.CSI_ReferenceNumber2);
			AssertEquals(ZString.Empty, identifier.CSI_Description);
		}

		protected override IEnumerable<Identifier> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var identifier = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().Identifiers.AddNew();
			identifier.CSI_Code = "1";
			yield return identifier;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			return invoiceLine.Identifiers.AddNew();
		}
	}
}
