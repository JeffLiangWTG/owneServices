using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class CusPersonValidationTest : TestCaseWithFactory
	{
		public void TestCheckCPN_PER_Person_Mandatory()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var cusPerson = manifest.Persons.AddNew();
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertHasErrorContaining(cusPerson.CPN_PER_PersonInfo, MandatoryValidation.MustBeEntered);
			var glbPerson = Factory.New<GlbPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			AssertNoErrorContaining(cusPerson.CPN_PER_PersonInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckPersonIsUnique()
		{
			var errorMessage = "Person must be unique";
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var glbPerson1 = Factory.New<GlbPerson>();
			var glbPerson2 = Factory.New<GlbPerson>();
			var cusPerson1 = manifest.Persons.AddNew();
			var cusPerson2 = manifest.Persons.AddNew();
			cusPerson1.CPN_PER_Person = glbPerson1.PK;
			cusPerson2.CPN_PER_Person = glbPerson2.PK;
			AssertNoError(cusPerson1.CPN_PER_PersonInfo, errorMessage);
			AssertNoError(cusPerson2.CPN_PER_PersonInfo, errorMessage);
			cusPerson2.CPN_PER_Person = glbPerson1.PK;
			AssertNoError(cusPerson1.CPN_PER_PersonInfo, errorMessage);
			AssertHasError(cusPerson2.CPN_PER_PersonInfo, errorMessage);
		}

		public void TestCheckBirthDate()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Birth Date against the person");
			glbPerson.PER_BirthDate = ZDate.Today.AddDays(1);
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Birth Date against the person");
		}

		public void TestCheckPersonCountry()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Country against the person");
			glbPerson.PER_RN_NKCountry = Core.Constants.CountryCodes.Hungary;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Country against the person");
		}

		public void TestCheckIdentificationNumber()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter an Identification Number against the person");
			glbPerson.PER_DriversLicenseNumber = "123";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter an Identification Number against the person");
		}

		public void TestCheckPersonGender()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please update the Gender against the person. Only Male or Female is valid");
			glbPerson.PER_Gender = Core.Constants.Genders.Woman;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please update the Gender against the person. Only Male or Female is valid");
		}

		public void TestCheckPersonNationality()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Nationality against the person");
			glbPerson.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.Croatia;
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Nationality against the person");
		}

		public void TestCheckPersonPassport()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter Passport Details against the person");
			glbPerson.PER_Passport = "N124545DC";
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter Passport Details against the person");
		}

		public void CheckPersonPassportExpiry()
		{
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Passport Expiry Date against the person");
			glbPerson.PER_PassportExpiryDate = ZDate.Today.AddDays(-1);
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please enter a Passport Expiry Date against the person");
			AssertHasMessageError(cusPerson.CPN_PER_PersonInfo, "Please update the Passport Expiry Date against the person as it has expired");
			glbPerson.PER_BirthDate = ZDate.Today.AddDays(1);
			cusPerson.Validation.ValidateCPN_PER_Person();
			AssertNoMessageError(cusPerson.CPN_PER_PersonInfo, "Please update the Passport Expiry Date against the person as it has expired");
		}

		public void TestCheckCPN_IsPassenger()
		{
			var errorMessage = "Exactly one person should be marked as the driver, i.e. not a passenger.";
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var person1 = manifest.Persons.AddNew();
			var person2 = manifest.Persons.AddNew();
			var person3 = manifest.Persons.AddNew();
			person1.CPN_IsPassenger = false;
			person2.CPN_IsPassenger = true;
			person3.CPN_IsPassenger = false;
			AssertNoMessageError(person1.CPN_IsPassengerInfo, errorMessage);
			AssertNoMessageError(person2.CPN_IsPassengerInfo, errorMessage);
			AssertHasMessageError(person3.CPN_IsPassengerInfo, errorMessage);
			person3.CPN_IsPassenger = true;
			AssertNoMessageError(person1.CPN_IsPassengerInfo, errorMessage);
			AssertNoMessageError(person2.CPN_IsPassengerInfo, errorMessage);
			AssertNoMessageError(person3.CPN_IsPassengerInfo, errorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			glbPerson = Factory.New<GlbPerson>();
			cusPerson = Factory.New<CusPerson>();
			cusPerson.CPN_PER_Person = glbPerson.PK;
		}
		GlbPerson glbPerson;
		CusPerson cusPerson;
	}
}
