using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class DangerousGoodsHelperTest : TestCaseWithFactory
	{
		public void TestDangerousGoodsName()
		{
			var substance = DGSubstanceTestHelper.Create("0000", "A", "IMO", additionalInitialisation: (subs) =>
			{
				subs.DG_FlashPoint = "-4 cc";
				subs.DG_PSN = "I am very dangerous";
			});

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance.PK;

			var dangerousGoodsName = DangerousGoodsHelper.GetDangerousGoodsName(undg);
			AssertEquals("I am very dangerous", dangerousGoodsName);

			var engName = undg.Substance.Names.AddNew();
			engName.DA_Language = Core.Constants.Languages.English;
			engName.DA_Descriptor = "English 1";
			dangerousGoodsName = DangerousGoodsHelper.GetDangerousGoodsName(undg);
			AssertEquals("English 1", dangerousGoodsName);

			var chsName = undg.Substance.Names.AddNew();
			chsName.DA_Language = Core.Constants.Languages.ChineseSimplified;
			chsName.DA_Descriptor = "Chinese 1";
			dangerousGoodsName = DangerousGoodsHelper.GetDangerousGoodsName(undg);
			AssertEquals("Chinese 1", dangerousGoodsName);
		}

		public void TestIsDangerousChemical()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			anotherFactory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeListAttribute(codeList.PK, "Alias", "液氨");
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "China Customs Tariff Additional Elements");
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "00005", "CAS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute("AdditionalInfo1", "00005", tariff);
			anotherFactory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.XC_GoodsSpecModel = "7664-41-7";

			var isDangerousChemical = DangerousGoodsHelper.IsDangerousChemical(Factory, invoiceLine.AdditionalInformationHelper, invoiceLine.EffectiveAssessmentDate);
			Assert(isDangerousChemical);

			invoiceLine.XC_GoodsSpecModel = "";
			invoiceLine.JI_NameOfGoods = "氨";
			isDangerousChemical = DangerousGoodsHelper.IsDangerousChemical(Factory, invoiceLine.AdditionalInformationHelper, invoiceLine.EffectiveAssessmentDate);
			Assert(isDangerousChemical);

			invoiceLine.JI_NameOfGoods = "液氨";
			isDangerousChemical = DangerousGoodsHelper.IsDangerousChemical(Factory, invoiceLine.AdditionalInformationHelper, invoiceLine.EffectiveAssessmentDate);
			Assert(invoiceLine.GoodsIsDangerousChemical);
		}
	}
}
