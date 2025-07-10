using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

public abstract class BaseGuidedDecisionMakingSourceAbstractTest<T> : TestCaseWithFactory
	where T : BaseGuidedDecisionMakingSource
{
	public virtual void TestVATCode()
	{
		invoiceLine.JI_ZZF_NKTaxType = "RED";
		var wrapper = GetGuidedDecisionMakingSource(invoiceLine);
		AssertEquals("VATCode should be captured from invoice line to the GDMWSourceWrapper.", "RED", wrapper.VATCode);
	}

	public virtual void TestCountryCode()
	{
		JobComInvoiceLine invoiceLine;

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var wrapper = GetGuidedDecisionMakingSource(invoiceLine);
		AssertEquals(Core.Constants.CountryCodes.Latvia, wrapper.CountryCode);

		invoiceLine.JI_JZ = ZGuid.Empty;
		AssertEquals(Core.Constants.CountryCodes.Latvia, wrapper.CountryCode);
	}

	public virtual void TestSupplementaryCode()
	{
		invoiceLine.JI_SupplementaryCode1 = "AAAA";
		AssertEquals("AAAA", wrapper.SupplementaryCodes.First());
	}

	public virtual void TestSupplementaryCodes()
	{
		invoiceLine.JI_SupplementaryCode1 = "AAAA";
		invoiceLine.JI_SupplementaryCode2 = "BBBB";
		AssertSequencesEqual(new ZString[] { "AAAA", "BBBB" }, wrapper.SupplementaryCodes);
	}

	public virtual void TestDataGrouping()
	{
		AssertEquals(Core.Constants.CountryCodes.Latvia, wrapper.DataGrouping);
	}

	public virtual void TestParentDataGrouping()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
		Factory.Save();
		AssertEquals("EUN", wrapper.ParentDataGrouping);
	}

	public virtual void TestUserLanguage()
	{
		AssertEquals(Core.Constants.Languages.English, wrapper.UserLanguage);
	}

	public virtual void TestDutyRateTypeCode()
	{
		AssertEquals(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, wrapper.DutyRateTypeCode);
	}

	public virtual void TestAllowQuickAdditionalCodeScreen()
	{
		Assert(!wrapper.AllowQuickAdditionalCodeScreen);
	}

	public virtual void TestAllowQuickConditionScreen()
	{
		Assert(!wrapper.AllowQuickConditionScreen);
	}

	[TestDate(2022, 5, 25, 11, 0, 0)]
	public virtual void TestEffectiveDate()
	{
		AssertEquals(new ZDate(2022, 5, 25), wrapper.EffectiveDate);
		invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 5, 24, 11, 0, 0);
		AssertEquals(new ZDate(2022, 5, 24), wrapper.EffectiveDate);
	}

	public virtual void TestTariffCode()
	{
		invoiceLine.JI_Tariff = "11001100";
		AssertEquals("11001100", wrapper.TariffCode);
	}

	public virtual void TestCountryOfOrigin()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Madagascar;
		AssertEquals("", wrapper.CountryOfOrigin);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		wrapper = GetGuidedDecisionMakingSource(invoiceLine2);
		AssertEquals(Core.Constants.CountryCodes.Botswana, wrapper.CountryOfOrigin);
	}

	public virtual void TestPreference()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
		Factory.Save();
		var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
		var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
		Factory.Save();
		var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
		Factory.Save();

		var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
		Factory.Save();

		var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
		helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
		helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");
		Factory.Save();

		invoiceLine.JI_Tariff = "11001100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Botswana;
		AssertEquals("STD", wrapper.Preference);

		var preferenceRED = helper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
		var testRate2 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
		helper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
		helper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add22", "ord22");
		Factory.Save();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "11001100";
		invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine2.ZG_CountryOfDestination = Core.Constants.CountryCodes.Botswana;
		wrapper = GetGuidedDecisionMakingSource(invoiceLine2);
		AssertEquals("", wrapper.Preference);

		invoiceLine2.JI_PrimaryPreference = "MFN";
		AssertEquals("", wrapper.Preference);

		invoiceLine2.JI_PrimaryPreference = "RED";
		AssertEquals("RED", wrapper.Preference);
	}

	public virtual void TestQuotaOrderNumber()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
		Factory.Save();
		var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
		var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
		Factory.Save();
		var preferenceSTD = helper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
		Factory.Save();

		var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
		Factory.Save();

		var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
		helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
		Factory.Save();

		invoiceLine.JI_Tariff = "11001100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Botswana;
		invoiceLine.JI_PrimaryPreference = "STD";
		AssertEquals("ord11", wrapper.QuotaOrderNumber);

		helper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");
		Factory.Save();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "11001100";
		invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine2.ZG_CountryOfDestination = Core.Constants.CountryCodes.Botswana;
		invoiceLine2.JI_PrimaryPreference = "STD";

		wrapper = GetGuidedDecisionMakingSource(invoiceLine2);
		AssertEquals("", wrapper.QuotaOrderNumber);

		invoiceLine2.JI_ConcessionOrder = "ord13";
		AssertEquals("", wrapper.QuotaOrderNumber);

		invoiceLine2.JI_ConcessionOrder = "ord12";
		AssertEquals("ord12", wrapper.QuotaOrderNumber);
	}

	public virtual void TestCustomsFirstQuantity()
	{
		invoiceLine.JI_CustomsQuantity = 10m;
		invoiceLine.JI_CustomsUnitQty = "KG";
		AssertEquals(10m, wrapper.CustomsFirstQuantity);
	}

	public virtual void TestCustomsSecondQuantity()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
		Factory.Save();

		var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
		Factory.Save();

		helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU2, "T", dataGrouping);
		Factory.Save();

		invoiceLine.JI_Tariff = "11001100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine.JI_CustomsSecondQuantity = 10m;
		invoiceLine.JI_CustomsSecondUnitQty = "KG";
		AssertEquals(0m, wrapper.CustomsSecondQuantity);

		invoiceLine.JI_CustomsSecondUnitQty = "T";
		AssertEquals(10m, wrapper.CustomsSecondQuantity);
	}

	public virtual void TestCustomsSecondUnitQty()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
		Factory.Save();

		var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
		Factory.Save();

		helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU2, "T", dataGrouping);
		Factory.Save();

		invoiceLine.JI_Tariff = "11001100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine.JI_CustomsSecondQuantity = 10m;
		invoiceLine.JI_CustomsSecondUnitQty = "T";
		AssertEquals("CustomsSecondUnitQty should be captured from UOM.", "T", wrapper.CustomsSecondUnitQty);

		invoiceLine.JI_CustomsSecondUnitQty = "KG";
		AssertEquals("CustomsSecondUnitQty should be captured from UOM regardless the input of JI_CustomsSecondUnitQty.", "T", wrapper.CustomsSecondUnitQty);
	}

	public virtual void TestCustomsThirdQuantity()
	{
		invoiceLine.JI_CustomsThirdQuantity = 10m;
		invoiceLine.JI_CustomsThirdUnitQty = "KG";
		AssertEquals(10m, wrapper.CustomsThirdQuantity);
	}

	public virtual void TestCustomsThirdUnitQty()
	{
		invoiceLine.JI_CustomsThirdQuantity = 10m;
		invoiceLine.JI_CustomsThirdUnitQty = "HLT";
		AssertEquals("HLT", wrapper.CustomsThirdUnitQty);
	}

	public virtual void TestSupportingAndAdditonalDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var wrapper = GetGuidedDecisionMakingSource(invoiceLine);

		var supportingDocument1 = declaration.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Code = "1";
		supportingDocument1.CSI_ReferenceNumber = "11";
		supportingDocument1.CSI_DateOfIssue = new ZDateTime(2023, 06, 22);

		var supportingDocument2 = invoice.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Code = "2";
		supportingDocument2.CSI_ReferenceNumber = "22";
		supportingDocument2.CSI_DateOfIssue = new ZDateTime(2023, 06, 23);

		var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument3.CSI_Code = "3";
		supportingDocument3.CSI_ReferenceNumber = "33";
		supportingDocument3.CSI_DateOfIssue = new ZDateTime(2023, 06, 24);

		var additionalDocument1 = declaration.AdditionalInfos.AddNew();
		additionalDocument1.CSI_Code = "4";
		additionalDocument1.CSI_ReferenceNumber = "44";
		additionalDocument1.CSI_DateOfIssue = new ZDateTime(2023, 06, 25);

		var additionalDocument2 = invoice.AdditionalInfos.AddNew();
		additionalDocument2.CSI_Code = "5";
		additionalDocument2.CSI_ReferenceNumber = "55";
		additionalDocument2.CSI_DateOfIssue = new ZDateTime(2023, 06, 26);

		var additionalDocument3 = invoiceLine.AdditionalInfos.AddNew();
		additionalDocument3.CSI_Code = "6";
		additionalDocument3.CSI_ReferenceNumber = "66";
		additionalDocument3.CSI_DateOfIssue = new ZDateTime(2023, 06, 27);

		using (new DeclarationValidationDeciderTestContext(declaration, isUCC6: true))
		{
			AssertContainsExactElementsInAnyOrder(new (ZString Code, ZString Reference, ZDateTime DateOfIssue)[] { ("1", "11", new ZDateTime(2023, 06, 22)), ("2", "22", new ZDateTime(2023, 06, 23)), ("3", "33", new ZDateTime(2023, 06, 24)), ("4", "44", new ZDateTime(2023, 06, 25)), ("5", "55", new ZDateTime(2023, 06, 26)), ("6", "66", new ZDateTime(2023, 06, 27)) }, wrapper.SupportingAndAdditionalDocuments);
		}

		using (new DeclarationValidationDeciderTestContext(declaration, isUCC6: false))
		{
			AssertContainsExactElementsInAnyOrder(new (ZString Code, ZString Reference, ZDateTime DateOfIssue)[] { ("1", "11", new ZDateTime(2023, 06, 22)), ("2", "22", new ZDateTime(2023, 06, 23)), ("3", "33", new ZDateTime(2023, 06, 24)) }, wrapper.SupportingAndAdditionalDocuments);
		}
	}

	public virtual void TestIsImport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Import", true, wrapper.IsImport);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("Export", false, wrapper.IsImport);
	}

	public virtual void TestIsExport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Import", false, wrapper.IsExport);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("Export", true, wrapper.IsExport);
	}

	public virtual void TestTariffType()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			AssertEquals("Should be EXP when invoiceLine.UniversalTariffType is EXP.", "EXP", wrapper.TariffType);
			declaration.JE_MessageType = "IMP";
			AssertEquals("Should be IMP when invoiceLine.UniversalTariffType is IMP.", "IMP", wrapper.TariffType);
		});
	}

	public virtual void TestCountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var date1 = new ZDate(2010, 12, 10);
		var date4 = new ZDate(2079, 06, 06);
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
		var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
		helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
		Factory.Save();

		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Madagascar;
		AssertEquals("", wrapper.EffectiveCountryOfDestination);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.ZG_CountryOfDestination = Core.Constants.CountryCodes.Botswana;
		wrapper = GetGuidedDecisionMakingSource(invoiceLine2);
		AssertEquals(Core.Constants.CountryCodes.Botswana, wrapper.EffectiveCountryOfDestination);
	}

	public abstract void TestIsCustomsFirstQuantityEditable();

	public abstract void TestIsCustomsSecondQuantityEditable();

	public abstract void TestIsCustomsThirdQuantityEditable();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		wrapper = GetGuidedDecisionMakingSource(invoiceLine);
	}

	protected abstract T GetGuidedDecisionMakingSource(JobComInvoiceLine invoiceLine);

	protected JobDeclaration declaration;
	protected JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	protected T wrapper;
}
