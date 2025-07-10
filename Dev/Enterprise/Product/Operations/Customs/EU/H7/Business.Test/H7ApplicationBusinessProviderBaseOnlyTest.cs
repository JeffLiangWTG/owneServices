using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(H7ApplicationBusinessProvider))]
	sealed class H7ApplicationBusinessProviderBaseOnlyTest : H7ApplicationBusinessProviderTest<H7ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestType()
		{
			AssertType<H7ApplicationBusinessProvider>("Ensure that the default country is served by this H7ApplicationBusinessProvider", Factory.NewWithValidTestData<AsycudaManifestHeader>().ApplicationBusinessProvider);
		}

		protected override IEnumerable<string> ExpectedCountryCodes => new List<string>
		{
			Core.Constants.CountryCodes.Austria,
			Core.Constants.CountryCodes.Belgium,
			Core.Constants.CountryCodes.Bulgaria,
			Core.Constants.CountryCodes.Croatia,
			Core.Constants.CountryCodes.Cyprus,
			Core.Constants.CountryCodes.CzechRepublic,
			Core.Constants.CountryCodes.Denmark,
			Core.Constants.CountryCodes.Estonia,
			Core.Constants.CountryCodes.Finland,
			Core.Constants.CountryCodes.Germany,
			Core.Constants.CountryCodes.Greece,
			Core.Constants.CountryCodes.Hungary,
			Core.Constants.CountryCodes.Latvia,
			Core.Constants.CountryCodes.Lithuania,
			Core.Constants.CountryCodes.Luxembourg,
			Core.Constants.CountryCodes.Malta,
			Core.Constants.CountryCodes.Netherlands,
			Core.Constants.CountryCodes.Poland,
			Core.Constants.CountryCodes.Portugal,
			Core.Constants.CountryCodes.Romania,
			Core.Constants.CountryCodes.Slovakia,
			Core.Constants.CountryCodes.Slovenia,
			Core.Constants.CountryCodes.Sweden,
		};
	}
}
