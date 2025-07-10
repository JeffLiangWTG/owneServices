using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PersonalItemDecQuestion))]
	sealed class PersonalItemDecQuestionsTest : CusCodeDataTest<PersonalItemDecQuestion>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.PersonalItemDecQuestion, personalItemDecQuestion.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<PersonalItemDecQuestionValidation>(personalItemDecQuestion.Validation);
		}

		public void TestCY_Code()
		{
			personalItemDecQuestion.CY_Code = "1";
			AssertEquals("1", personalItemDecQuestion.CY_Code);
			AssertEquals(1, personalItemDecQuestion.CY_CodeInfo.MaxLength);
		}

		public void TestCY_Data()
		{
			personalItemDecQuestion.CY_Data = "Y";
			AssertEquals("Y", personalItemDecQuestion.CY_Data);
			AssertEquals(1, personalItemDecQuestion.CY_DataInfo.MaxLength);
		}

		protected override IEnumerable<PersonalItemDecQuestion> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return personalItemDecQuestion;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<PersonalItemDecQuestion>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			personalItemDecQuestion = new PersonalItemDecQuestionCollection(declaration).AddNew();
			Factory.Save();
		}
		PersonalItemDecQuestion personalItemDecQuestion;
	}
}
