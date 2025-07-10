using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Moq.Protected;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails.Testing;

sealed class LiquidationDetailsLineWrapperTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return LiquidationDetailsLineWrapper.New(entryLine, Factory);
	}

	public void TestConfirmedValues()
	{
		var entryLineMock = Factory.NewMoq<CusEntryLine>();
		entryLineMock.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(true);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;

		var entryLine = entryLineMock.Object;
		entryLine.CL_CH = entryHeader.PK;
		entryLine.CL_ConfirmedCustomsValue = 1m;
		entryLine.CL_CustomsValue = 2m;
		entryLine.CL_ConfirmedStatisticalValue = 3m;
		entryLine.CL_StatisticalValue = 4m;
		entryLine.CL_ConfirmedValueForVAT = 5m;
		entryLine.CL_ValueForVAT = 6m;
		var confirmedFee = entryLine.ConfirmedFees.AddNew();
		confirmedFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		confirmedFee.CF_ChargeAmount = 7m;

		var wrapper = LiquidationDetailsLineWrapper.New(entryLine, Factory);
		AssertEquals(1m, wrapper.CustomsValue);
		AssertEquals(3m, wrapper.StatisticalValue);
		AssertEquals(5m, wrapper.VatValue);
		AssertEquals(12m, wrapper.BaseVatableValue);

		var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
		var entryLine2 = entryLineMock2.Object;
		entryLine2.CL_CH = entryHeader.PK;
		entryLine2.CL_ConfirmedCustomsValue = 1m;
		entryLine2.CL_CustomsValue = 2m;
		entryLine2.CL_ConfirmedStatisticalValue = 3m;
		entryLine2.CL_StatisticalValue = 4m;
		entryLine2.CL_ConfirmedValueForVAT = 5m;
		entryLine2.CL_ValueForVAT = 6m;
		var confirmedFee2 = entryLine2.ConfirmedFees.AddNew();
		confirmedFee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		confirmedFee2.CF_ChargeAmount = 7m;

		entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES040;
		wrapper = LiquidationDetailsLineWrapper.New(entryLine2, Factory);
		AssertEquals(2m, wrapper.CustomsValue);
		AssertEquals(4m, wrapper.StatisticalValue);
		AssertEquals(6m, wrapper.VatValue);
		AssertEquals(13m, wrapper.BaseVatableValue);
	}

	public void TestAllLiquidationDetailsLineWrapperProperties()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping("FR");
		var rateType = helper.CreateCusRateType("FR", Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
		helper.CreateCusRateCode(Factory, "DTY", rateType.PK);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_SystemCreateTimeUtc = new ZDateTime(2021, 8, 6, 0, 0, 0);
		var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader1.Charges.AddNew();
		var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var charge = entryHeader.ConfirmedCharges.AddOrUpdate("C11");
		charge.C1_ChargeAmount = 10m;
		charge.C1_MethodOfPayment = "5";

		var entryLine1 = Factory.New<CusEntryLineForTest>();
		entryHeader.MergedLines.Add(entryLine1);
		invoiceLine1.JI_CL = entryLine1.PK;

		var wrapper = LiquidationDetailsLineWrapper.New(entryLine1, Factory);

		CombineAssertions(() =>
		{
			entryLine1.CL_LineNumber = 1;
			AssertEquals("1", wrapper.LineNumber);

			entryLine1.CL_AdValoremTariff = "1101.01.01";
			AssertEquals("1101.01.01", wrapper.Commodity);

			entryLine1.EffectiveDescription = "description1";
			AssertEquals("description1", wrapper.Description);

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "AAAAAAAAAAAA";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 1;
			AssertEquals("1 AA          ", wrapper.PackageSummary);

			invoiceLine1.JI_CountryOfOrigin = "FR";
			AssertEquals("FR      ", wrapper.Origin);

			invoiceLine1.JI_PrimaryPreference = "123";
			AssertEquals("123         ", wrapper.Preference);

			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.Charges.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			var entryLine2 = Factory.New<CusEntryLineForTest>();
			entryHeader.MergedLines.Add(entryLine2);
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
			invoiceLine1.JI_LinePrice = 10m;
			invoiceLine2.JI_LinePrice = 20m;
			entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
			entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
			AssertEquals("7.19            ", wrapper.ItemPrice);
			AssertEquals("EUR       ", wrapper.ItemPriceCurrency);

			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			entryLine1.CL_InvoiceAmount = entryLine1.GetInvoicedDocumentaryAmountCore();
			entryLine1.CL_RX_NKInvoiceAmountCurrency = entryLine1.GetInvoicedDocumentaryAmountCurrencyCore();
			AssertEquals("10              ", wrapper.ItemPrice);
			AssertEquals("USD       ", wrapper.ItemPriceCurrency);

			invoiceLine1.JI_CustomsSecondUnitQty = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 10m;
			AssertEquals("KG          ", wrapper.SupplementaryUnitQty);
			AssertEquals("10             ", wrapper.SupplementaryQty);

			invoiceLine1.JI_SupplementaryCode1 = "B002";
			invoiceLine1.JI_SupplementaryCode2 = "B001";
			var cana1 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";
			var cana2 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";
			AssertEquals("V910, V911", string.Join(", ", wrapper.FRAdditionalCodes));
			AssertEquals("B001, B002", string.Join(", ", wrapper.CEAdditionalCodes));
			AssertEquals("V910, V911 - B001, B002 ", wrapper.AdditionalCodesAsString);
			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "1001";
			supportingDocument1.CSI_IsDTP = ZBool.True;
			var supportingDocument2 = invoiceHeader1.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "2001";
			supportingDocument2.CSI_IsDTP = ZBool.True;
			var supportingDocument3 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "3001";
			supportingDocument3.CSI_IsDTP = ZBool.True;
			var supportingDocument4 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "4001";
			supportingDocument4.CSI_IsDTP = ZBool.False;
			AssertEquals("1001, 2001, 3001  ", wrapper.SpecialProvision);

			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";
			AssertEquals("10              ", wrapper.GrossMassInKg);

			invoiceLine1.JI_NetWeight = 20m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			AssertEquals("20              ", wrapper.NettMassInKg);

			invoiceLine1.JI_TariffBypassCode = "1";
			AssertEquals("1", wrapper.TariffBypass);

			entryLine1.CL_CustomsValue = 30m;
			AssertEquals(30m, wrapper.CustomsValue);

			entryLine1.CL_StatisticalValue = 40m;
			AssertEquals(40m, wrapper.StatisticalValue);

			entryLine1.CL_ValueForVAT = 50m;
			AssertEquals(50m, wrapper.VatValue);

			entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1.01m);
			AssertEquals(51m, wrapper.BaseVatableValue);

			var fee1 = entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1.1m);
			fee1.G4_BaseAmount = 11.11m;
			fee1.G4_RateDuty = "%";
			fee1.G4_RateSuspension = "1";
			fee1.NationalFeeTypeCode = "A325";

			var fee2 = entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 2.2m);
			fee2.G4_BaseAmount = 22.22m;
			fee2.G4_RateDuty = "%";
			fee2.G4_RateSuspension = "20";
			fee2.CF_MethodOfPayment = "2";
			fee2.NationalFeeTypeCode = "A325";

			var fee3 = entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 3.3m);
			fee3.G4_BaseAmount = 33.33m;
			fee3.G4_RateDuty = "%";
			fee3.G4_RateSuspension = "30";
			fee3.CF_MethodOfPayment = "3";
			fee3.NationalFeeTypeCode = "A336";

			var fee4 = entryLine1.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.TotalAmountPayable, 4.4m);
			fee4.G4_BaseAmount = 44.44m;
			fee4.G4_RateDuty = "002";
			fee4.G4_RateSuspension = "40";
			fee4.CF_MethodOfPayment = "4";
			fee4.NationalFeeTypeCode = "A365";

			AssertEquals("Has entry ConfirmedCharges", @"
Tax code  Base Amount     Tax rate      Total amount    SL
C11                                     10              5
A325      33                            3
A336      33              30.00%        3               3
A365      44                            4               4
", wrapper.ItemTaxBreakdown);

			AssertEquals("", wrapper.ItemChargesBreakdown);

			var fee5 = entryLine2.ConfirmedFees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.TotalAmountPayable, 4.4m);
			fee5.G4_BaseAmount = 44.44m;
			fee5.G4_RateDuty = "002";
			fee5.G4_RateSuspension = "40";
			fee5.CF_MethodOfPayment = "4";
			fee5.NationalFeeTypeCode = "A365";
			var wrapper2 = LiquidationDetailsLineWrapper.New(entryLine2, Factory);
			AssertEquals("No entry ConfirmedCharges", @"
Tax code  Base Amount     Tax rate      Total amount    SL
A365      44                            4               4
", wrapper2.ItemTaxBreakdown);

			var charge1 = invoiceLine1.Charges.AddNew("ADD", 1);
			charge1.J7_Amount = 1m;
			charge1.J7_RX_NKCurrency = "EUR";
			charge1.J7_IsDutiable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsStatisticalValueApplicable = true;
			charge1.J7_IsIncludedInITOT = true;

			var charge2 = invoiceLine1.Charges.AddNew("ADV", 1);
			charge2.J7_Amount = 2m;
			charge2.J7_RX_NKCurrency = "EUR";
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = false;
			charge2.J7_IsStatisticalValueApplicable = false;
			charge2.J7_IsIncludedInITOT = false;

			wrapper = LiquidationDetailsLineWrapper.New(entryLine1, Factory);
			AssertEquals(@"
Code  Amount            Currency  Dutiable  TVA Apply  Statable  Incl. In Line
ADD   1                 EUR       Y         Y          Y         Y
ADV   2                 EUR       N         N          N         N
", wrapper.ItemChargesBreakdown);
		});
	}

	public void TestTotalWeightInKg_NoException()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_Weight = 10m;
		invoiceLine.JI_WeightUQ = "";

		var wrapper = LiquidationDetailsLineWrapper.New(entryLine, Factory);
		AssertEquals("empty uint", false, entryLine.EffectiveGrossWeight.IsValid);
		AssertEquals("No Exception", "0               ", wrapper.GrossMassInKg);
	}

	public void TestNetWeightInKg_NoException()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_NetWeight = 20m;
		invoiceLine.JI_NetWeightUQ = "";

		var wrapper = LiquidationDetailsLineWrapper.New(entryLine, Factory);
		AssertEquals("empty uint", false, entryLine.EffectiveNetWeight.IsValid);
		AssertEquals("No Exception", "0               ", wrapper.NettMassInKg);
	}

	public class CusEntryLineForTest : CusEntryLine
	{
		public CusEntryLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ZDecimal GetInvoicedDocumentaryAmountCore() => base.GetInvoicedDocumentaryAmountCore();

		public new ZString GetInvoicedDocumentaryAmountCurrencyCore() => base.GetInvoicedDocumentaryAmountCurrencyCore();
	}
}
