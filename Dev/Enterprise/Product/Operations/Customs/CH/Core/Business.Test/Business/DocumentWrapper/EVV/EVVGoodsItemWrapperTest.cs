using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemWrapper))]
sealed class EVVGoodsItemWrapperTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
	{
		var dutyOrTaxMock = new Mock<IEvvGoodsItem>();
		AssertExceptionThrown<ArgumentNullException>("Null message", () => EVVGoodsItemWrapper.New(null, Factory, SwissCustomsLanguageList.Codes.German));
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemWrapper.New(dutyOrTaxMock.Object, null, SwissCustomsLanguageList.Codes.German));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		GoodsItemMock.Setup(x => x.CustomsItemNumber).Returns("12");
		GoodsItemMock.Setup(x => x.Description).Returns("desc");
		GoodsItemMock.Setup(x => x.CommodityCode).Returns("1234.5678");
		GoodsItemMock.Setup(x => x.OriginCountry).Returns("NL");
		GoodsItemMock.Setup(x => x.NetMass).Returns(1);
		GoodsItemMock.Setup(x => x.NetMassConfirmation).Returns(ZBool.True);
		GoodsItemMock.Setup(x => x.GrossMass).Returns(1);
		GoodsItemMock.Setup(x => x.GrossMassConfirmation).Returns(ZBool.True);
		GoodsItemMock.Setup(x => x.AdditionalUnit).Returns(1);
		GoodsItemMock.Setup(x => x.AdditionalUnitConfirmation).Returns(ZBool.True);
		GoodsItemMock.Setup(x => x.CustomsNetWeight).Returns(1);
		GoodsItemMock.Setup(x => x.DutyAndTaxes).Returns(new IEvvGoodsItemDutyOrTax[] { new Mock<IEvvGoodsItemDutyOrTax>().Object });
		GoodsItemMock.Setup(x => x.Permits).Returns(new IEvvGoodsItemPermit[] { new Mock<IEvvGoodsItemPermit>().Object });
		GoodsItemMock.Setup(x => x.Packagings).Returns(new IEvvGoodsItemPackaging[] { new Mock<IEvvGoodsItemPackaging>().Object });
		GoodsItemMock.Setup(x => x.ProducedDocuments).Returns(new IEvvGoodsItemProducedDocument[] { new Mock<IEvvGoodsItemProducedDocument>().Object });
		GoodsItemMock.Setup(x => x.SpecialMentions).Returns(new IEvvGoodsItemSpecialMention[] { new Mock<IEvvGoodsItemSpecialMention>().Object });
		GoodsItemMock.Setup(x => x.Details).Returns(new IEvvGoodsItemDetail[] { new Mock<IEvvGoodsItemDetail>().Object });

		AssertEquals("CustomsItemNumber", "12", GoodsItemWrapperDE.CustomsItemNumber);
		AssertEquals("Description", "desc", GoodsItemWrapperDE.Description);
		AssertEquals("CommodityCode", "1234.5678", GoodsItemWrapperDE.CommodityCode);
		AssertEquals("CountryOfOrigin", "NL", GoodsItemWrapperDE.CountryOfOrigin);
		AssertEquals("CountryOfOrigin", "NL", GoodsItemWrapperDE.CountryOfOrigin);
		AssertEquals("NetMass", 1m, GoodsItemWrapperDE.NetMass);
		AssertEquals("NetMassConfirmation", ZBool.True, GoodsItemWrapperDE.NetMassConfirmation);
		AssertEquals("GrossMass", 1m, GoodsItemWrapperDE.GrossMass);
		AssertEquals("GrossMassConfirmation", ZBool.True, GoodsItemWrapperDE.GrossMassConfirmation);
		AssertEquals("AdditionalUnit", 1m, GoodsItemWrapperDE.AdditionalUnit);
		AssertEquals("AdditionalUnitConfirmation", ZBool.True, GoodsItemWrapperDE.AdditionalUnitConfirmation);
		AssertEquals("CustomsNetWeight", 1m, GoodsItemWrapperDE.CustomsNetWeight);
		AssertEquals("DutyAndTaxes", 1, GoodsItemWrapperDE.DutyAndTaxes.Count);
		AssertEquals("Permits", 1, GoodsItemWrapperDE.Permits.Count);
		AssertEquals("Packagings", 1, GoodsItemWrapperDE.Packagings.Count);
		AssertEquals("ProducedDocuments", 1, GoodsItemWrapperDE.ProducedDocuments.Count);
		AssertEquals("SpecialMentions", 1, GoodsItemWrapperDE.SpecialMentions.Count);
		AssertEquals("GoodsItemWrapper", 1, GoodsItemWrapperDE.Details.Count);
	});

	public void TestIsPreference() => CombineAssertions(() =>
	{
		GoodsItemMock.Setup(m => m.IsOriginPreference).Returns(true);
		AssertEquals("IsOriginPreference=true", ZBool.True, GoodsItemWrapperDE.IsPreferenceOrigin);

		GoodsItemMock.Setup(m => m.IsOriginPreference).Returns(false);
		AssertEquals("IsOriginPreference=false", ZBool.False, GoodsItemWrapperDE.IsPreferenceOrigin);
	});

	public void TestIsCommercialGood() => CombineAssertions(() =>
	{
		GoodsItemStatisticMock.Setup(m => m.IsCommercialGood).Returns(true);
		AssertEquals("IsCommercialGood=true", ZBool.True, GoodsItemWrapperDE.IsCommercialGood);

		GoodsItemStatisticMock.Setup(m => m.IsCommercialGood).Returns(false);
		AssertEquals("IsCommercialGood=false", ZBool.False, GoodsItemWrapperDE.IsCommercialGood);

		GoodsItemMock.Setup(m => m.Statistic).Returns<IEvvGoodsItemStatistic>(null);
		AssertEquals("No statistics", ZBool.False, GoodsItemWrapperDE.IsCommercialGood);
	});

	public void TestStatisticalCode() => CombineAssertions(() =>
	{
		GoodsItemMock.Setup(x => x.StatisticalCode).Returns("2");
		AssertEquals("Has value", "002", GoodsItemWrapperDE.StatisticalCode);

		GoodsItemMock.Setup(x => x.StatisticalCode).Returns<string>(null);
		AssertEquals("No value", ZString.Empty, GoodsItemWrapperDE.StatisticalCode);
	});

	public void TestCustomsClearanceType() => CombineAssertions(() =>
	{
		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns("2");
		AssertEquals("Has value", "02", GoodsItemWrapperDE.CustomsClearanceType);

		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns<string>(null);
		AssertEquals("No value", ZString.Empty, GoodsItemWrapperDE.CustomsClearanceType);

		GoodsItemMock.Setup(x => x.Statistic).Returns<IEvvGoodsItemStatistic>(null);
		AssertEquals("No statistics", ZString.Empty, GoodsItemWrapperDE.CustomsClearanceType);
	});

	public void TestCustomsClearanceTypeDescription() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateProcedureCodeList(Factory, languages: TestLanguages);

		AssertDescription(SwissCustomsLanguageList.Codes.German, RefCusCodeTestHelper.ValidImportProcedureCode, nameof(RefCusCodeTestHelper.ValidImportProcedureCode) + "DE");
		AssertDescription(SwissCustomsLanguageList.Codes.German, RefCusCodeTestHelper.ValidExportProcedureCode, ZString.Empty);
		AssertDescription(SwissCustomsLanguageList.Codes.Italian, RefCusCodeTestHelper.ValidImportProcedureCode, nameof(RefCusCodeTestHelper.ValidImportProcedureCode) + "IT");
		AssertDescription(SwissCustomsLanguageList.Codes.Italian, RefCusCodeTestHelper.ValidExportProcedureCode, ZString.Empty);

		GoodsItemMock.Setup(x => x.Statistic).Returns<IEvvGoodsItemStatistic>(null);
		AssertEquals("No statistics", ZString.Empty, GoodsItemWrapperDE.CustomsClearanceTypeDescription);

		void AssertDescription(string language, string procedureCode, ZString expectedDescription)
		{
			GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns(procedureCode);
			var goodsItemDocumentWrapper = CreateGoodsItemWrapper(Factory.GetDocumentLanguage(language));
			AssertEquals($"ProcedureCode={procedureCode} Language={language}", expectedDescription, goodsItemDocumentWrapper.CustomsClearanceTypeDescription);
		}
	});

	public void TestIsRepairOrRefinement() => CombineAssertions(() =>
	{
		GoodsItemStatisticMock.Setup(x => x.IsRepair).Returns(false);

		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns(ProcedureCodesEdec.NormalDuty);
		AssertEquals("NormalDuty", ZBool.False, GoodsItemWrapperDE.IsRepairOrRefinement);

		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns(ProcedureCodesEdec.RefinementTransportation);
		AssertEquals("RefinementTransportation", ZBool.True, GoodsItemWrapperDE.IsRepairOrRefinement);

		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns(ProcedureCodesEdec.RepairTransportation);
		AssertEquals("RepairTransportation", ZBool.True, GoodsItemWrapperDE.IsRepairOrRefinement);

		GoodsItemStatisticMock.Setup(x => x.IsRepair).Returns(true);
		GoodsItemStatisticMock.Setup(x => x.CustomsClearanceType).Returns(ProcedureCodesEdec.NormalDuty);
		AssertEquals("IsRepair is ticked", ZBool.True, GoodsItemWrapperDE.IsRepairOrRefinement);

		GoodsItemMock.Setup(x => x.Statistic).Returns<IEvvGoodsItemStatistic>(null);
		AssertEquals("No statistics", ZBool.False, GoodsItemWrapperDE.IsRepairOrRefinement);
	});

	public void TestTotalDutyAndTaxesAmount() => CombineAssertions(() =>
	{
		GoodsItemMock.Setup(x => x.TotalDutyAndTaxesAmount).Returns(123.45m);
		AssertEquals("Value provided", 123.45m, GoodsItemWrapperDE.TotalDutyAndTaxesAmount);

		GoodsItemMock.Setup(x => x.TotalDutyAndTaxesAmount).Returns<decimal?>(null);
		AssertEquals("No value provided", 0m, GoodsItemWrapperDE.TotalDutyAndTaxesAmount);
	});

	public void TestIsCustomsNetWeight() => CombineAssertions(() =>
	{
		AssertEquals("Value not provided", false, GoodsItemWrapperDE.IsCustomsNetWeight);
		GoodsItemMock.Setup(x => x.CustomsNetWeight).Returns(1);
		AssertEquals("Value provided", true, GoodsItemWrapperDE.IsCustomsNetWeight);
	});

	public void TestIsRefinementType() => CombineAssertions(() =>
	{
		AssertEquals("Value not provided", false, GoodsItemWrapperDE.IsRefinementType);
		GoodsItemMock.Setup(x => x.RefinementType).Returns("1");
		AssertEquals("Value provided", true, GoodsItemWrapperDE.IsRefinementType);
	});

	public void TestIsProcessType() => CombineAssertions(() =>
	{
		AssertEquals("Value not provided", false, GoodsItemWrapperDE.IsProcessType);
		GoodsItemMock.Setup(x => x.ProcessType).Returns("1");
		AssertEquals("Value provided", true, GoodsItemWrapperDE.IsProcessType);
	});

	public void TestIsBillingType() => CombineAssertions(() =>
	{
		AssertEquals("Value not provided", false, GoodsItemWrapperDE.IsBillingType);
		GoodsItemMock.Setup(x => x.BillingType).Returns("1");
		AssertEquals("Value provided", true, GoodsItemWrapperDE.IsBillingType);
	});

	public void TestPermitObligation() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.PermitObligation).Returns(RefCusCodeTestHelper.ValidPermitObligationCode);
		AssertEquals("DE", nameof(RefCusCodeTestHelper.ValidPermitObligationCode) + "DE", GoodsItemWrapperDE.PermitObligation);
		AssertEquals("IT", nameof(RefCusCodeTestHelper.ValidPermitObligationCode) + "IT", GoodsItemWrapperIT.PermitObligation);
	});

	public void TestNonCustomsLawObligation() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNonCustomsLawObligationCodeList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.NonCustomsLawObligation).Returns(NonCustomsLawObligationCodes.Needed);
		AssertEquals("DE", nameof(NonCustomsLawObligationCodes.Needed) + "DE", GoodsItemWrapperDE.NonCustomsLawObligation);
		AssertEquals("IT", nameof(NonCustomsLawObligationCodes.Needed) + "IT", GoodsItemWrapperIT.NonCustomsLawObligation);
	});

	public void TestDirection() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardDirectionList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.Direction).Returns(RefCusCodeTestHelper.ValidSimpleCode);
		AssertEquals("Direction", "0 - DescriptionDE", GoodsItemWrapperDE.Direction);
		AssertEquals("Direction", "0 - DescriptionIT", GoodsItemWrapperIT.Direction);
	});

	public void TestRefinementType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardRefinementTypeList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.RefinementType).Returns(RefCusCodeTestHelper.ValidSimpleCode);
		AssertEquals("RefinementType", "0 - DescriptionDE", GoodsItemWrapperDE.RefinementType);
		AssertEquals("RefinementType", "0 - DescriptionIT", GoodsItemWrapperIT.RefinementType);
	});

	public void TestProcessType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardProcessTypeList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.ProcessType).Returns(RefCusCodeTestHelper.ValidSimpleCode);
		AssertEquals("ProcessType", "0 - DescriptionDE", GoodsItemWrapperDE.ProcessType);
		AssertEquals("ProcessType", "0 - DescriptionIT", GoodsItemWrapperIT.ProcessType);
	});

	public void TestBillingType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardBillingTypeList(Factory, languages: TestLanguages);

		GoodsItemMock.Setup(x => x.BillingType).Returns(RefCusCodeTestHelper.ValidSimpleCode);
		AssertEquals("BillingType", "0 - DescriptionDE", GoodsItemWrapperDE.BillingType);
		AssertEquals("BillingType", "0 - DescriptionIT", GoodsItemWrapperIT.BillingType);
	});

	Mock<IEvvGoodsItem> GoodsItemMock => goodsItemMock ??= new Mock<IEvvGoodsItem>();
	Mock<IEvvGoodsItem> goodsItemMock;

	Mock<IEvvGoodsItemStatistic> GoodsItemStatisticMock => goodsItemStatisticMock ??= CreateGoodsItemStatisticMock();
	Mock<IEvvGoodsItemStatistic> goodsItemStatisticMock;

	Mock<IEvvGoodsItemStatistic> CreateGoodsItemStatisticMock()
	{
		var statisticMock = new Mock<IEvvGoodsItemStatistic>();
		GoodsItemMock.Setup(x => x.Statistic).Returns(statisticMock.Object);
		return statisticMock;
	}

	EVVGoodsItemWrapper GoodsItemWrapperDE => goodsItemWrapperDE ??= CreateGoodsItemWrapper(SwissCustomsLanguageList.Codes.German);
	EVVGoodsItemWrapper goodsItemWrapperDE;

	EVVGoodsItemWrapper GoodsItemWrapperIT => goodsItemWrapperIT ??= CreateGoodsItemWrapper(SwissCustomsLanguageList.Codes.Italian);
	EVVGoodsItemWrapper goodsItemWrapperIT;

	EVVGoodsItemWrapper CreateGoodsItemWrapper(string documentLanguage)
	{
		return EVVGoodsItemWrapper.New(GoodsItemMock.Object, Factory, Factory.GetDocumentLanguage(documentLanguage));
	}

	string[] TestLanguages => [
			Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.German),
			Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.Italian),
		];
}
