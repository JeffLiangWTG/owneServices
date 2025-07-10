using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PIDCusPersonValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDuplicateFullName()
		{
			var cusPerson = declaration.Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson.Person.PER_FullName = "홍길동";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson = declaration.Persons.AddNew();
			glbPerson = Factory.New<GlbPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			cusPerson.Person.PER_FullName = "홍길동";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoErrors(cusPerson.CPN_PER_PersonInfo);

			cusPerson = declaration.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertHasErrorContaining(cusPerson.CPN_PER_PersonInfo, "You have already entered the person");
		}
		public void TestDuplicateRelationshipCode00()
		{
			var cusPerson = declaration.Persons.AddNew();
			cusPerson.RelationshipToDeclarant = RelationshipCodeList.Codes._01;
			cusPerson.Validation.ValidateRelationshipToDeclarant();
			AssertNoErrors(cusPerson.RelationshipToDeclarantInfo);

			cusPerson = declaration.Persons.AddNew();
			cusPerson.RelationshipToDeclarant = RelationshipCodeList.Codes._01;
			cusPerson.Validation.ValidateRelationshipToDeclarant();
			AssertNoErrors(cusPerson.RelationshipToDeclarantInfo);

			cusPerson.RelationshipToDeclarant = RelationshipCodeList.Codes._00;
			cusPerson.Validation.ValidateRelationshipToDeclarant();
			AssertNoErrors(cusPerson.RelationshipToDeclarantInfo);

			cusPerson = declaration.Persons.AddNew();
			cusPerson.RelationshipToDeclarant = RelationshipCodeList.Codes._00;
			cusPerson.Validation.ValidateRelationshipToDeclarant();
			AssertHasErrorContaining(cusPerson.RelationshipToDeclarantInfo, "You have already entered the code");

			cusPerson.RelationshipToDeclarant = "X";
			AssertHasMessageErrorContaining(cusPerson.RelationshipToDeclarantInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
		}
		JobDeclaration declaration;
	}
}
