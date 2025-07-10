using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusPersonCountry))]
	sealed class CusPersonCountryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusPerson = Factory.New<JobDeclaration>().Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			glbPerson.PER_FullName = "Kenny G";
			cusPerson.CPN_PER_Person = glbPerson.PK;

			var country = cusPerson.Countries.AddNew();
			country.CPC_Type = CusPersonCountryTypeList.Codes.RelationshipToDeclarant;
			country.CPC_Value = "아버지";

			return country;
		}
	}
}
