using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using TransportTypeGenericList = Enterprise.Customs.Business.CustomsLists.TransportTypeGenericList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportLineCustomsValueProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportLineCustomsValueProvider>
	{
		public void TestConstructor_ArgumentNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Parameter declaration is null", () => new ImportLineCustomsValueProvider(null, new[] { invoiceLine }));
				AssertExceptionThrown<ArgumentException>("Parameter invoiceLines is null", () => new ImportLineCustomsValueProvider(declaration, null));
				AssertExceptionThrown<ArgumentException>("Parameter invoiceLines is empty list", () => new ImportLineCustomsValueProvider(declaration, new List<JobComInvoiceLine>()));
			});
		}

		public void TestCustomsValueDepartureAirport()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;
			declaration.JE_IATALoadPort = "DXB";
			AssertEquals("DXB", Provider.CustomsValueDepartureAirport);
		}

		public void TestCustomsValueDepartureAirport_TransportModeNotAIR()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Road;
			declaration.JE_IATALoadPort = "DXB";
			AssertNull(Provider.CustomsValueDepartureAirport);
		}

		public void TestCustomsValueDestinationPlace()
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_PortName = "Wallersdorf";
			port.RL_Code = "TEST";

			declaration.JE_RL_NKPortOfFirstArrival = "TEST";
			AssertEquals("Wallersdorf", Provider.CustomsValueDestinationPlace);
		}

		public void TestCustomsValueAdditionDeductionDescription()
		{
			additionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._016;
			additionDeductionCharge.J7_ChargeDescription = "MIN";
			AssertEquals("MIN", Provider.CustomsValueAdditionDeductionDescription);
		}

		public void TestCustomsValueAdditionDeductionDescription_Empty()
		{
			additionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._017;
			additionDeductionCharge.J7_ChargeDescription = "MIN";
			AssertNull(Provider.CustomsValueAdditionDeductionDescription);
		}

		public void TestCustomsValueNetPrice()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			invoiceLine.JI_NetPrice = 20145.63;
			invoiceLine2.JI_NetPrice = 111.11;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceCurrExRate = 0.89;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;

			CombineAssertions(() =>
			{
				var netPrice = Provider.CustomsValueNetPrice;
				AssertEquals("Cached", netPrice, Provider.CustomsValueNetPrice);
				AssertEquals(20256.74m, netPrice.Value);
				AssertEquals(CurrencyCodes.UnitedStates, netPrice.CurrencyCode);
				Assert(netPrice.CurrencyRateAgreedFlag);
				AssertEquals(0.89m, netPrice.CurrencyRate);
			});
		}

		public void TestCustomsValueIndirectPayment()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			var indirectPaymentCharge = invoiceLine2.Charges.AddNew();
			indirectPaymentCharge.J7_Amount = 111.22;
			indirectPaymentCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			indirectPaymentCharge.J7_ExchangeRate = 0.87;
			indirectPaymentCharge.IsJ7_ExchangeRateUserEnterable = true;
			indirectPaymentCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;

			CombineAssertions(() =>
			{
				var indPayment = Provider.CustomsValueIndirectPayment;
				AssertEquals("Cached", indPayment, Provider.CustomsValueIndirectPayment);
				AssertEquals(18256.85m, indPayment.Value);
				AssertEquals(CurrencyCodes.UnitedStates, indPayment.CurrencyCode);
				Assert(indPayment.CurrencyRateAgreedFlag);
				AssertEquals(0.87m, indPayment.CurrencyRate);
			});
		}

		public void TestCustomsValueIndirectPayment_WrongType()
		{
			indirectPaymentCharge.J7_ChargeType = "INF";
			AssertNull(Provider.CustomsValueIndirectPayment);
		}

		public void TestCustomsValueAirFreightCosts()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;

			var airFreightCharge14 = invoiceLine.Charges.AddNew();
			airFreightCharge14.J7_Amount = 155.27;
			airFreightCharge14.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			airFreightCharge14.J7_ExchangeRate = 0.87;
			airFreightCharge14.J7_ExchangeRateDate = date;
			airFreightCharge14.J7_ChargeType = ImportChargeCodeList.Codes._014;
			airFreightCharge14.IsJ7_ExchangeRateUserEnterable = true;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			var airFreightCharge14a = invoiceLine2.Charges.AddNew();
			airFreightCharge14a.J7_Amount = 123.45;
			airFreightCharge14a.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			airFreightCharge14a.J7_ExchangeRate = 0.87;
			airFreightCharge14a.J7_ExchangeRateDate = date;
			airFreightCharge14a.J7_ChargeType = ImportChargeCodeList.Codes._014;
			airFreightCharge14a.IsJ7_ExchangeRateUserEnterable = true;

			CombineAssertions(() =>
			{
				var airFreightCosts = Provider.CustomsValueAirFreightCosts;
				AssertEquals("Cached", airFreightCosts, Provider.CustomsValueAirFreightCosts);
				AssertEquals(18424.35m, airFreightCosts.Value);
				AssertEquals(CurrencyCodes.UnitedStates, airFreightCosts.CurrencyCode);
				AssertEquals(false, airFreightCosts.CurrencyRateIATA);
				Assert(airFreightCosts.CurrencyRateAgreedFlag);
				AssertEquals(0.87m, airFreightCosts.CurrencyRate);
				AssertEquals(date, airFreightCosts.CurrencyRateDate);
			});
		}

		public void TestCustomsValueAirFreightCosts_ApportionedCharges()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;

			var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge.J7_Amount = 155.27;
			apportionedCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			apportionedCharge.J7_ExchangeRate = 0.87;
			apportionedCharge.J7_ExchangeRateDate = date;
			apportionedCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
			apportionedCharge.IsJ7_ExchangeRateUserEnterable = true;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			var airFreightCharge14a = invoiceLine2.ApportionedCharges.AddNew();
			airFreightCharge14a.J7_Amount = 11.11;
			airFreightCharge14a.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			airFreightCharge14a.J7_ExchangeRate = 0.87;
			airFreightCharge14a.J7_ExchangeRateDate = date;
			airFreightCharge14a.J7_ChargeType = ImportChargeCodeList.Codes._014;
			airFreightCharge14a.IsJ7_ExchangeRateUserEnterable = true;

			CombineAssertions(() =>
			{
				var airFreightCosts = Provider.CustomsValueAirFreightCosts;
				AssertEquals("Cached", airFreightCosts, Provider.CustomsValueAirFreightCosts);
				AssertEquals(18312.01m, airFreightCosts.Value);
				AssertEquals(CurrencyCodes.UnitedStates, airFreightCosts.CurrencyCode);
				AssertEquals(false, airFreightCosts.CurrencyRateIATA);
				Assert(airFreightCosts.CurrencyRateAgreedFlag);
				AssertEquals(0.87m, airFreightCosts.CurrencyRate);
				AssertEquals(date, airFreightCosts.CurrencyRateDate);
			});
		}

		public void TestCustomsValueAirFreightCosts_JE_TransportModeNotAIR()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Road;
			AssertNull(Provider.CustomsValueAirFreightCosts);
		}

		public void TestCustomsValueAirFreightCosts_NoChargesFound()
		{
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;
			airFreightCharge.J7_ChargeType = ImportChargeCodeList.Codes._016;
			AssertNull(Provider.CustomsValueAirFreightCosts);
		}

		public void TestCustomsValueAdditionDeduction()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			var thirdAdditionDeductionCharge = invoiceLine2.Charges.AddNew();
			thirdAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
			thirdAdditionDeductionCharge.J7_Amount = 5.00;
			thirdAdditionDeductionCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			thirdAdditionDeductionCharge.J7_ExchangeRate = 0.87;
			thirdAdditionDeductionCharge.J7_ExchangeRateDate = date;
			thirdAdditionDeductionCharge.IsJ7_ExchangeRateIATA = false;
			thirdAdditionDeductionCharge.IsJ7_ExchangeRateUserEnterable = true;

			var fourthAdditionDeductionCharge = invoiceLine2.ApportionedCharges.AddNew();
			fourthAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			fourthAdditionDeductionCharge.J7_Amount = 4.00;
			fourthAdditionDeductionCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			fourthAdditionDeductionCharge.J7_ExchangeRate = 0.87;
			fourthAdditionDeductionCharge.J7_ExchangeRateDate = date;
			fourthAdditionDeductionCharge.IsJ7_ExchangeRateUserEnterable = true;

			var fifthAdditionDeductionCharge = invoiceLine2.ApportionedCharges.AddNew();
			fifthAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._003;
			fifthAdditionDeductionCharge.J7_Amount = 6.00;

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeGenericList.Codes.Rail;

				var additions = Provider.CustomsValueAdditionDeduction;
				AssertEquals("Count", 3, additions.Count);
				AssertEquals("Cached", additions, Provider.CustomsValueAdditionDeduction);

				var addition = additions.FirstOrDefault(x => x.Type == ImportChargeCodeList.Codes._001);
				var addition2 = additions.FirstOrDefault(x => x.Type == ImportChargeCodeList.Codes._010);
				var addition3 = additions.FirstOrDefault(x => x.Type == ImportChargeCodeList.Codes._003);
				AssertEquals("Charge 1 amount", 18150.63m, addition.Value);
				AssertEquals(CurrencyCodes.UnitedStates, addition.CurrencyCode);
				AssertEquals("Is IATA", false, addition.CurrencyRateIATA);
				Assert("Rate agreed", addition.CurrencyRateAgreedFlag);
				AssertEquals(0.87m, addition.CurrencyRate);
				AssertEquals(date, addition.CurrencyRateDate);
				AssertEquals("Charge 2 amount", 18149.63m, addition2.Value);
				AssertEquals("Charge 3 amount", 6.00m, addition3.Value);
			});
		}

		public void TestCustomsValueAdditionDeduction_WrongType()
		{
			additionDeductionCharge.J7_ChargeType = "020";
			airFreightCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
			AssertEquals(false, Provider.CustomsValueAdditionDeduction.Any());
		}

		public void TestCustomsValueAdditionDeduction_JE_TransportModeAIR()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;

				var additions = Provider.CustomsValueAdditionDeduction;
				AssertEquals("Count", 1, additions.Count);
			});
		}

		public void TestAdditionDeductionCharges()
		{
			var secondAdditionDeductionCharge = invoiceLine.Charges.AddNew();
			secondAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._002;
			declaration.JE_TransportMode = TransportTypeGenericList.Codes.Air;
			var result = Provider.CustomsValueAdditionDeduction;
			AssertContainsExactElementsInAnyOrder("Charges", new[] { ImportChargeCodeList.Codes._001, ImportChargeCodeList.Codes._002 }, result.Select(x => x.Type));
		}

		public void TestAdditionDeductionChargesApportioned()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1500F01";

			var additionDeductionApportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			additionDeductionApportionedCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
			additionDeductionApportionedCharge.J7_Amount = 1.00;
			additionDeductionApportionedCharge.J7_RX_NKCurrency = CurrencyCodes.Germany;
			additionDeductionApportionedCharge.J7_ExchangeRate = 1;
			additionDeductionApportionedCharge.J7_ExchangeRateDate = date;
			additionDeductionApportionedCharge.IsJ7_ExchangeRateIATA = false;
			additionDeductionApportionedCharge.IsJ7_ExchangeRateUserEnterable = true;

			var airFreightApportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			airFreightApportionedCharge.J7_Amount = 2.00;
			airFreightApportionedCharge.J7_RX_NKCurrency = CurrencyCodes.Germany;
			airFreightApportionedCharge.J7_ExchangeRate = 1;
			airFreightApportionedCharge.J7_ExchangeRateDate = date;
			airFreightApportionedCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			airFreightApportionedCharge.IsJ7_ExchangeRateUserEnterable = true;

			var secondAdditionDeductionCharge = invoiceLine.ApportionedCharges.AddNew();
			secondAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._002;
			secondAdditionDeductionCharge.J7_Amount = 1.00;

			var thirdAdditionDeductionCharge = invoiceLine2.ApportionedCharges.AddNew();
			thirdAdditionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			thirdAdditionDeductionCharge.J7_Amount = 1.50;

			var result = Provider.CustomsValueAdditionDeduction;
			CombineAssertions(() =>
			{
				AssertEquals(3, result.Count);
				AssertContainsExactElementsInAnyOrder("Types", new[] { ImportChargeCodeList.Codes._001, ImportChargeCodeList.Codes._002, ImportChargeCodeList.Codes._010 }, result.Select(x => x.Type));
				AssertContainsExactElementsInAnyOrder("Values", new[] { 18146.63m, 1m, 18149.13m }, result.Select(x => x.Value));
			});
		}

		protected override ImportLineCustomsValueProvider GetProvider() => dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			date = new ZDate(2020, 10, 20);

			declaration = Factory.New<JobDeclaration>();
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1500F01";

			additionDeductionCharge = invoiceLine.Charges.AddNew();
			additionDeductionCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
			additionDeductionCharge.J7_Amount = 18145.63;
			additionDeductionCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			additionDeductionCharge.J7_ExchangeRate = 0.87;
			additionDeductionCharge.J7_ExchangeRateDate = date;
			additionDeductionCharge.IsJ7_ExchangeRateIATA = false;
			additionDeductionCharge.IsJ7_ExchangeRateUserEnterable = true;

			indirectPaymentCharge = invoiceLine.Charges.AddNew();
			indirectPaymentCharge.J7_Amount = 18145.63;
			indirectPaymentCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			indirectPaymentCharge.J7_ExchangeRate = 0.87;
			indirectPaymentCharge.IsJ7_ExchangeRateUserEnterable = true;
			indirectPaymentCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;

			airFreightCharge = invoiceLine.Charges.AddNew();
			airFreightCharge.J7_Amount = 18145.63;
			airFreightCharge.J7_RX_NKCurrency = CurrencyCodes.UnitedStates;
			airFreightCharge.J7_ExchangeRate = 0.87;
			airFreightCharge.J7_ExchangeRateDate = date;
			airFreightCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			airFreightCharge.IsJ7_ExchangeRateUserEnterable = true;

			dataProvider = new ImportLineCustomsValueProvider(declaration, invoice.InvoiceLines.Cast<JobComInvoiceLine>());
		}
		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ImportLineCustomsValueProvider dataProvider;
		InvoiceLineCharge additionDeductionCharge;
		InvoiceLineCharge indirectPaymentCharge;
		InvoiceLineCharge airFreightCharge;
		ZDate date;
	}
}
