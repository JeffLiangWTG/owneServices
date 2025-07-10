using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CusPersonCountryCollection<CusPersonCountry>))]
	sealed class CusPersonCountryCollectionTest : Customs.Business.Testing.CusPersonCountryCollectionTest<CusPersonCountryCollection<CusPersonCountry>, CusPersonCountry>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var person = header.Persons.AddNew();
			var country = person.Countries.AddNew();
			country.CPC_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			country.CPC_Type = "OCC";
			country.CPC_Value = "H";
			country.ClearHasChanges();
			return new CusPersonCountryCollection<CusPersonCountry>(person);
		}
	}
}
