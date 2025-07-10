using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class ValidationHelperTest : TestCaseWithFactory
{
	public void TestCheckWithinRange()
	{
		var dummy = Factory.New<DummyBusinessObject>();
		dummy.Z0_CodeInfo.AdditionalValidation += () => ValidationHelper.CheckWithinRange((ZPropertyInfoString)dummy.Z0_CodeInfo, 1, 999);
		CombineAssertions(() =>
		{
			TestCheckWithinRange((ZPropertyInfoString)dummy.Z0_CodeInfo, 1, 999);
		});
	}

	public static void TestCheckWithinRange(ZPropertyInfoString propertyInfo, int minimumAcceptableValue, int maximumAcceptableValue)
	{
		var expectedMessage = ValidationMessages.Shared.GetFieldIsNotWithinRangeMessage(propertyInfo.HumanReadableName, minimumAcceptableValue, maximumAcceptableValue);
		var invalidValueInRange = new[] { ".", ",", ":", "-", "!", "A", "/", "<", "{", "2.1", (minimumAcceptableValue - 1).ToString(), (maximumAcceptableValue + 1).ToString() };
		var validValueInRange = new[] { "", minimumAcceptableValue.ToString(), maximumAcceptableValue.ToString(), (minimumAcceptableValue + 1).ToString(), (maximumAcceptableValue - 1).ToString() };

		foreach (var invalidValue in invalidValueInRange)
		{
			if (invalidValue.Length <= propertyInfo.MaxLength)
			{
				propertyInfo.Value = invalidValue;
				AssertHasMessageError($"When {invalidValue} entered", propertyInfo, expectedMessage);
			}
		}
		foreach (var validValue in validValueInRange)
		{
			if (validValue.Length <= propertyInfo.MaxLength)
			{
				propertyInfo.Value = validValue;
				AssertNoMessageErrors($"When {validValue} entered", propertyInfo);
			}
		}
	}
}
