using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingVATCollection))]
	sealed class GuidedDecisionMakingVATCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingVATCollection>
	{
		public void TestLoad_NotCanaryIsland()
		{
			TestLoad(new ZString[] { "EX", "IV1", "IV2" });
		}

		public void TestLoad_CanaryIsland()
		{
			TestLoad(new ZString[] { "EX", "IG3" }, isDestinationCanaryIsland: true);
		}

		void TestLoad(ZString[] expectedVATCodes, bool isDestinationCanaryIsland = false)
		{
			SetUpVATApplicabilities();
			var vatCollection = GetCollection(isDestinationCanaryIsland);
			vatCollection.Load();

			AssertArrayEqualsByElements("VATApplicabilities are correct", expectedVATCodes, vatCollection.Cast<GuidedDecisionMakingVAT>().Select(v => v.VATCode).ToArray());
		}

		void SetUpVATApplicabilities()
		{
			var countryCode = Core.Constants.CountryCodes.Spain;
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("IV1", 0.21, countryCode);
			helper.CreateTaxOrFee("IV2", 0.21, countryCode);
			helper.CreateTaxOrFee("IG3", 0.21, countryCode);
			var tariffType = helper.CreateTariffType(countryCode, "IMP");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV1");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV2");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IG3");
			Factory.Save();
		}

		GuidedDecisionMakingVATCollection GetCollection(ZBool isDestinationCanaryIsland)
		{
			var gDMBasicSource = new Mock<IESGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("ES");
			gDMBasicSource.Setup(x => x.DestinationStateIsCanaryIsland).Returns(isDestinationCanaryIsland);
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.Today);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("11112222");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("ES");
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>() { "V001" });
			var gdmBasic = new GuidedDecisionMakingBasic(gDMBasicSource.Object, Factory);

			var vatCollection = new GuidedDecisionMakingVATCollection(gdmBasic);
			return vatCollection;
		}

		protected override GuidedDecisionMakingVATCollection GetCollectionToTest() => GetCollection(false);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var gdmBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
			return new GuidedDecisionMakingVAT(gdmBasic);
		}
	}
}
