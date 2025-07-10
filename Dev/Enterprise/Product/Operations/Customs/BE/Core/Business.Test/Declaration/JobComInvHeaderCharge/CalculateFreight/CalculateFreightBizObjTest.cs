using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CalculateFreightBizObj))]
class CalculateFreightBizObjTest : NonPersistentBusinessObjectTestCase
{
	public void TestDefaultPercentageEUN()
	{
		var factory = Factory;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var countryCode = Core.Constants.CountryCodes.Belgium;
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(countryCode, "Belgium", eun);

		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");
		var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "BOS", "Boston", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Percentage", "Desc.", codeType, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		cusCode1.Attributes.AddNew("Percentage", "70");
		factory.Save();

		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
		AssertEquals("Percentage", 100m, bizObj.Percentage);

		declaration.JE_IATALoadPort = "BOS";
		bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
		AssertEquals("Percentage", 70m, bizObj.Percentage);
	}

	public void TestDefaultPercentageBE()
	{
		var factory = Factory;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var countryCode = Core.Constants.CountryCodes.Belgium;
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(countryCode, "Belgium", eun);

		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
		var cusCodeEUN = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "BOS", "Boston", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Percentage", "Desc.", codeType, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		cusCodeEUN.Attributes.AddNew("Percentage", "70");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");
		var cusCode1 = helper.CreateCusCodeList(countryCode, codeType, "BOS", "Boston", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Percentage", "Desc.", codeType, countryCode);
		cusCode1.Attributes.AddNew("Percentage", "60");
		factory.Save();

		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
		AssertEquals("Percentage", 100m, bizObj.Percentage);

		declaration.JE_IATALoadPort = "BOS";
		bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
		AssertEquals("Percentage", 60m, bizObj.Percentage);
	}
	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		return CalculateFreightBizObj.New(invoice.Charges, declaration);
	}
}
