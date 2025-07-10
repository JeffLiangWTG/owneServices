using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class CustomsDocDataObjectProviderHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetProvider()
		{
			AssertValidProvider(Core.Constants.CountryCodes.UnitedStates, "USATF6A", null, "Enterprise.Customs.US.Business.CustomsDocDataObjectProvider");

			AssertValidProvider(Core.Constants.CountryCodes.EuropeanUnion, "ATRCertificate", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");

			CombineAssertions("NCTS CustomsDocDataObjectProviders", () =>
			{
				AssertValidProvider("NCTS", "WHATEVER", null, "Enterprise.Customs.EU.NCTS.Business.Documents.DocDataObjects.NctsCustomsDocDataObjectProvider");
				AssertValidProvider("FR.NCTS", "FRPortsRegularizationTransitDOA", "NctsHeader", "Enterprise.Customs.FR.Business.Documents.NctsCustomsDocDataObjectProvider");
			});

			CombineAssertions("Countries having their own CustomsDocDataObjectProvider at EntryHeader level", () =>
			{
				AssertValidProvider(Core.Constants.CountryCodes.France, "WHATEVER", null, "Enterprise.Customs.FR.Business.Documents.CustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.Italy, "WHATEVER", null, "Enterprise.Customs.IT.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.Spain, "WHATEVER", null, "Enterprise.Customs.ES.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.SouthAfrica, "WHATEVER", null, "Enterprise.Customs.ZA.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");
			});

			CombineAssertions("Countries without own CustomsDocDataObjectProvider at Entry Header level", () =>
			{
				AssertValidProvider(Core.Constants.CountryCodes.Germany, "WHATEVER", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.Ireland, "WHATEVER", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider");
			});

			CombineAssertions("CustomsDocDataObjectProviders at JobDeclaration level", () =>
			{
				AssertValidProvider(Core.Constants.CountryCodes.EuropeanUnion, "CMRWayBill", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.JobDeclarationCustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.Germany, "CMRWayBill", null, "Enterprise.Customs.DE.Business.Documents.DocDataObjects.JobDeclarationCustomsDocDataObjectProvider");

				AssertValidProvider(Core.Constants.CountryCodes.Latvia, "CMRWayBill", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.JobDeclarationCustomsDocDataObjectProvider");
				AssertValidProvider(Core.Constants.CountryCodes.Italy, "CMRWayBill", null, "Enterprise.Customs.EU.Business.Documents.DocDataObjects.JobDeclarationCustomsDocDataObjectProvider");
			});
		}

		[ExpectNoExceptions]
		void AssertValidProvider(string docDataObjectProviderSourceDictionaryKey, string dataContext, string businessObjectName, string expectedProvider)
		{
			var provider = CustomsDocDataObjectProviderHelper.GetProvider(docDataObjectProviderSourceDictionaryKey, dataContext, businessObjectName);
			NUnit.Framework.Assert.That(provider, Is.Not.EqualTo(default(ICustomsDocDataObjectProvider)));
			NUnit.Framework.Assert.That(provider.GetType().FullName, Is.EqualTo(expectedProvider));
		}
	}
}
