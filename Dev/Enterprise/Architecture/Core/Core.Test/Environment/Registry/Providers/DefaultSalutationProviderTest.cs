using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DefaultSalutationProviderTest : TestCase
	{
		public void TestDefaultSalutationIsEmptyWhenLanguageNotSpecified()
		{
			AssertEquals(string.Empty, DefaultSalutationProvider.GetDefaultSalutation("", Enterprise.Core.Constants.SalutationGenders.Man));
		}

		public void TestDefaultSalutationWhenLanguageIsSpecified()
		{
			AssertEquals(Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(Enterprise.Core.Constants.Languages.English),
				DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.English, ""));
			AssertEquals(Enterprise.Core.Constants.DefaultSalutations.DearMale.ToString(Enterprise.Core.Constants.Languages.English),
				DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.English, Enterprise.Core.Constants.SalutationGenders.Man));

			AssertEquals(Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(Enterprise.Core.Constants.Languages.ChineseSimplified),
				DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.ChineseSimplified, ""));
			AssertEquals(Enterprise.Core.Constants.DefaultSalutations.DearFemale.ToString(Enterprise.Core.Constants.Languages.ChineseSimplified),
				DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.ChineseSimplified, Enterprise.Core.Constants.SalutationGenders.Woman));
		}
	}
}
