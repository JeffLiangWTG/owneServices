using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.EU.Testing
{
	sealed class MRNFormatValidatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCheckCountryCode()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("11ZZ11111111111110", Factory, ZString.Empty), Is.EqualTo("MRN does not contain a valid country/region code").Using(CustomComparers.TypeComparison), "Valid");
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("11DE11111111111115", Factory, ZString.Empty), Is.EqualTo(ZString.Empty), "Invalid");
			});
		}

		[ExpectNoExceptions]
		public void TestCheckDigit()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("11DE11111111111111", Factory, ZString.Empty), Is.EqualTo("MRN does not have a valid last digit. The last digit should be 5").Using(CustomComparers.TypeComparison), "Valid");
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("11DE11111111111115", Factory, ZString.Empty), Is.EqualTo(ZString.Empty), "Invalid");
			});
		}

		[ExpectNoExceptions]
		public void TestCheckMRNFormatError()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("INVALID", Factory, ZString.Empty).ToString(), Does.Contain("Please enter a MRN in the following format with only numbers and upper case letters:"), "Valid");
				NUnit.Framework.Assert.That(MRNFormatValidator.CheckMRNFormat("11DE11111111111115", Factory, ZString.Empty), Is.EqualTo(ZString.Empty), "Invalid");
			});
		}
	}
}
