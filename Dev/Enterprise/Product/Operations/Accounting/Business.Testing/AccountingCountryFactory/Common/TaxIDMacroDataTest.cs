using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class TaxIDMacroDataTest : TestCase
	{
		public void TestConstructorValidation()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("When OrgTaxRegistrationPrefix and OrgTaxRegistrationCode are valid",
					() => new TaxIDMacroData(orgTaxRegistrationPrefix: "dummyOrgTaxRegistrationPrefix", "dummyOrgTaxRegistrationCode"));

				AssertExceptionThrown<ArgumentException>("When OrgTaxRegistrationPrefix is not valid and OrgTaxRegistrationCode is valid",
				"Value cannot be empty string (\"\").\r\nParameter name: OrgTaxRegistrationPrefix", () => new TaxIDMacroData(orgTaxRegistrationPrefix: null, "dummyOrgTaxRegistrationCode"));

				AssertExceptionThrown<ArgumentException>("When OrgTaxRegistrationPrefix is not valid and OrgTaxRegistrationCode is valid",
				"Value cannot be empty string (\"\").\r\nParameter name: OrgTaxRegistrationPrefix", () => new TaxIDMacroData(orgTaxRegistrationPrefix: ZString.Empty, "dummyOrgTaxRegistrationCode"));

				AssertExceptionThrown<ArgumentException>("When OrgTaxRegistrationPrefix is valid and OrgTaxRegistrationCode is not valid",
				"Value cannot be empty string (\"\").\r\nParameter name: OrgTaxRegistrationCode", () => new TaxIDMacroData("dummyOrgTaxRegistrationPrefix", orgTaxRegistrationCode: null));

				AssertExceptionThrown<ArgumentException>("When OrgTaxRegistrationPrefix is valid and OrgTaxRegistrationCode is not valid",
				"Value cannot be empty string (\"\").\r\nParameter name: OrgTaxRegistrationCode", () => new TaxIDMacroData("dummyOrgTaxRegistrationPrefix", orgTaxRegistrationCode: ZString.Empty));

				AssertNoExceptionThrown("When ExtraOrgTaxRegistrationPrefix and ExtraOrgTaxRegistrationCode are valid",
					() => new TaxIDMacroData("dummyOrgTaxRegistrationPrefix", "dummyOrgTaxRegistrationCode", "dummyExtraOrgTaxRegistrationPrefix", "dummyExtraOrgTaxRegistrationCode"));

				AssertExceptionThrown<ArgumentException>("When ExtraOrgTaxRegistrationPrefix is not valid",
				"Value cannot be null.\r\nParameter name: ExtraOrgTaxRegistrationPrefix", () => new TaxIDMacroData("dummyOrgTaxRegistrationPrefix", "dummyOrgTaxRegistrationCode",null));

				AssertExceptionThrown<ArgumentException>("When ExtraOrgTaxRegistrationCode is not valid",
				"Value cannot be null.\r\nParameter name: ExtraOrgTaxRegistrationCode", () => new TaxIDMacroData("dummyOrgTaxRegistrationPrefix", "dummyOrgTaxRegistrationCode", extraOrgTaxRegistrationCode: null));
			});
		}
	}
}
