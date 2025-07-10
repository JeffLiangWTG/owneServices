using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	[TestedType(typeof(GuidedDecisionMakingVATCollection))]
	sealed class GuidedDecisionMakingVATCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingVATCollection>
	{
		public void TestLoad()
		{
			SetUpVATApplicabilities(Factory);
			var vatCollection = GetCollectionToTest();
			vatCollection.Load();

			AssertArrayEqualsByElements("Only VATs in qualified trade group can be loaded.", new ZString[] { "V002" }, vatCollection.Cast<GuidedDecisionMakingVAT>().Select(v => v.AdditionalCode).ToArray());
		}

		internal static void SetUpVATApplicabilities(BusinessObjectFactory factory)
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDate.Today.AddDays(-10);
			var endDate = ZDate.Today.AddDays(10);
			var eun = refDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			refDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
			var tradeGroup1 = refDataHelper.LoadOrCreateTradeGroup("FR", "CONTI");
			var tradeGroup2 = refDataHelper.LoadOrCreateTradeGroup("FR", "MGPRE");

			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EuropeanUnion, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			factory.Save();
			var tariff = refDataHelper.CreateTariff(Core.Constants.CountryCodes.EuropeanUnion, tariffType.PK, "1111111111", startDate, endDate, "dummy tariff1");
			refDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "RED", "V001", startDate, endDate, description: "V001 DESC", category: "A001", tradeGroup: tradeGroup1.PK);
			refDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "STD", "V002", startDate, endDate, description: "V002 DESC", category: "A001", tradeGroup: tradeGroup2.PK);

			refDataHelper.CreateTaxOrFee("RED", 0.055, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
			refDataHelper.CreateTaxOrFee("STD", 0.033, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
		}

		protected override GuidedDecisionMakingVATCollection GetCollectionToTest()
		{
			var gDMBasicSource = new Mock<IGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("FR");
			gDMBasicSource.Setup(x => x.RegionOrTerritoryOfDestination).Returns("MGPRE");
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.Today);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("1111111111");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("FR");
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>() { "V001" });
			var gdmBasic = new GuidedDecisionMakingBasic(gDMBasicSource.Object, Factory);

			var vatCollection = new GuidedDecisionMakingVATCollection(gdmBasic);
			return vatCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var gdmBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
			return new GuidedDecisionMakingVAT(gdmBasic);
		}
	}
}
