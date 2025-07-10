using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingVATCollection))]
	sealed class GuidedDecisionMakingVATCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingVATCollection>
	{
		public void TestLoad()
		{
			SetUpVATApplicabilities();

			var vatCollection = GetCollectionToTest();
			vatCollection.Load();

			AssertArrayEqualsByElements("Valid VATApplicability can be loaded with correct order(first VATRateValue, then VATCode, last AdditionalCode).", new[] { "A001-V002-V002 DESC-STD-STD DESC-0.033", "A001-V001-V001 DESC-RED-RED DESC-0.055", "A001-V002-V002 DESC-RED-RED DESC-0.055" }, vatCollection.Cast<GuidedDecisionMakingVAT>().Select(v => $"{v.Category}-{v.AdditionalCode}-{v.AdditionalCodeDescription}-{v.VATCode}-{v.Description}-{v.VATRateValue:F3}").ToArray());
			AssertArrayEqualsByElements("Captured VATCode from gdmBasic source that VATCode matched and additional code is provided in supplementary codes from gdmBasic source would be ticked initially.", new ZBool[] { false, false, true }, vatCollection.Cast<GuidedDecisionMakingVAT>().OrderBy(v => v.AdditionalCode).Select(x => x.IsTicked).ToArray());
		}

		public void TestLoad_VATApplicabitiesWithEmptyAdditionalCode()
		{
			SetUpVATApplicabilities_WithEmptyAdditionalCodes();
			var gDMBasicSource = new Mock<IGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("FR");
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.Today);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("1111111111");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("FR");
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>());
			gDMBasicSource.Setup(x => x.VATCode).Returns("RED");
			var gdmBasic = new GuidedDecisionMakingBasic(gDMBasicSource.Object, Factory);
			var vatCollection = new GuidedDecisionMakingVATCollection(gdmBasic);
			vatCollection.Load();

			AssertArrayEqualsByElements("Valid VATApplicability can be loaded with correct order(first VATRateValue, then VATCode, last AdditionalCode).", new[] { "A001--STD-STD DESC-0.033", "A001--RED-RED DESC-0.055", "A001--SRR-SRR DESC-0.088" }, vatCollection.Cast<GuidedDecisionMakingVAT>().Select(v => $"{v.Category}-{v.AdditionalCode}-{v.VATCode}-{v.AdditionalCodeDescription}-{v.VATRateValue:F3}").ToArray());

			AssertArrayEqualsByElements("Captured VATCode from gdmBasic source would be ticked initially even if no supplementary codes from gdmBasic source if its additional code is empty.", new ZBool[] { false, true, false }, vatCollection.Cast<GuidedDecisionMakingVAT>().OrderBy(v => v.AdditionalCode).Select(x => x.IsTicked).ToArray());

			void SetUpVATApplicabilities_WithEmptyAdditionalCodes()
			{
				var startDate = ZDate.Today.AddDays(-10);
				var endDate = ZDate.Today.AddDays(10);
				var eun = RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
				RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);

				var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EuropeanUnion, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
				Factory.Save();
				var tariff = RefDataHelper.CreateTariff(Core.Constants.CountryCodes.EuropeanUnion, tariffType.PK, "1111111111", startDate, endDate, "dummy tariff1");
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "RED", "", startDate, endDate, category: "A001");
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "STD", "", startDate, endDate, category: "A001");
				RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "SRR", "", startDate, endDate, category: "A001");
				RefDataHelper.CreateTaxOrFee("RED", 0.055, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
				RefDataHelper.CreateTaxOrFee("STD", 0.033, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
				RefDataHelper.CreateTaxOrFee("SRR", 0.088, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
				Factory.Save();
			}
		}

		void SetUpVATApplicabilities()
		{
			var startDate = ZDate.Today.AddDays(-10);
			var endDate = ZDate.Today.AddDays(10);
			var eun = RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);

			var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EuropeanUnion, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();
			var tariff = RefDataHelper.CreateTariff(Core.Constants.CountryCodes.EuropeanUnion, tariffType.PK, "1111111111", startDate, endDate, "dummy tariff1");
			RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "RED", "V001", startDate, endDate, description: "V001 DESC", category: "A001");
			RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "STD", "V002", startDate, endDate, description: "V002 DESC", category: "A001");
			RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "RED", "V002", startDate, endDate, description: "V002 DESC", category: "A001");

			var tariff2 = RefDataHelper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "2222222222", startDate, endDate, "dummy tariff2");
			RefDataHelper.CreateNewOrGetExistingVATApplicability(tariff2, Core.Constants.CountryCodes.France, "RED", "V004", startDate, endDate, description: "V004 DESC", category: "A001");

			RefDataHelper.CreateTaxOrFee("RED", 0.055, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
			RefDataHelper.CreateTaxOrFee("STD", 0.033, Core.Constants.CountryCodes.EuropeanUnion, startDate, endDate);
			Factory.Save();
		}

		protected override GuidedDecisionMakingVATCollection GetCollectionToTest()
		{
			var gDMBasicSource = new Mock<IGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("FR");
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.Today);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("1111111111");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("FR");
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>() { "V002" });
			gDMBasicSource.Setup(x => x.VATCode).Returns("RED");
			var gdmBasic = new GuidedDecisionMakingBasic(gDMBasicSource.Object, Factory);

			var vatCollection = new GuidedDecisionMakingVATCollection(gdmBasic);
			return vatCollection;
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			return new GuidedDecisionMakingVAT(gdmBasic);
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
