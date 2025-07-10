using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderEntryCustomsOfficeWrapperTest : TestCaseWithFactory
{
	public void TestNationality()
	{
		AssertEquals(ZString.Empty, entryCustomsOfficeWrapper.Nationality);
		officeCode.CY_Data = "IT303199";
		AssertEquals("IT", entryCustomsOfficeWrapper.Nationality);
		officeCode.CY_Data = "DE303199";
		AssertEquals(ZString.Empty, entryCustomsOfficeWrapper.Nationality);
		jobDeclaration.CustomsOffices.RemoveAndDeleteAll();
		var extraEuOfficeCode1 = jobDeclaration.CustomsOffices.AddNew();
		extraEuOfficeCode1.CY_Data = "BR289320";
		var extraEuOfficeCode2 = jobDeclaration.CustomsOffices.AddNew();
		extraEuOfficeCode2.CY_Data = "AU328299";
		AssertEquals(ZString.Empty, entryCustomsOfficeWrapper.Nationality);
	}

	public void TestReferenceNumber()
	{
		AssertEquals(ZString.Empty, entryCustomsOfficeWrapper.ReferenceNumber);
		officeCode.CY_Data = "IT303199";
		AssertEquals("303199", entryCustomsOfficeWrapper.ReferenceNumber);
		officeCode.CY_Data = "DE303199";
		AssertEquals("DE303199", entryCustomsOfficeWrapper.ReferenceNumber);
	}

	public void TestName()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT303199", "CAMPOBASSO", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE503199", "VERDEN", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE009106", "Kontrolleinheit Flughafen Rostock", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		Factory.Save();

		officeCode.CY_Data = "IT303199";
		AssertEquals(ZString.Empty, entryCustomsOfficeWrapper.Name);

		officeCode.CY_Data = "DE503199";
		AssertEquals("VERDEN", entryCustomsOfficeWrapper.Name);

		officeCode.CY_Data = "DE009106";
		AssertEquals("Kontrolleinheit Flughafen Rost", entryCustomsOfficeWrapper.Name);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderEntryCustomsOfficeWrapper(null));
		AssertNoExceptionThrown(() => new IMHeaderEntryCustomsOfficeWrapper(jobDeclaration));
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		officeCode = Factory.New<EuOfficeCode>();
		jobDeclaration.CustomsOffices.Add(officeCode);
		officeCode.CY_Code = "ENT";
		entryCustomsOfficeWrapper = new IMHeaderEntryCustomsOfficeWrapper(jobDeclaration);
	}
	EuOfficeCode officeCode;
	JobDeclaration jobDeclaration;
	IMHeaderEntryCustomsOfficeWrapper entryCustomsOfficeWrapper;
}
