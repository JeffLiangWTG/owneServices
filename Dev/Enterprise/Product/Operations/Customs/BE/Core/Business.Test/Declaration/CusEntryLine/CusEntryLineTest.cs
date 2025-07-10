using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

	public override void TestDutyRateDescription()
	{
		var cusEntryLine = Factory.New<CusEntryLine>();
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_CL = cusEntryLine.PK;
		AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
		AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
		AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
		var b00Fee = cusEntryLine.Fees.AddNew();
		b00Fee.CF_Rate = 17.5;
		b00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		var a30Fee = cusEntryLine.Fees.AddNew();
		a30Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
		a30Fee.CF_ChargeAmount = 202m;
		a30Fee.CF_BaseValue = 404m;
		a30Fee.CF_Rate = 50;
		var a00Fee = cusEntryLine.Fees.AddNew();
		a00Fee.CF_ChargeAmount = 33m;
		a00Fee.CF_BaseValue = 100m;
		a00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		a00Fee.CF_Rate = 33;
		AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
		AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
		AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);
	}

	[TestDate(2005, 6, 2)]
	public override void TestMoneyInLocalCurrency()
	{
		var newCurrency = RefCurrency.New(Factory);
		newCurrency.RX_Code = "MDD";
		var from = new ZDateTime(2005, 6, 1);
		var to = new ZDateTime(2005, 6, 5);
		newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

		var declaration = ImportJobDeclaration;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

		var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
		line1.JI_Tariff = "2203.10.10 10";
		line2.JI_Tariff = "2203.10.10 10";
		line1.JI_LinePrice = 100.0m;
		line2.JI_LinePrice = 200.0m;

		var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line1ONS.J7_Amount = 5.0m;
		line1ONS.J7_IsDutiable = true;
		var line1OFT = line1.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		line1OFT.J7_Amount = 10.0m;
		line1OFT.J7_IsDutiable = true;
		var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line2ONS.J7_Amount = 10.0m;
		line2ONS.J7_IsDutiable = true;
		var line2OFT = line2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		line2OFT.J7_IsDutiable = true;
		line2OFT.J7_Amount = 20.0m;

		CombineAssertions(() =>
		{
			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_CIF.Amount);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_CIF.Amount);
			AssertEquals("PreReq, line 1 CIF currency is invoice currency", newCurrency.Code, line1.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 2 CIF currency is invoice currency", newCurrency.Code, line2.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_Calc_CIF);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_Calc_CIF);
			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong.
			AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
		});
	}

	public new void TestTypeOfFees()
	{
		AssertEquals("Fees' type should be expected.", ExpectedTypeOfFees, Factory.New<CusEntryLine>().Fees.GetType());
	}
}
