using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	abstract class DeclarationTypeBasedEntryStyleCalculationStrategyTest<TStrategy> : TestCaseWithFactory
		where TStrategy : DeclarationTypeBasedEntryStyleCalculationStrategy
	{
		public void TestConstructor()
		{
			var originCountry = Factory.New<RefCountry>();
			AssertExceptionThrown<ArgumentNullException>("When Factory is null", () => GetNewStrategy(factory: null, ZString.Empty, originCountry, fallbackInfoProviderMock.Object));
			AssertExceptionThrown<ArgumentNullException>("When fallbackInfoProvider is null", () => GetNewStrategy(Factory, ZString.Empty, originCountry, fallbackInfoProvider: null));
			AssertNoExceptionThrown(() => GetNewStrategy(Factory, ZString.Empty, originCountry, fallbackInfoProviderMock.Object));
		}

		public void TestCalculateWithFallbackWhenOriginCountryIsNull()
		{
			fallbackInfoProviderMock.Setup(m => m.GetEntryStyleForInwardProcessingVATPayment()).Returns("AA");
			var strategy = GetNewStrategy(Factory, "", null, fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "AA", strategy.Calculate());
		}

		public abstract void TestCalculateBasedOn15And17CodeTypes();

		public void TestCalculateWithFallbackWhenCountryCodeIsEqualToTheJobDeclarationOne()
		{
			fallbackInfoProviderMock.Setup(m => m.GetEntryStyleForInwardProcessingVATPayment()).Returns("AA");
			var originCountry = Factory.New<RefCountry>();
			originCountry.RN_Code = "AA";
			var strategy = GetNewStrategy(Factory, "AA", originCountry, fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "AA", strategy.Calculate());
		}

		public void TestCalculateWithFallbackWhenCountryIsEligibleToACommonTransitProcedureButNotMemberOfEU_NonUCC6()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, Core.Constants.CountryCodes.Switzerland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			fallbackInfoProviderMock.Setup(m => m.GetEntrySubStyleForCommonTransit(It.IsAny<RefCountry>())).Returns("AA");
			var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "AA", strategy.Calculate());
		}

		public void TestCalculateWithFallbackWhenCountryIsEligibleToACommonTransitProcedureButNotMemberOfEU_IsUCC6()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, Core.Constants.CountryCodes.Switzerland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland), jobDeclaration);
				Assert("EntryStyle not EU", strategy.Calculate() != "EU");
			}
		}

		public void TestCalculateWithFallbackWhenCountryIsASpecialTerritoryOfTheCommunity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var eusft = helper.CreateTradeGroup("EUN", "EUSFT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusft, Core.Constants.CountryCodes.FrenchGuyana, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.FrenchGuyana), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "CO", strategy.Calculate());
		}

		public void TestCalculateWithFallbackWhenCountryIsMemberOfEUAndHasSpecialTerritoriesOfTheCommunity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var eusfr = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusfr, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "CO", strategy.Calculate());
		}

		public void TestCalculateWithFallbackWhenCountryIsMemberOfEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			fallbackInfoProviderMock.Setup(m => m.GetEntryStyleForInwardProcessingVATPayment()).Returns("AA");
			var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "AA", strategy.Calculate());
		}

		public void TestCalculateWithFallbackDefaultData()
		{
			var strategy = GetNewStrategy(Factory, "", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", ExpectedDefaultEntryStyle, strategy.Calculate());
		}

		protected abstract ZString ExpectedDefaultEntryStyle { get; }

		protected abstract IEntryStyleCalculationStrategy GetNewStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider);

		protected override void SetUp()
		{
			base.SetUp();
			fallbackInfoProviderMock = new Mock<IEntryStyleCalculatorFallbackInfoProvider>();
		}

		Mock<IEntryStyleCalculatorFallbackInfoProvider> fallbackInfoProviderMock;
	}
}
