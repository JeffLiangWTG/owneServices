using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class InvoiceLineChargeWrapperTest : TestCaseWithFactory
{
	public void TestInvoiceLineChargeWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var charge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 20);
		charge.J7_IsDutiable = ZBool.True;
		charge.J7_IsIncludedInITOT = ZBool.True;
		IInvoiceLineChargeWrapper wrapper = new InvoiceLineChargeWrapper(charge);

		AssertEquals("Charge Code", charge.J7_ChargeType, wrapper.ChargeCode);
		AssertEquals("Charge Amount", charge.J7_Amount, wrapper.Amount);
		AssertEquals("Money in Local Currency", charge.MoneyInLocalCurrency, wrapper.MoneyInLocalCurrency);
		AssertEquals("Currency", charge.Currency, wrapper.Currency);
		AssertEquals("IsDutiable", ZBool.True, wrapper.IsDutiable);
		AssertEquals("IsIncludedInLine", ZBool.True, wrapper.IsIncludedInLine);
		AssertEquals("Line Id", invoiceLine.PK, wrapper.LineId);
	}

	public void TestInvoiceApportionedChargeWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var charge = invoiceLine.ApportionedCharges.AddNew(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 20);
		charge.J7_IsDutiable = ZBool.True;
		charge.J7_IsIncludedInITOT = ZBool.True;
		IInvoiceLineChargeWrapper wrapper = new InvoiceLineApportionedChargeWrapper(charge);

		AssertEquals("Charge Code", charge.J7_ChargeType, wrapper.ChargeCode);
		AssertEquals("Charge Amount", charge.J7_Amount, wrapper.Amount);
		AssertEquals("Money in Local Currency", charge.MoneyInLocalCurrency, wrapper.MoneyInLocalCurrency);
		AssertEquals("Currency", charge.Currency, wrapper.Currency);
		AssertEquals("IsDutiable", ZBool.True, wrapper.IsDutiable);
		AssertEquals("IsIncludedInLine", ZBool.True, wrapper.IsIncludedInLine);
		AssertEquals("Line Id", invoiceLine.PK, wrapper.LineId);
		AssertEquals("Currency Converter", charge.CurrencyConverter, wrapper.CurrencyConverter);
	}
}
