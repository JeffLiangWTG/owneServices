using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class CHMod11ValidatorTest : TestCaseWithFactory
{
	public void TestIsValidPattern()
	{
		var validPrefixes = new List<string> { "CHE", "CHE-", "E-", "E" };
		var validNumbers = new List<string> { "105908410", "105.908.410" };
		var validSuffixes = new List<string> { "", " MWST", " TVA", " IVA", "MWST", "TVA", "IVA" };
		var invalidPrefixes = new List<string> { "", "CHE.", "AAA", "XXX-" };
		var invalidNumbers = new List<string> { "105-908.410", "10.59.08.41", "105.9a8.410", "105.9X8410", "105.998.410.410" };
		var invalidSuffixes = new List<string> { "VAT", " VAT", " XXX", "Z" };

		UidTest(validPrefixes, validNumbers, validSuffixes, true);
		UidTest(invalidPrefixes, validNumbers, validSuffixes, false);
		UidTest(validPrefixes, invalidNumbers, validSuffixes, false);
		UidTest(validPrefixes, validNumbers, invalidSuffixes, false);
		UidTest(invalidPrefixes, invalidNumbers, validSuffixes, false);
		UidTest(validPrefixes, invalidNumbers, invalidSuffixes, false);
		UidTest(invalidPrefixes, validNumbers, invalidSuffixes, false);
		UidTest(invalidPrefixes, invalidNumbers, invalidSuffixes, false);

		void UidTest(List<string> prefixes, List<string> numbers, List<string> suffixes, bool expected)
		{
			foreach (var prefix in prefixes)
			{
				foreach (var number in numbers)
				{
					foreach (var suffix in suffixes)
					{
						var uid = prefix + number + suffix;
						AssertEquals($"expected pattern validation for {uid}: {expected} ", expected, CHMod11Validator.IsValidPattern(uid));
					}
				}
			}
		}
	}

	public void TestIsValidCheckDigit()
	{
		var validPrefixes = new List<string> { "CHE", "CHE-", "E-", "E" };
		var validNumbers = new List<string> { "105908410", "105.908.410" };
		var validSuffixes = new List<string> { "", " MWST", "MWST", " TVA", " IVA" };
		var invalidNumbers = new List<string> { "109322557", "109.322.557", "10932255", "1093225578" };

		CheckDigitTest(validPrefixes, validNumbers, validSuffixes, true);
		CheckDigitTest(validPrefixes, invalidNumbers, validSuffixes, false);

		void CheckDigitTest(List<string> prefixes, List<string> numbers, List<string> suffixes, bool expected)
		{
			foreach (var prefix in prefixes)
			{
				foreach (var number in numbers)
				{
					foreach (var suffix in suffixes)
					{
						var uid = prefix + number + suffix;
						AssertEquals($"expected check digit for {uid}: {expected} ", expected, CHMod11Validator.IsValidCheckDigit(uid));
					}
				}
			}
		}
	}
}
