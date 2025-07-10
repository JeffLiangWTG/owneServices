using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonBaseOnlyTest : CusPersonAbstractTest<CusPersonCountry>
	{
		public override void TestCountriesType()
		{
			var person = Factory.New<CusPerson>();
			AssertEquals("CusPersonCountryCollection type", "Enterprise.Customs.ASYCUDA.Business.CusPersonCountryCollection`1[Enterprise.Customs.ASYCUDAManifest.Business.CusPersonCountry]", person.Countries.GetType().ToString());
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			var personCountry = person.Countries.AddNew();
			personCountry.CPC_Type = "DSA";
			personCountry.CPC_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			personCountry.CPC_Value = "SD";
			Factory.Save();

			personCountry.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (person.IsDeleted)
				{
					Assert("This is wrong Person Countries should not be deleted after the Person", false);
				}
			};

			person.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusPerson)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusPersonCountry)));
		}

		public void TestProxiedPropertiesDefaultToEmptyWithNoPerson()
		{
			var person = Factory.New<CusPerson>();
			AssertEquals(ZString.Empty, person.PersonFullName);
			AssertEquals(ZString.Empty, person.PersonGender);
			AssertEquals(ZString.Empty, person.PersonPassport);
			AssertEquals(ZDate.Empty, person.PersonPassportExpiry);
			AssertEquals(ZDate.Empty, person.PersonBirthDate);
			AssertEquals(ZString.Empty, person.PersonIdentificationNumber);
			AssertEquals(ZString.Empty, person.PersonNationality);
			AssertEquals(ZString.Empty, person.PersonCountry);
			AssertEquals(ZString.Empty, person.PersonPassportPlaceOfIssue);
		}

		public void TestPropertiesProxiedFromGlbPerson()
		{
			glbPerson.PER_FullName = "Daniel";
			glbPerson.PER_Gender = Core.Constants.Genders.Man;
			glbPerson.PER_Passport = "123";
			glbPerson.PER_PassportExpiryDate = ZDate.Today;
			glbPerson.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			glbPerson.PER_DriversLicenseNumber = "DL123";
			glbPerson.PER_RN_NKNationalityCodeISO = Core.Constants.CountryCodes.UnitedKingdom;
			glbPerson.PER_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			glbPerson.PER_PassportPlaceOfIssue = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var person = Factory.New<CusPerson>();
			person.CPN_PER_Person = glbPerson.PK;
			AssertEquals("Daniel", person.PersonFullName);
			AssertEquals(Core.Constants.Genders.Man, person.PersonGender);
			AssertEquals("123", person.PersonPassport);
			AssertEquals(ZDate.Today, person.PersonPassportExpiry);
			AssertEquals(ZDateTime.BrettsBirthday.Date, person.PersonBirthDate);
			AssertEquals("DL123", person.PersonIdentificationNumber);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, person.PersonNationality);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, person.PersonCountry);
			AssertEquals(Core.Constants.CountryCodes.Australia, person.PersonPassportPlaceOfIssue);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(AsycudaManifestHeaderSchema.Constants.Prefix, Factory.New<CusPerson>().CPN_ParentTableCode);
		}

		public void TestTypeDecider()
		{
			AssertType<CusPersonTypeDecider>(CusPerson.TypeDecider);
		}

		public void TestHeader()
		{
			var person = Factory.New<CusPerson>();
			AssertNull(person.Header);
			var header = Factory.New<AsycudaManifestHeader>();
			header.Persons.Add(person);
			AssertSame(header, person.Header);
			AssertEquals(true, person.CPN_ParentIDInfo.ReadOnly);
			AssertEquals(true, person.CPN_ParentTableCodeInfo.ReadOnly);
		}

		public void TestCountries()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.Persons));
		}

		public void TestValidation()
		{
			AssertType<CusPersonValidation>(Factory.New<CusPerson>().Validation);
		}

		public void TestLookups()
		{
			AssertType<CusPersonLookups>(Factory.New<CusPerson>().Lookups);
		}

		public void TestIsDriver()
		{
			var person = Factory.New<CusPerson>();
			AssertEquals(true, person.IsDriver);
			person.CPN_IsPassenger = true;
			AssertEquals(false, person.IsDriver);
		}

		public void TestCPN_PER_Person()
		{
			var person = Factory.New<CusPerson>();
			glbPerson.PER_FullName = "Gary Marsh";
			person.CPN_PER_Person = glbPerson.PK;
			AssertEquals("Gary Marsh", person.PersonFullName);

			var glbPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			glbPerson2.PER_FullName = "Scott McDonald";
			person.CPN_PER_Person = glbPerson2.PK;
			AssertEquals("Scott McDonald", person.PersonFullName);
		}

		public void TestFetchStrategy()
		{
			var person = Factory.New<CusPerson>();
			AssertType<CusPersonFetchStrategy>(person.FetchStrategy);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			return person;
		}

		protected override void SetUp()
		{
			base.SetUp();
			glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
		}
		GlbPerson glbPerson;
	}
}
