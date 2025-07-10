using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;
using JobComInvoiceLine = Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.IT.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class AdditionDeductionCodeBucketTest : TestCaseWithFactory
{
	public void TestCalculateAmountSumsUccChargeCodeAmounts()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: 50,
			currency: CurrencyCodes.EuropeanUnion);
		var chargeTwo = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			amount: 20,
			currency: CurrencyCodes.EuropeanUnion);

		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);
		var chargeWrapperTwo = new InvoiceLineChargeWrapper(chargeTwo);

		var bucket = new AdditionDeductionCodeBucket("AB");
		bucket.AddCharge(chargeWrapperOne);
		bucket.AddCharge(chargeWrapperTwo);

		var amount = bucket.CalculateAmount();

		AssertEquals("Code", bucket.Code, "AB");
		AssertEquals("Amount", 70m, amount);
	}

	public void TestCalculateAmountInMultipleCurrencies()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: 48.85,
			currency: CurrencyCodes.UnitedStates);
		var chargeTwo = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			amount: 20.6689,
			currency: CurrencyCodes.EuropeanUnion);

		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);
		var chargeWrapperTwo = new InvoiceLineChargeWrapper(chargeTwo);

		var bucket = new AdditionDeductionCodeBucket("AB");
		bucket.AddCharge(chargeWrapperOne);
		bucket.AddCharge(chargeWrapperTwo);

		var amount = bucket.CalculateAmount();
		AssertEquals("Code", bucket.Code, "AB");
		AssertEquals("Amount", 70.91m, amount);
	}

	public void TestCalculateAmount_WithSingleEntryInEUR()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: 48.8501,
			currency: CurrencyCodes.EuropeanUnion);
		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);

		var bucket = new AdditionDeductionCodeBucket("AB");
		bucket.AddCharge(chargeWrapperOne);

		var amount = bucket.CalculateAmount();
		AssertEquals("Code", bucket.Code, "AB");
		AssertEquals("Amount", 48.85m, amount);
	}

	public void TestCalculateAmount_WithSingleEntryInUSD()
	{
		var chargeOne = CreateChargeAndAddToInvoiceLine(chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: 48.85,
			currency: CurrencyCodes.UnitedStates);
		var chargeWrapperOne = new InvoiceLineChargeWrapper(chargeOne);

		var bucket = new AdditionDeductionCodeBucket("AB");
		bucket.AddCharge(chargeWrapperOne);

		var amount = bucket.CalculateAmount();
		AssertEquals("Code", bucket.Code, "AB");
		AssertEquals("Amount", 50.24m, amount);
	}

	public void TestCalculateAmount_ReturnDecimalWithPrecisionTwo()
	{
		var chargeOne = invoiceLine
			.ApportionedCharges
			.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 0, CurrencyCodes.EuropeanUnion);
		chargeOne.J7_Amount = 40.0000m;
		var chargeWrapperOne = new InvoiceLineApportionedChargeWrapper(chargeOne);

		var bucket = new AdditionDeductionCodeBucket("AK");
		bucket.AddCharge(chargeWrapperOne);

		var amount = bucket.CalculateAmount();
		AssertEquals("Rounded Amount", 40.00m, amount);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var usd = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates);
		usd.ExchangeRates.DeleteAll();

		var thisMonthUsdRate = usd.ExchangeRates.AddNew();
		thisMonthUsdRate.RE_ExRateType = "CUS";
		thisMonthUsdRate.RE_StartDate = ZDateTime.MinSmallDateTimeValue;
		thisMonthUsdRate.RE_ExpiryDate = ZDateTime.MaxSmallDateTimeValue;
		thisMonthUsdRate.RE_SellRate = 0.9723;

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

	InvoiceLineCharge CreateChargeAndAddToInvoiceLine(ZString chargeType, ZDecimal amount, string currency)
	{
		var chargeOne = invoiceLine.Charges.AddNew();
		chargeOne.J7_ChargeType = chargeType;
		chargeOne.J7_Amount = amount;
		chargeOne.J7_RX_NKCurrency = currency;
		return chargeOne;
	}
}

sealed class AdditionDeductionCodeBucketRoundingIssueTest : TestCaseWithFactory
{
	public void TestCalculateAmount_WithUSDRoundingIsCorrect()
	{
		var lineOneCharges = CreateChargesForInvoiceLine(invoiceLineOne, 100, 50, 100, 100);
		var lineTwoCharges = CreateChargesForInvoiceLine(invoiceLineTwo, 80, 40, 80, 80);
		var lineThreeCharges = CreateChargesForInvoiceLine(invoiceLineThree, 20, 10, 20, 20);

		var allCharges = lineOneCharges.Union(lineTwoCharges).Union(lineThreeCharges).ToArray();
		var bucket = new AdditionDeductionCodeBucket("BA");
		allCharges.ForEach(bucket.AddCharge);

		var totalAmount = bucket.CalculateAmount();
		AssertEquals("Amount", 704.66m, totalAmount);
	}

	IInvoiceLineChargeWrapper[] CreateChargesForInvoiceLine(JobComInvoiceLine line, decimal oftCodeAmount, decimal aftCodeAmount, decimal onsCodeAmount, decimal advCodeAmount)
	{
		var oftCharge = CreateChargeAndAddToInvoiceLine(line, chargeType: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			amount: oftCodeAmount,
			currency: CurrencyCodes.UnitedStates);

		var aftCharge = CreateChargeAndAddToInvoiceLine(line, chargeType: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			amount: aftCodeAmount,
			currency: CurrencyCodes.UnitedStates);

		var onsCharge = CreateChargeAndAddToInvoiceLine(line, chargeType: UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge,
			amount: onsCodeAmount,
			currency: CurrencyCodes.UnitedStates);

		var advCharge = CreateChargeAndAddToInvoiceLine(line, chargeType: UCCCustomsChargeTypeList.Codes.AdjustmentCharge,
			amount: advCodeAmount,
			currency: CurrencyCodes.UnitedStates);
		return new IInvoiceLineChargeWrapper[]
		{
			new InvoiceLineChargeWrapper(oftCharge),
			new InvoiceLineChargeWrapper(aftCharge),
			new InvoiceLineChargeWrapper(onsCharge),
			new InvoiceLineChargeWrapper(advCharge),
		};
	}

	InvoiceLineCharge CreateChargeAndAddToInvoiceLine(JobComInvoiceLine line, ZString chargeType, ZDecimal amount, string currency)
	{
		var chargeOne = line.Charges.AddNew();
		chargeOne.J7_ChargeType = chargeType;
		chargeOne.J7_Amount = amount;
		chargeOne.J7_RX_NKCurrency = currency;
		return chargeOne;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var usd = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates);
		usd.ExchangeRates.DeleteAll();

		var usdExchangeRate = usd.ExchangeRates.AddNew();
		usdExchangeRate.RE_ExRateType = "CUS";
		usdExchangeRate.RE_StartDate = ZDateTime.MinSmallDateTimeValue;
		usdExchangeRate.RE_ExpiryDate = ZDateTime.MaxSmallDateTimeValue;
		usdExchangeRate.RE_SellRate = 0.9934;

		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLineOne = invoice.InvoiceLines.AddNew();
		invoiceLineTwo = invoice.InvoiceLines.AddNew();
		invoiceLineThree = invoice.InvoiceLines.AddNew();

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLineOne.JI_CL = entryLine.PK;
		invoiceLineTwo.JI_CL = entryLine.PK;
		invoiceLineThree.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLineOne, invoiceLineTwo, invoiceLineThree;
}
