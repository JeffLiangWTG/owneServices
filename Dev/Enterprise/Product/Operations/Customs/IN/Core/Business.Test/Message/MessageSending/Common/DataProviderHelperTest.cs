using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DataProviderHelper))]
sealed class DataProviderHelperTest : TestCaseWithFactory
{
	public void TestToDateTimeOrNullIfEmpty_ZDate()
	{
		CombineAssertions(() =>
		{
			var inputValue = ZDate.Empty;
			AssertNull("Date is empty", inputValue.ToDateTimeOrNullIfEmpty());

			inputValue = new ZDate(2022, 3, 16);
			AssertEquals("Date is valid", new DateTime(2022, 3, 16), inputValue.ToDateTimeOrNullIfEmpty());
		});
	}

	public void TestToDateTimeOrNullIfEmpty_ZDateTime()
	{
		CombineAssertions(() =>
		{
			var inputValue = ZDateTime.Empty;
			AssertNull("Date is empty", inputValue.ToDateTimeOrNullIfEmpty());

			inputValue = new ZDateTime(2022, 3, 16, 17, 5, 1);
			AssertEquals("Date is valid", new DateTime(2022, 3, 16, 17, 5, 1), inputValue.ToDateTimeOrNullIfEmpty());
		});
	}

	public void TestGetFullAddressString()
	{
		AssertGetFullAddressString("Company", "Address1", "Address2", "City", "State", "Postcode", "Company Address1 Address2 City State Postcode");
		AssertGetFullAddressString("Company", "", "Address2", "City", "State", "Postcode", "Company Address2 City State Postcode");
		AssertGetFullAddressString("Company", "Address1", "", "City", "State", "Postcode", "Company Address1 City State Postcode");
		AssertGetFullAddressString("Company", "Address1", "Address2", "", "State", "Postcode", "Company Address1 Address2 State Postcode");
		AssertGetFullAddressString("Company", "Address1", "Address2", "City", "", "Postcode", "Company Address1 Address2 City Postcode");
		AssertGetFullAddressString("Company", "Address1", "Address2", "City", "State", "", "Company Address1 Address2 City State");
		AssertGetFullAddressString("Company", "", "", "", "", "", "Company");
		AssertGetFullAddressString("", "Address1", "", "", "", "", "Address1");
		AssertGetFullAddressString("", "", "Address2", "", "", "", "Address2");
		AssertGetFullAddressString("", "", "", "City", "", "", "City");
		AssertGetFullAddressString("", "", "", "", "State", "", "State");
		AssertGetFullAddressString("", "", "", "", "", "Postcode", "Postcode");
	}

	void AssertGetFullAddressString(ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postcode, ZString expectedResult)
	{
		var address = Factory.New<JobDocAddress>();
		address.CompanyName = companyName;
		address.Address1 = address1;
		address.Address2 = address2;
		address.City = city;
		address.State = state;
		address.Postcode = postcode;
		AssertEquals("Full address string", expectedResult, address.GetFullAddressString());
	}

	public void TestNormalizeToSingleLine()
	{
		var inputText = ZString.Empty;
		AssertEquals("Empty string", ZString.Empty, inputText.NormalizeToSingleLine());

		inputText = "This is a single line string.";
		AssertEquals("Single line string", "This is a single line string.", inputText.NormalizeToSingleLine());

		inputText = "First line.\nSecond line.\rThird line.\r\nLast line.";
		AssertEquals("Normalized single line string", "First line. Second line. Third line. Last line.", inputText.NormalizeToSingleLine());
	}
}
