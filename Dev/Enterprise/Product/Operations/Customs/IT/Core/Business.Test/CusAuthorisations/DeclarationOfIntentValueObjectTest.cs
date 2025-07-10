using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DeclarationOfIntentValueObjectTest : TestCase
{
	public void TestProperties_EmptyAuthorisationNumber()
	{
		var valueObject = new DeclarationOfIntentValueObject(ZString.Empty);
		AssertProperties(valueObject, ZString.Empty, ZBool.False, ZDateTime.Empty);
	}

	public void TestProperties_ValidAuthorisationNumber()
	{
		var valueObject = new DeclarationOfIntentValueObject(ValidAuthorisationNumber);
		AssertProperties(valueObject, ValidAuthorisationNumber, ZBool.False, new ZDateTime(2020, 12, 31, 11, 22, 33));
	}

	public void TestProperties_PlaceholderAuthorisationNumber()
	{
		var valueObject = new DeclarationOfIntentValueObject(PlaceholderAuthorisationNumber);
		AssertProperties(valueObject, PlaceholderAuthorisationNumber, ZBool.True, ZDateTime.Empty);
	}

	public void TestProperties_InvalidAuthorisationNumber()
	{
		var valueObject = new DeclarationOfIntentValueObject(InvalidAuthorisationNumber);
		AssertProperties(valueObject, InvalidAuthorisationNumber, ZBool.False, ZDateTime.Empty);
	}

	public void TestIsValid_EmptyAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			Assert($"['', true]", !IsDOINumberValid(ZString.Empty, true));
			Assert($"['', false]", !IsDOINumberValid(ZString.Empty, false));
		});
	}

	public void TestIsValid_ValidAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			Assert($"['{ValidAuthorisationNumber}', true]", IsDOINumberValid(ValidAuthorisationNumber, true));
			Assert($"['{ValidAuthorisationNumber}', false]", IsDOINumberValid(ValidAuthorisationNumber, false));
		});
	}

	public void TestIsValid_PlaceholderAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			Assert($"['{PlaceholderAuthorisationNumber}', true]", IsDOINumberValid(PlaceholderAuthorisationNumber, true));
			Assert($"['{PlaceholderAuthorisationNumber}', false]", !IsDOINumberValid(PlaceholderAuthorisationNumber, false));
			Assert($"['Y', true]", !IsDOINumberValid("Y", true));
		});
	}

	public void TestIsValid_InvalidAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			Assert($"['{InvalidAuthorisationNumber}', true]", !IsDOINumberValid(InvalidAuthorisationNumber, true));
			Assert($"['{InvalidAuthorisationNumber}', false]", !IsDOINumberValid(InvalidAuthorisationNumber, false));

			const string validFirstBlock = "201231112233";
			const string invalidFirstBlock = "AABBCCDDEEFF";
			const string validSecondBlock = "12345123456";
			const string invalidSecondBlock = "AAAAABBBBBB";
			Assert("Invalid length", !IsDOINumberValid(validFirstBlock, true));
			Assert("Valid length, both blocks invalid", !IsDOINumberValid(invalidFirstBlock + invalidSecondBlock, true));
			Assert("Valid length, wrong first block", !IsDOINumberValid(invalidFirstBlock + validSecondBlock, true));
			Assert("Valid length, wrong second block", !IsDOINumberValid(validFirstBlock + invalidSecondBlock, true));
			Assert("Valid length, both blocks valid", IsDOINumberValid(validFirstBlock + validSecondBlock, true));
		});
	}

	void AssertProperties(DeclarationOfIntentValueObject valueObject, ZString expectedDOINumber, ZBool expectedIsPlaceholder, ZDateTime expectedIssueDate)
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(valueObject.DOINumber), expectedDOINumber, valueObject.DOINumber);
			AssertEquals(nameof(valueObject.IsPlaceholder), expectedIsPlaceholder, valueObject.IsPlaceholder);
			AssertEquals(nameof(valueObject.IssueDate), expectedIssueDate, valueObject.IssueDate);
		});
	}

	ZBool IsDOINumberValid(ZString doiNumber, ZBool considerPlaceholderValidValue)
	{
		var valueObject = new DeclarationOfIntentValueObject(doiNumber);
		return valueObject.IsValid(considerPlaceholderValidValue);
	}

	const string ValidAuthorisationNumber = "20123111223312345123456";
	const string PlaceholderAuthorisationNumber = "X";
	const string InvalidAuthorisationNumber = "AABBCCDDEEFFAAAAABBBBBB";
}
