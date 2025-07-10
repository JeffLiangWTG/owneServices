using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class ImportEntryStyleCalculationStrategyTest : DeclarationTypeBasedEntryStyleCalculationStrategyTest<ImportEntryStyleCalculationStrategy>
	{
		public override void TestCalculateBasedOn15And17CodeTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "CO15");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "IM15");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "EU15");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "MD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "LA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedArabEmirates), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "CO", strategy.Calculate());

			strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Moldova), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "IM", strategy.Calculate());

			strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic), fallbackInfoProviderMock.Object);
			AssertEquals("EntryStyle", "EU", strategy.Calculate());
		}

		protected override ZString ExpectedDefaultEntryStyle => EntryStyleListImport.Codes.ImportNormal;

		protected override IEntryStyleCalculationStrategy GetNewStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider)
		{
			return new ImportEntryStyleCalculationStrategy(factory, declarationCountryCode, originCountry, fallbackInfoProvider);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fallbackInfoProviderMock = new Mock<IEntryStyleCalculatorFallbackInfoProvider>();
		}

		Mock<IEntryStyleCalculatorFallbackInfoProvider> fallbackInfoProviderMock;
	}
}
