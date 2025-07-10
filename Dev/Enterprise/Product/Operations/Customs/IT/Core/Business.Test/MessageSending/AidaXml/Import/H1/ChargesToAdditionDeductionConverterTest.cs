using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.Business.CusEntryLine;
using JobComInvoiceLine = Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.IT.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class ChargesToAdditionDeductionConverterTest : TestCaseWithFactory
{
	public void TestGetAdditionAndDeductions_WithOnlyChargesOfTypeAddition()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge,
			amount: 30,
			currency: CurrencyCodes.EuropeanUnion);
		chargeOne.J7_IsDutiable = ZBool.True;

		var chargeTwo = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge,
			amount: 20,
			currency: CurrencyCodes.EuropeanUnion);
		chargeTwo.J7_IsDutiable = ZBool.True;

		var chargeThree = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge,
			amount: 12,
			currency: CurrencyCodes.EuropeanUnion);
		chargeThree.J7_IsDutiable = ZBool.True;

		var chargeFour = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			amount: 12,
			currency: CurrencyCodes.EuropeanUnion);
		chargeFour.J7_IsDutiable = ZBool.True;

		var chargeNotToBeCounted = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge,
			amount: 243,
			currency: CurrencyCodes.EuropeanUnion);
		chargeNotToBeCounted.J7_IsDutiable = ZBool.False;
		chargeNotToBeCounted.J7_IsIncludedInITOT = ZBool.False;

		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);
		var chargeWrapperTwo = new InvoiceLineChargeWrapper(chargeTwo);
		var chargeWrapperThree = new InvoiceLineChargeWrapper(chargeThree);
		var chargeWrapperFour = new InvoiceLineApportionedChargeWrapper(chargeFour);
		var chargeWrapperNotToBeCounted = new InvoiceLineApportionedChargeWrapper(chargeNotToBeCounted);

		var allCharges = new IInvoiceLineChargeWrapper[] { chargeWrapperOne, chargeWrapperTwo, chargeWrapperThree, chargeWrapperFour, chargeWrapperNotToBeCounted };

		var converter = new ChargesToAdditionDeductionConverter();
		var additionDeductionCodes = converter.GetAdditionsAndDeductions(allCharges);

		AssertNotNull("Addition Codes", additionDeductionCodes);
		AssertEquals("Count", 3, additionDeductionCodes.Count);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.AB);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.AG);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.AK);
	}

	public void TestGetAdditionsAndDeductions_WithOnlyDeductionType()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: 30,
			currency: CurrencyCodes.EuropeanUnion);
		chargeOne.J7_IsDutiable = ZBool.False;
		chargeOne.J7_IsIncludedInITOT = ZBool.True;

		var chargeTwo = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge,
			amount: 20,
			currency: CurrencyCodes.EuropeanUnion);
		chargeTwo.J7_IsDutiable = ZBool.False;
		chargeTwo.J7_IsIncludedInITOT = ZBool.True;

		var chargeThree = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			amount: 12,
			currency: CurrencyCodes.EuropeanUnion);
		chargeThree.J7_IsDutiable = ZBool.False;
		chargeThree.J7_IsIncludedInITOT = ZBool.True;

		var chargeNotToBeCounted = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.AdjustmentCharge,
			amount: 223,
			currency: CurrencyCodes.EuropeanUnion);
		chargeNotToBeCounted.J7_IsDutiable = ZBool.False;
		chargeNotToBeCounted.J7_IsIncludedInITOT = ZBool.False;

		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);
		var chargeWrapperTwo = new InvoiceLineChargeWrapper(chargeTwo);
		var chargeWrapperThree = new InvoiceLineApportionedChargeWrapper(chargeThree);
		var chargeWrapperNotToBeCounted = new InvoiceLineApportionedChargeWrapper(chargeNotToBeCounted);

		var allCharges = new IInvoiceLineChargeWrapper[] { chargeWrapperOne, chargeWrapperTwo, chargeWrapperThree, chargeWrapperNotToBeCounted };
		var converter = new ChargesToAdditionDeductionConverter();
		var additionDeductionCodes = converter.GetAdditionsAndDeductions(allCharges);

		AssertNotNull("Deduction Codes", additionDeductionCodes);
		AssertEquals("Count", 2, additionDeductionCodes.Count);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.BA);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.BB);
	}

	public void TestGetAdditionsAndDeductions()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge,
			amount: 30,
			currency: CurrencyCodes.EuropeanUnion);
		chargeOne.J7_IsDutiable = ZBool.True;

		var chargeTwo = CreateChargeAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge,
			amount: 20,
			currency: CurrencyCodes.EuropeanUnion);
		chargeTwo.J7_IsDutiable = ZBool.False;
		chargeTwo.J7_IsIncludedInITOT = ZBool.True;

		var chargeNotToBeCounted = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.AdjustmentCharge,
			amount: 223,
			currency: CurrencyCodes.EuropeanUnion);
		chargeNotToBeCounted.J7_IsDutiable = ZBool.False;
		chargeNotToBeCounted.J7_IsIncludedInITOT = ZBool.False;

		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);
		var chargeWrapperTwo = new InvoiceLineChargeWrapper(chargeTwo);
		var chargeWrapperNotToBeCounted = new InvoiceLineApportionedChargeWrapper(chargeNotToBeCounted);

		var allCharges = new IInvoiceLineChargeWrapper[] { chargeWrapperOne, chargeWrapperTwo, chargeWrapperNotToBeCounted };
		var converter = new ChargesToAdditionDeductionConverter();
		var additionDeductionCodes = converter.GetAdditionsAndDeductions(allCharges);

		AssertNotNull("Addition and Deduction Codes", additionDeductionCodes);
		AssertEquals("Count", 2, additionDeductionCodes.Count);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.AB);
		AssertCollectionContains(additionDeductionCodes, code => code.Code == AdditionDeductionCodeList.Codes.BB);
	}

	public void TestGetAdditionsAndDeductions_DISCode()
	{
		var chargeCodeDIS = CreateApportionedChargAndAddToInvoiceLine(
			chargeType: UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge,
			amount: 150m,
			currency: CurrencyCodes.EuropeanUnion);
		chargeCodeDIS.J7_IsDutiable = ZBool.False;
		chargeCodeDIS.J7_IsIncludedInITOT = ZBool.False;

		var chargeWrapper = new InvoiceLineApportionedChargeWrapper(chargeCodeDIS);
		var charges = new[] { chargeWrapper };
		var codes = new ChargesToAdditionDeductionConverter().GetAdditionsAndDeductions(charges);

		AssertNotNull("Codes", codes);
		AssertEquals("Count", 1, codes.Count);
		AssertCollectionContains(message: "DIS Charge as Deductible",
			items: codes,
			predicate: c => c.Code == AdditionDeductionCodeList.Codes.BB && c.CalculateAmount() == 150m);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;

	EU.Business.Declaration.InvoiceLineCharge CreateChargeAndAddToInvoiceLine(ZString chargeType, ZDecimal amount, string currency)
	{
		var chargeOne = invoiceLine.Charges.AddNew();
		chargeOne.J7_ChargeType = chargeType;
		chargeOne.J7_Amount = amount;
		chargeOne.J7_RX_NKCurrency = currency;
		return chargeOne;
	}

	BaseInvoiceLineApportionedCharge CreateApportionedChargAndAddToInvoiceLine(string chargeType, ZDecimal amount, string currency)
	{
		var chargeOne = invoiceLine.ApportionedCharges.AddNew();
		chargeOne.J7_ChargeType = chargeType;
		chargeOne.J7_Amount = amount;
		chargeOne.J7_RX_NKCurrency = currency;
		return chargeOne;
	}
}
