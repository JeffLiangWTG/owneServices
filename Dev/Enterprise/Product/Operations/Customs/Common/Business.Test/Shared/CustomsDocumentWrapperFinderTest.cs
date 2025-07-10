using NUnit.Framework;

namespace Enterprise.Customs.Common.Shared.Testing;

sealed class CustomsDocumentWrapperFinderTest : TestCase
{
	public void TestGetCustomsDocumentWrapperProvider()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Enterprise.Customs.TW.Business.CustomsDocumentWrapperProvider", CustomsDocumentWrapperFinder.GetCustomsDocumentWrapperProvider(Core.Constants.CountryCodes.Taiwan).GetType().FullName);
			AssertNull(CustomsDocumentWrapperFinder.GetCustomsDocumentWrapperProvider(Core.Constants.CountryCodes.Australia));
		});
	}
}
