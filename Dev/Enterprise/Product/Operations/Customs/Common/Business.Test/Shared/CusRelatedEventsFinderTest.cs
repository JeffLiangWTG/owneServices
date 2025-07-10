using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class CusRelatedEventsFinderTest : TestCase
	{
		public void GetCusRelatedBusinessObjectsProvider()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedBizOsProvider(Core.Constants.CountryCodes.Australia).GetType().FullName, Is.EqualTo("Enterprise.Customs.AU.Declaration.Business.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedBizOsProvider(Core.Constants.CountryCodes.NewZealand).GetType().FullName, Is.EqualTo("Enterprise.Customs.NZ.Business.Express.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedBizOsProvider(Core.Constants.CountryCodes.Canada).GetType().FullName, Is.EqualTo("Enterprise.Customs.CA.Business.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedBizOsProvider(Core.Constants.CountryCodes.UnitedStates).GetType().FullName, Is.EqualTo("Enterprise.Customs.Business.CusRelatedEventsProvider"));
			});
		}

		[ExpectNoExceptions]
		public void TestGetRelatedMasterBillsProvider()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedParentBizOsProvider(Core.Constants.CountryCodes.Australia).GetType().FullName, Is.EqualTo("Enterprise.Customs.AU.Declaration.Business.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedParentBizOsProvider(Core.Constants.CountryCodes.NewZealand).GetType().FullName, Is.EqualTo("Enterprise.Customs.NZ.Business.Express.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedParentBizOsProvider(Core.Constants.CountryCodes.Canada).GetType().FullName, Is.EqualTo("Enterprise.Customs.CA.Business.CusRelatedEventsProvider"));
				NUnit.Framework.Assert.That(CusRelatedEventsFinder.GetCusRelatedParentBizOsProvider(Core.Constants.CountryCodes.UnitedStates).GetType().FullName, Is.EqualTo("Enterprise.Customs.Business.CusRelatedEventsProvider"));
			});
		}
	}
}
