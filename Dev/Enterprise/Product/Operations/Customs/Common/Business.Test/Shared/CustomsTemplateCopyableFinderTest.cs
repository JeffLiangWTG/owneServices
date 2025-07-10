using NUnit.Framework;

namespace Enterprise.Customs.Common.Shared.Testing
{
	class CustomsTemplateCopyableProviderTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCountrySpecificCopyableProvider()
		{
			NUnit.Framework.Assert.That(CustomsTemplateCopyableFinder.GetCustomsTemplateCopyableProvider(Core.Constants.CountryCodes.UnitedStates).GetType().FullName, Is.EqualTo("Enterprise.Customs.US.Business.CustomsTemplateCopyableProvider"));
			NUnit.Framework.Assert.That(CustomsTemplateCopyableFinder.GetCustomsTemplateCopyableProvider(Core.Constants.CountryCodes.PuertoRico).GetType().FullName, Is.EqualTo("Enterprise.Customs.US.Business.CustomsTemplateCopyableProvider"));
			NUnit.Framework.Assert.That(CustomsTemplateCopyableFinder.GetCustomsTemplateCopyableProvider(Core.Constants.CountryCodes.Australia), Is.EqualTo(default(Integration.Customs.Shared.ICustomsTemplateCopyableProvider)));
		}
	}
}
