using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class CusEntryNumHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetAdditionalReferenceNumberTypes()
		{
			int defaultValuesCount = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value.Count;

			var numHelper = ObjectFactory.Get<Integration.Customs.ICusEntryNumHelper>();

			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes("IS").Count, Is.EqualTo(new IcelandAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.UnitedStates).Count, Is.EqualTo(new UnitedStatesAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.Australia).Count, Is.EqualTo(defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.UnitedArabEmirates).Count, Is.EqualTo(new UnitedArabEmiratesAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.Canada).Count, Is.EqualTo(new CanadaAdditionalReferenceNumberTypes().Count + defaultValuesCount - 2), "PCN, CCN already added to registry");
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.China).Count, Is.EqualTo(new ChinaAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.HongKong).Count, Is.EqualTo(new ChinaAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.Germany).Count, Is.EqualTo(new GermanyAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(Core.Constants.CountryCodes.Brazil).Count, Is.EqualTo(new BrazilAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			NUnit.Framework.Assert.That(numHelper.GetAdditionalReferenceNumberTypes(string.Empty).Count, Is.EqualTo(defaultValuesCount));
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalReferenceNumberCategory()
		{
			var numHelper = ObjectFactory.Get<Integration.Customs.ICusEntryNumHelper>();
			NUnit.Framework.Assert.That(numHelper.AdditionalReferenceNumberCategory, Is.EqualTo(CusEntryNumber.Categories.AdditionalReferenceNumber).Using(CustomComparers.TypeComparison), "Should be same.");
		}
	}
}
