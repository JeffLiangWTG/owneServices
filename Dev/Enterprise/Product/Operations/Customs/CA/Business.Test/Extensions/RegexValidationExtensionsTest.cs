using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RegexValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestHasCharactersNotSupportedByCAMessaging()
		{
			var temp = "123ABCabc- .,()/='+:_#$@[`]~{}?\"%&*;<>";
			Assert(!RegexValidationExtensions.HasCharactersNotSupportedByCAMessaging(temp));

			temp = "–"; // Character Code 8211
			Assert(RegexValidationExtensions.HasCharactersNotSupportedByCAMessaging(temp));
			temp = "!";
			Assert(RegexValidationExtensions.HasCharactersNotSupportedByCAMessaging(temp));
			temp = "|";
			Assert(RegexValidationExtensions.HasCharactersNotSupportedByCAMessaging(temp));
			temp = "^";
			Assert(!RegexValidationExtensions.HasCharactersNotSupportedByCAMessaging(temp));
		}

		public void TestHasInvalidSymbolsForEdiFactMessaging()
		{
			ZString temp = "A+1";
			Assert(temp.HasInvalidSymbolsForCCN());

			temp = "A:1";
			Assert(temp.HasInvalidSymbolsForCCN());

			temp = "A'1";
			Assert(temp.HasInvalidSymbolsForCCN());

			temp = "A 1";
			Assert(!temp.HasInvalidSymbolsForCCN());

			temp = "A1";
			Assert(!temp.HasInvalidSymbolsForCCN());

			temp = "A-1";
			Assert(!temp.HasInvalidSymbolsForCCN());

			temp = "a1";
			Assert(!temp.HasInvalidSymbolsForCCN());
		}
	}
}
