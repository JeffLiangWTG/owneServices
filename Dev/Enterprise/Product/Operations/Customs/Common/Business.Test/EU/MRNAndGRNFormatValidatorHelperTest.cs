using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Common.EU.Testing
{
	sealed class MRNAndGRNFormatValidatorHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsGRNDigitValid()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsGRNDigitValid("28CH1234GE0000307").isGRNDigitValid, Is.EqualTo(false));
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsGRNDigitValid("28CH1234GE0000337").isGRNDigitValid, Is.EqualTo(true));
			});
		}

		[ExpectNoExceptions]
		public void TestIsMRNDigitValid()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("23IT123456789015J1").isMRNDigitValid, Is.EqualTo(false));
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsMRNDigitValid("23IT123456789012J1").isMRNDigitValid, Is.EqualTo(true));
			});
		}

		[ExpectNoExceptions]
		public void TestIsCountryCodeValid()
		{
			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = "DE";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsCountryCodeValid("DE", Factory), Is.EqualTo(true));
				NUnit.Framework.Assert.That(MRNAndGRNFormatValidatorHelper.IsCountryCodeValid("XX", Factory), Is.EqualTo(false));
			});
		}
	}
}
