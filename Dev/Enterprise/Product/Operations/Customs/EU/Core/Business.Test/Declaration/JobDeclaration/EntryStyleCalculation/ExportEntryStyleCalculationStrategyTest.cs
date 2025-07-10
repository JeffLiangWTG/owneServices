using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class ExportEntryStyleCalculationStrategyTest : DeclarationTypeBasedEntryStyleCalculationStrategyTest<ExportEntryStyleCalculationStrategy>
	{
		public override void TestCalculateBasedOn15And17CodeTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "EX17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU17");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "KP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "BN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "TJ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.KoreaNorth), fallbackInfoProviderMock.Object);
				AssertEquals("EntryStyle", "CO", strategy.Calculate());

				strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brunei), fallbackInfoProviderMock.Object);
				AssertEquals("EntryStyle", "EX", strategy.Calculate());

				strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Tajikistan), fallbackInfoProviderMock.Object);
				AssertEquals("EntryStyle", "EU", strategy.Calculate());
			});
		}

		public void TestCalculateBasedOn15And17CodeTypes_UCC6()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "EX17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU17");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "KP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "BN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "TJ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
				{
					var strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.KoreaNorth), jobDeclaration);
					AssertEquals("EntryStyle", "CO", strategy.Calculate());

					strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brunei), jobDeclaration);
					AssertEquals("EntryStyle", "EX", strategy.Calculate());

					strategy = GetNewStrategy(Factory, Core.Constants.CountryCodes.Latvia, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Tajikistan), jobDeclaration);
					AssertEquals("EntryStyle", "EX", strategy.Calculate());
				}
			});
		}

		protected override ZString ExpectedDefaultEntryStyle => EntryStyleListExport.Codes.ExportNormal;

		protected override IEntryStyleCalculationStrategy GetNewStrategy(BusinessObjectFactory factory, ZString declarationCountryCode, RefCountry originCountry, IEntryStyleCalculatorFallbackInfoProvider fallbackInfoProvider)
		{
			return new ExportEntryStyleCalculationStrategy(factory, declarationCountryCode, originCountry, fallbackInfoProvider);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fallbackInfoProviderMock = new Mock<IEntryStyleCalculatorFallbackInfoProvider>();
		}

		Mock<IEntryStyleCalculatorFallbackInfoProvider> fallbackInfoProviderMock;
	}
}
