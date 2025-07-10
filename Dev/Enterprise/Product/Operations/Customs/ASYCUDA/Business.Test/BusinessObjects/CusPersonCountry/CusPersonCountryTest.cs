using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPersonCountry))]
	sealed class CusPersonCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationAndLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var cusPersonCountry = header.Persons.AddNew().Countries.AddNew();

			AssertType<CusPersonCountryValidation>(cusPersonCountry.Validation);
			AssertType<CusPersonCountryLookups>(cusPersonCountry.Lookups);
		}

		public void TestParentPerson()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var cusPerson = header.Persons.AddNew();
			var cusPersonCountry = cusPerson.Countries.AddNew();
			AssertSame(cusPerson, cusPersonCountry.ParentPerson);

			cusPersonCountry.CPC_CPN_Person = ZGuid.Empty;
			AssertNull(cusPersonCountry.ParentPerson);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var cusPerson = header.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
			var country = cusPerson.Countries.AddNew();
			country.CPC_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			country.CPC_Type = "OCC";
			country.CPC_Value = "H";
			return country;
		}
	}
}
