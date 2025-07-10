using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportCusPersonValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBirthdayAndFullName()
		{
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			var cusPerson = declaration.Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "You have not entered a birthday for this person.");
			AssertNoMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "You have not entered a full name for this person.");
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "You have not entered a birthday for this person.");
			AssertHasMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "You have not entered a full name for this person.");

			cusPerson.Person.PER_FullName = "홍길동";
			cusPerson.Person.PER_BirthDate = new ZDate(2023, 01, 04);
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson.Person.PER_FullName = "홍길동1";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "The full name of Stevedores must be in English, Korean, or spaces only. it must not enter numbers or special characters.");

			cusPerson.Person.PER_FullName = "HONG";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson.Person.PER_FullName = "HONG!";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "The full name of Stevedores must be in English, Korean, or spaces only. it must not enter numbers or special characters.");

			cusPerson.Person.PER_FullName = "홍 길동";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson.Person.PER_FullName = "Maurício";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasMessageErrorContaining(cusPerson.CPN_PER_PersonInfo, "The full name of Stevedores must be in English, Korean, or spaces only. it must not enter numbers or special characters.");
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
