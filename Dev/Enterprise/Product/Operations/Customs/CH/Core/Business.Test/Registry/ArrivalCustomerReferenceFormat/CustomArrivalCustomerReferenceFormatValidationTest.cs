using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business.Testing;

class CustomArrivalCustomerReferenceFormatValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAuthorizationLocationCode() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG00001";
		GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = orgHeader.PK;
		var authorisationTestHelper = new CusAuthorisationTestHelper(Factory);
		authorisationTestHelper.CreateCusAuthorisationHeader("101", CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, permitHolder: orgHeader);
		Factory.Save();

		ValidationTestHelper.AssertErrorIfNotEntered(customFormat.AuthorizationLocationCodeInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(customFormat.AuthorizationLocationCodeInfo, "XXX", "101");
	});

	public void TestCheckAuthorizationLocationCodeUnique() => CombineAssertions(() =>
	{
		const string errorMessage = "The Authorization Location Code has been duplicated and must be unique.";

		var customFormat2 = new CustomArrivalCustomerReferenceFormat(customFormat.CurrentFallbackLevel, Factory);
		customFormat2.AuthorizationLocationCode = "102";

		var customFormatCollection = new CustomArrivalCustomerReferenceFormatCollection(customFormat.CurrentFallbackLevel, Factory);
		customFormatCollection.Add(customFormat);
		customFormatCollection.Add(customFormat2);

		customFormat.AuthorizationLocationCode = "102";
		AssertHasError(customFormat.AuthorizationLocationCodeInfo, errorMessage);

		customFormat.AuthorizationLocationCode = "101";
		AssertNoError(customFormat.AuthorizationLocationCodeInfo, errorMessage);
	});

	public void TestCheckPrefix()
	{
		AssertCheckPrefixOrSuffixLength(customFormat.PrefixInfo, "Prefix",
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix2,
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4);
	}

	public void TestCheckSuffix()
	{
		AssertCheckPrefixOrSuffixLength(customFormat.SuffixInfo, "Suffix",
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix2,
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix4);
	}

	public void AssertCheckPrefixOrSuffixLength(ZPropertyInfo propertyInfo, string name,
		string yearOption2, string yearOption4)
	{
		var yearOptionList = new CustomArrivalCustomerReferenceFormatYearOptionList();
		var errorMessage2 = $"{name} must not be longer than 8 characters if '{yearOptionList.GetDescriptionFromCode(yearOption2)}' is selected.";
		var errorMessage4 = $"{name} must not be longer than 6 characters if '{yearOptionList.GetDescriptionFromCode(yearOption4)}' is selected.";

		AssertError(errorMessage2, yearOption2, "123456789");
		AssertError(null, yearOption2, "12345678");
		AssertError(errorMessage4, yearOption4, "1234567");
		AssertError(null, yearOption4, "1234567");

		foreach (var yearOption in yearOptionList.GetAllCodes().Except(new[] { yearOption2, yearOption4 }).Append(string.Empty))
		{
			AssertError(null, yearOption, "1234567890");
		}

		customFormat.YearOption = CustomArrivalCustomerReferenceFormatYearOptionList.Codes.None;
		propertyInfo.Value = new ZString("1234567890");
		customFormat.YearOption = yearOption2;
		AssertHasError("Validation triggered by YearOption", propertyInfo, errorMessage2);

		void AssertError(string expectedErrorMessage, ZString yearOption, ZString value)
		{
			customFormat.YearOption = yearOption;
			propertyInfo.Value = value;
			if (expectedErrorMessage != null)
			{
				AssertHasError(propertyInfo, expectedErrorMessage);
			}
			if (expectedErrorMessage != errorMessage2)
			{
				AssertNoError(propertyInfo, errorMessage2);
			}
			if (expectedErrorMessage != errorMessage2)
			{
				AssertNoError(propertyInfo, errorMessage2);
			}
		}
	}

	public void TestCheckYearOption()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(customFormat.YearOptionInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(customFormat.YearOptionInfo, "9", "2");
	}

	public void TestCheckSequenceNumberLength() => CombineAssertions(() =>
	{
		const string errorMessage = "Please enter a 'Sequence Number Digits' within the range 1 to 15.";

		customFormat.SequenceNumberLength = 0;
		AssertHasError(customFormat.SequenceNumberLengthInfo, errorMessage);
		customFormat.SequenceNumberLength = 1;
		AssertNoError(customFormat.SequenceNumberLengthInfo, errorMessage);
		customFormat.SequenceNumberLength = 15;
		AssertNoError(customFormat.SequenceNumberLengthInfo, errorMessage);
		customFormat.SequenceNumberLength = 16;
		AssertHasError(customFormat.SequenceNumberLengthInfo, errorMessage);
	});

	public void TestCheckCustomerReferenceLength() => CombineAssertions(() =>
	{
		const string errorMessage = "Arrival Customer Reference cannot exceed 22 characters.";

		customFormat.Prefix = "AAAA";
		customFormat.Suffix = "BBBB";
		customFormat.YearOption = "1";
		customFormat.SequenceNumberLength = 12;
		AssertNoRowError(customFormat, errorMessage);
		customFormat.Prefix = "AAAAA";
		AssertHasRowError(customFormat, errorMessage);
		customFormat.Prefix = "AAAA";
		customFormat.Suffix = "BBBBB";
		AssertHasRowError(customFormat, errorMessage);
		customFormat.Suffix = "BBBB";
		customFormat.YearOption = "2";
		AssertHasRowError(customFormat, errorMessage);
		customFormat.YearOption = "4";
		AssertHasRowError(customFormat, errorMessage);
		customFormat.YearOption = "1";
		customFormat.SequenceNumberLength = 13;
		AssertHasRowError(customFormat, errorMessage);
	});

	protected override void SetUp()
	{
		base.SetUp();
		var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
		customFormat = new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
	}

	CustomArrivalCustomerReferenceFormat customFormat;
}
