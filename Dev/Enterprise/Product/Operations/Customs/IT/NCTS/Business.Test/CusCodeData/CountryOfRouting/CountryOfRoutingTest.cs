using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(CountryOfRouting))]
sealed class CountryOfRoutingTest : Customs.Business.Testing.CusCodeDataTest<CountryOfRouting>
{
	public void TestGetNewValidation()
	{
		AssertType<CountryOfRoutingValidation>(countryOfRouting.Validation);
	}

	public void TestGetNewLookups()
	{
		AssertType<CountryOfRoutingLookups>(countryOfRouting.Lookups);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		countryOfRouting = header.CountriesOfRouting.AddNew();
	}
	NctsHeader header;
	CountryOfRouting countryOfRouting;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override IEnumerable<CountryOfRouting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return (CountryOfRouting)GetNewBusinessObject(factory);
	}

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var countryOfRouting = header.CountriesOfRouting.AddNew();
		return countryOfRouting;
	}
}
