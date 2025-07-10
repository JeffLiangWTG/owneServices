using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(UniversalReferenceDataHelper))]
sealed class UniversalReferenceDataHelperTest : TestCaseWithFactory
{
	public void TestGetCustomsOfficeCollection()
	{
		ReferenceDataTestHelper.AssertCustomsOfficeCollection(
			() => UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, isAir: false),
			() => UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, isAir: true));
	}

	public void TestGetSWControl()
	{
		var factory = new BusinessObjectFactory();
		RefDataSetupTestHelper.SetupControlResultCode(factory);

		AssertEquals("IN00121", UniversalReferenceDataHelper.GetSWControl(factory, "IN00121", ZDate.Today).ZZD_Code);
		AssertEquals("IN00122", UniversalReferenceDataHelper.GetSWControl(factory, "IN00122", ZDate.Today).ZZD_Code);
	}

	public void TestCustomsOfficeIsAirOrSea()
	{
		var factory = new BusinessObjectFactory();
		RefDataSetupTestHelper.SetupCustomsLocationeData(factory);

		var temp = UniversalReferenceDataHelper.GetCustomsOffice(factory, "ABC123", ZDate.Today);

		AssertEquals("ABC123", UniversalReferenceDataHelper.GetCustomsOffice(factory, "ABC123", ZDate.Today).ZZD_Code);
		AssertEquals("PQR123", UniversalReferenceDataHelper.GetCustomsOffice(factory, "PQR123", ZDate.Today).ZZD_Code);
	}

	public void TestGetSupportDocumentTypeCollection()
	{
		ReferenceDataTestHelper.AssertSupportDocumentTypeCollection(
			() => UniversalReferenceDataHelper.GetSupportingDocumentTypeCollection(Factory));
	}

	public void TestUnitOfQuantityList()
	{
		RefDataSetupTestHelper.SetupCustomsUnitOfQuantityCode(Factory);

		var unitOfQuantityList = UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, ZDate.Today);
		var actualsUnits = unitOfQuantityList.GetAllCodes();
		CombineAssertions(() =>
		{
			AssertEquals("Count", actualsUnits.Length, 2);
			var expectedUnits = new[] { "KGS", "PCS" };
			AssertContainsExactElementsInAnyOrder("Units", expectedUnits, actualsUnits);
			AssertSame("Cached", unitOfQuantityList, UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, ZDate.Today));
		});
	}
}
