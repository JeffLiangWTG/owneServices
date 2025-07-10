using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentCustomsValuationWrapperTest
		: DataProviderTestCase<IDeclarationGoodsShipmentCustomsValuation>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceCharge is null", DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(null));
		}

		public void TestExitToEntryChargeAmount()
		{
			var declarationGoodsShipmentCustomsValuationOFT = GetProvider();
			var declarationGoodsShipmentCustomsValuationONS = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeONS) as IDeclarationGoodsShipmentCustomsValuation);
			var declarationGoodsShipmentCustomsValuationOther = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeOther) as IDeclarationGoodsShipmentCustomsValuation);

			AssertNull("When ChargeType is OFT", declarationGoodsShipmentCustomsValuationOFT.ExitToEntryChargeAmount);

			AssertEquals("When ChargeType is ONS, Amount", chargeONS.J7_Amount, declarationGoodsShipmentCustomsValuationONS.ExitToEntryChargeAmount.Value);
			AssertEquals("When ChargeType is ONS, Currency", chargeONS.J7_RX_NKCurrency, declarationGoodsShipmentCustomsValuationONS.ExitToEntryChargeAmount.CurrencyID.ToString().ToUpper());

			AssertNull("When ChargeType is Other", declarationGoodsShipmentCustomsValuationOther.ExitToEntryChargeAmount);
		}

		public void TestFreightChargeAmount()
		{
			var declarationGoodsShipmentCustomsValuationOFT = GetProvider();
			var declarationGoodsShipmentCustomsValuationONS = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeONS) as IDeclarationGoodsShipmentCustomsValuation);
			var declarationGoodsShipmentCustomsValuationOther = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeOther) as IDeclarationGoodsShipmentCustomsValuation);

			AssertEquals("When ChargeType is OFT, Amount", chargeOFT.J7_Amount, declarationGoodsShipmentCustomsValuationOFT.FreightChargeAmount.Value);
			AssertEquals("When ChargeType is OFT, Currency", chargeOFT.J7_RX_NKCurrency, declarationGoodsShipmentCustomsValuationOFT.FreightChargeAmount.CurrencyID.ToString().ToUpper());

			AssertNull("When ChargeType is ONS", declarationGoodsShipmentCustomsValuationONS.FreightChargeAmount);

			AssertNull("When ChargeType is Other", declarationGoodsShipmentCustomsValuationOther.FreightChargeAmount);
		}

		public void TestChargesTypeCode()
		{
			var declarationGoodsShipmentCustomsValuationOFT = GetProvider();
			var declarationGoodsShipmentCustomsValuationONS = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeONS) as IDeclarationGoodsShipmentCustomsValuation);
			var declarationGoodsShipmentCustomsValuationOther = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeOther) as IDeclarationGoodsShipmentCustomsValuation);

			AssertEquals("When ChargeType is OFT", chargeOFT.J7_ChargeType, declarationGoodsShipmentCustomsValuationOFT.ChargesTypeCode.Value);

			AssertEquals("When ChargeType is ONS", chargeONS.J7_ChargeType, declarationGoodsShipmentCustomsValuationONS.ChargesTypeCode.Value);

			AssertEquals("When ChargeType is Other", chargeOther.J7_ChargeType, declarationGoodsShipmentCustomsValuationOther.ChargesTypeCode.Value);
		}

		public void TestOtherChargeDeductionAmount()
		{
			var declarationGoodsShipmentCustomsValuationOFT = GetProvider();
			var declarationGoodsShipmentCustomsValuationONS = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeONS) as IDeclarationGoodsShipmentCustomsValuation);
			var declarationGoodsShipmentCustomsValuationOther = (DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeOther) as IDeclarationGoodsShipmentCustomsValuation);

			AssertNull("When ChargeType is OFT", declarationGoodsShipmentCustomsValuationOFT.OtherChargeDeductionAmount);

			AssertNull("When ChargeType is ONS", declarationGoodsShipmentCustomsValuationONS.OtherChargeDeductionAmount);

			AssertEquals("When ChargeType is Other, Amount", chargeOther.J7_Amount, declarationGoodsShipmentCustomsValuationOther.OtherChargeDeductionAmount.Value);
			AssertEquals("When ChargeType is ONS, Currency", chargeOther.J7_RX_NKCurrency, declarationGoodsShipmentCustomsValuationOther.OtherChargeDeductionAmount.CurrencyID.ToString().ToUpper());
		}

		protected override IDeclarationGoodsShipmentCustomsValuation GetProvider()
			=> DeclarationGoodsShipmentCustomsValuationWrapper.NewOrNull(chargeOFT);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceAmount = 0.2m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_InvoiceDisplaySequence = 1;
			invoiceHeader.JZ_PaymentTerms = "POB";

			chargeOFT = invoiceHeader.Charges.AddNew();
			chargeOFT.J7_ChargeType = "OFT";
			chargeOFT.J7_Amount = 10.2m;
			chargeOFT.J7_RX_NKCurrency = "ILS";

			chargeONS = invoiceHeader.Charges.AddNew();
			chargeONS.J7_ChargeType = "ONS";
			chargeONS.J7_Amount = 10.2m;
			chargeONS.J7_RX_NKCurrency = "ILS";

			chargeOther = invoiceHeader.Charges.AddNew();
			chargeOther.J7_ChargeType = "COM";
			chargeOther.J7_Amount = 10.2m;
			chargeOther.J7_RX_NKCurrency = "ILS";
		}

		JobComInvoiceHeader invoiceHeader;
		InvoiceCharge chargeOFT;
		InvoiceCharge chargeONS;
		InvoiceCharge chargeOther;
	}
}
