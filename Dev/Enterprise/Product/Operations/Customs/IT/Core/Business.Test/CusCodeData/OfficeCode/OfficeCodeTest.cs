using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(OfficeCode))]
sealed class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
{
	public void TestValidation()
	{
		var officeCode = GetNewOfficeCode();
		AssertType<OfficeCodeValidation>("Validation Type", officeCode.Validation);
	}

	protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsOffices.AddNew();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();

	public void TestIsOfficeOfTransit()
	{
		var officeCode = GetNewOfficeCode();

		officeCode.CY_Code = "";
		AssertEquals("IsOfficeOfTransit", false, officeCode.IsOfficeOfTransit);

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfTransit;
		AssertEquals("IsOfficeOfTransit", true, officeCode.IsOfficeOfTransit);

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
		AssertEquals("IsOfficeOfTransit", false, officeCode.IsOfficeOfTransit);
	}

	public void TestIsOfficeOfDestination()
	{
		var officeCode = GetNewOfficeCode();

		officeCode.CY_Code = "";
		AssertEquals("IsOfficeOfDestination", false, officeCode.IsOfficeOfDestination);

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
		AssertEquals("IsOfficeOfDestination", true, officeCode.IsOfficeOfDestination);

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfTransit;
		AssertEquals("IsOfficeOfDestination", false, officeCode.IsOfficeOfDestination);
	}

	public void TestIsPartOfEuropeanUnion()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland, parent: null);

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "CUSOF", "IT0001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, "CUSOF", "CH0001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		Factory.Save();

		var officeCode = Factory.New<OfficeCode>();

		CombineAssertions(() =>
		{
			officeCode.CY_Data = "";
			AssertEquals("When Office is Empty, IsPartOfEuropeanUnion", false, officeCode.IsPartOfEuropeanUnion);

			officeCode.CY_Data = "IT0001";
			AssertEquals("When Office starts with IT, IsPartOfEuropeanUnion", true, officeCode.IsPartOfEuropeanUnion);

			officeCode.CY_Data = "CH0001";
			AssertEquals("When Office starts with CH, IsPartOfEuropeanUnion", false, officeCode.IsPartOfEuropeanUnion);
		});
	}

	OfficeCode GetNewOfficeCode() => (OfficeCode)GetNewBusinessObject();
}
