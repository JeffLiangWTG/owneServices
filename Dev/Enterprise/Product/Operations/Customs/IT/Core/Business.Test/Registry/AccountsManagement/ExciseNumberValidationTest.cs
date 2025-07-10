using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class ExciseNumberValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		var account = GetNewAccountCollection().AddNew();
		var exciseNumber = new ExciseNumber(account, Factory);

		AssertExceptionThrown<ArgumentNullException>("exciseNumber is required", () => new ExciseNumberValidation(null));
		AssertNoExceptionThrown(() => new ExciseNumberValidation(exciseNumber));
	}

	public void TestCheckNumber_MandatoryValidation()
	{
		var account = GetNewAccountCollection().AddNew();
		var exciseNumber = account.ExciseNumbers.AddNew();

		exciseNumber.Number = "X";
		AssertNoErrorContaining("Filled Number", exciseNumber.NumberInfo, "Please enter a value");

		exciseNumber.Number = ZString.Empty;
		AssertHasErrorContaining("Empty Number", exciseNumber.NumberInfo, "Please enter a value");
	}

	public void TestCheckNumber_FormatValidation()
	{
		SetUpITCountryStates();

		var account = GetNewAccountCollection().AddNew();
		var exciseNumber = account.ExciseNumbers.AddNew();
		exciseNumber.Number = ZString.Empty;
		AssertNoErrorContaining("Empty Excise Number", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "DE00";
		AssertHasErrorContaining("Chars 0 and 1 != 'IT'", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "ITXX";
		AssertHasErrorContaining("Chars 2 and 3 != '00'", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "ITXX";
		AssertHasErrorContaining("Chars 4 and 5 absent", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "ITXXMI";
		AssertHasErrorContaining("Chars 4 and 5 invalid state", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "IT00PD";
		AssertHasErrorContaining("Missing chars to reach 13 chars length", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);

		exciseNumber.Number = "IT00PD0000000";
		AssertNoErrorContaining("Valid Excise Number", exciseNumber.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);
	}

	public void TestCheckNumber_UniqueValidation()
	{
		SetUpITCountryStates();

		var accounts = GetNewAccountCollection();
		var account = accounts.AddNew();

		var exciseNumber1 = account.ExciseNumbers.AddNew();
		exciseNumber1.Number = "IT00PD0000000";
		AssertNoErrorContaining("No duplicated Excise Number in same account (PD)", exciseNumber1.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);
		var exciseNumber2 = account.ExciseNumbers.AddNew();
		exciseNumber2.Number = "IT00MI0000000";
		AssertNoErrorContaining("No duplicated Excise Number in same account (MI)", exciseNumber2.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);

		exciseNumber2.Number = "IT00PD0000000";
		AssertHasErrorContaining("Duplicated Excise Number in same account", exciseNumber2.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);

		exciseNumber2.Number = "IT00MI0000000";
		var duplicatedAccountInTheSameCompany = accounts.AddNew();
		var exciseNumber3 = duplicatedAccountInTheSameCompany.ExciseNumbers.AddNew();
		exciseNumber3.Number = "IT00MI0000000";
		AssertHasErrorContaining("Duplicated Excise Number in same company", exciseNumber3.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);

		exciseNumber3.Number = "IT00VE0000000";
		AssertNoErrorContaining("No more duplicated Excise Number in same company", exciseNumber3.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);

		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.GC_Name = "C01";
		otherCompany.Branches.AddNew();

		var accountsForOtherCompany = accounts.Clone(new FallbackLevel(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		var accountForOtherCompany = accountsForOtherCompany.AddNew();

		var exciseNumber4 = accountForOtherCompany.ExciseNumbers.AddNew();
		exciseNumber4.Number = "IT00VE0000000";

		exciseNumber1.Number = "IT00VE0000000";
		AssertHasErrorContaining("Duplicated Excise Number from other company", exciseNumber1.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);

		exciseNumber1.Number = "IT00RM0000000";
		AssertNoErrorContaining("No more duplicated Excise Number from other company", exciseNumber4.NumberInfo, ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);
	}

	void SetUpITCountryStates()
	{
		var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy);
		country.States.AddNew().RW_Code = "PD";
		country.States.AddNew().RW_Code = "MI";
		country.States.AddNew().RW_Code = "VE";
		country.States.AddNew().RW_Code = "RM";
	}

	AccountCollection GetNewAccountCollection()
	{
		return new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}
}
