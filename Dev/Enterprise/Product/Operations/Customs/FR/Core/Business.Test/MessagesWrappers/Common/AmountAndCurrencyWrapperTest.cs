using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class AmountAndCurrencyWrapperTest : TestCaseWithFactory
	{
		public void TestGetTotal_Cached()
		{
			SetupDeclaration();
			var groupCharge1 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m);
			groupCharge1.J7_IsSystem = true;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m);
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 20m);
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var chargeCodes = new ZString[] { Customs.Business.CustomsChargeTypeList.Codes.OtherCharges };
			var entryHeaderWrapper = AmountAndCurrencyWrapper.New(declaration.CustomsEntryHeaders[0], chargeCodes);

			var getTotalMethod = typeof(AmountAndCurrencyWrapper).GetMethod("GetTotal", BindingFlags.Instance | BindingFlags.NonPublic);
			var total1 = getTotalMethod.Invoke(entryHeaderWrapper, Array.Empty<object>());
			var total2 = getTotalMethod.Invoke(entryHeaderWrapper, Array.Empty<object>());

			AssertSame("Cached", total1, total2);
		}

		public void TestGetCurrency_NoNullReferenceError()
		{
			SetupDeclaration();
			var groupCharge1 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m);
			groupCharge1.J7_IsSystem = true;
			groupCharge1.J7_RX_NKCurrency = ZString.Empty;
			var invoceCharge = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m);
			invoceCharge.J7_RX_NKCurrency = ZString.Empty;
			var invoceLineCharge = invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 20m);
			invoceLineCharge.J7_RX_NKCurrency = ZString.Empty;
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var chargeCodes = new ZString[] { Customs.Business.CustomsChargeTypeList.Codes.OtherCharges };
			var entryHeaderWrapper = AmountAndCurrencyWrapper.New(declaration.CustomsEntryHeaders[0], chargeCodes);

			AssertEquals("No null reference exception: EntryHeader", Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion, entryHeaderWrapper.Currency);

			var entryLineWrapper = AmountAndCurrencyWrapper.New((CusEntryLine)declaration.CustomsEntryHeaders[0].AllEntryLines[0], chargeCodes);
			AssertEquals("No null reference exception: EntryLine", Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion, entryLineWrapper.Currency);
		}

		public void TestAmountAndCurrencyWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => AmountAndCurrencyWrapper.New((CusEntryHeader)null, Array.Empty<ZString>()));
			AssertExceptionThrown<ArgumentNullException>(() => AmountAndCurrencyWrapper.New((CusEntryLine)null, Array.Empty<ZString>()));
		}

		public void TestAmount()
		{
			SetupDeclaration();

			var groupCharge1 = invoice.GroupCharges.AddNew("AFT", 1000m, Enterprise.Core.Constants.CurrencyCodes.France);
			groupCharge1.J7_IsSystem = true;
			groupCharge1.J7_FullOrPartialApportionment = "PAA";
			var groupCharge2 = invoice.GroupCharges.AddNew("ANS", 2000m, Enterprise.Core.Constants.CurrencyCodes.France);
			groupCharge2.J7_IsSystem = true;
			groupCharge2.J7_FullOrPartialApportionment = "PAA";

			var invoicecharge1 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Enterprise.Core.Constants.CurrencyCodes.France);
			invoicecharge1.J7_IsSystem = true;
			var invoicecharge2 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 200m, Enterprise.Core.Constants.CurrencyCodes.France);
			invoicecharge2.J7_IsSystem = true;

				invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 10m, Enterprise.Core.Constants.CurrencyCodes.France);
				invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 20m, Enterprise.Core.Constants.CurrencyCodes.France);
				var appCharge1 = invoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 40m, Enterprise.Core.Constants.CurrencyCodes.France);
				appCharge1.J7_IsSystem = true;
				var appCharge2 = invoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 50m, Enterprise.Core.Constants.CurrencyCodes.France);
				appCharge2.J7_IsSystem = true;
				declaration.ResumeApportionment();
				Factory.Save();
				var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge(shutUp);

			Factory.Save();
			var lstCharges = new ZString[] { Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, "AFT", "ANS" };

			var entryHeaderWrapper = AmountAndCurrencyWrapper.New(declaration.CustomsEntryHeaders[0], lstCharges);
			AssertEquals("group charge wont be apportionned", 420m, entryHeaderWrapper.Amount);

			var entryline = (CusEntryLine)declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			var amountAndCurrencyWrapper = AmountAndCurrencyWrapper.New(entryline, lstCharges);
			AssertEquals(30m, amountAndCurrencyWrapper.Amount);

			var amountAndCurrencyWrapper2 = AmountAndCurrencyWrapper.New(entryline, lstCharges, true);
			AssertEquals(420m, amountAndCurrencyWrapper2.Amount);
		}

		public void TestResultCurrencyWhenSingleCurrency()
		{
			SetupDeclaration();

			var groupCharge1 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			groupCharge1.J7_IsSystem = true;
			var groupCharge2 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 2000m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			groupCharge2.J7_IsSystem = true;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 200m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);

			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 10m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 20m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);

			Factory.Save();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(shutUp);

			var lstCharges = new ZString[] { Customs.Business.CustomsChargeTypeList.Codes.OtherCharges };
			var entryHeaderWrapper = AmountAndCurrencyWrapper.New(declaration.CustomsEntryHeaders[0], lstCharges);

			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.UnitedStates, entryHeaderWrapper.Currency);

			var entryline = (CusEntryLine)declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			var entryLineWrapper = AmountAndCurrencyWrapper.New(entryline, lstCharges);

			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.UnitedStates, entryLineWrapper.Currency);
		}

		public void TestResultCurrencyWhenMultipleCurrencies()
		{
			SetupDeclaration();

			var groupCharge1 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m, Enterprise.Core.Constants.CurrencyCodes.France);
			groupCharge1.J7_IsSystem = true;
			var groupCharge2 = invoice.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 2000m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			groupCharge2.J7_IsSystem = true;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Enterprise.Core.Constants.CurrencyCodes.France);
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 200m, Enterprise.Core.Constants.CurrencyCodes.France);

			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 10m, Enterprise.Core.Constants.CurrencyCodes.France);
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 20m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);

			Factory.Save();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(shutUp);

			var lstCharges = new ZString[] { Customs.Business.CustomsChargeTypeList.Codes.OtherCharges };
			var entryHeaderWrapper = AmountAndCurrencyWrapper.New(declaration.CustomsEntryHeaders[0], lstCharges);

			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.France, entryHeaderWrapper.Currency);

			var entryline = (CusEntryLine)declaration.CustomsEntryHeaders[0].AllEntryLines[0];
			var entryLineWrapper = AmountAndCurrencyWrapper.New(entryline, lstCharges);

			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.France, entryLineWrapper.Currency);
		}
		public void TestGetAmountAndCurrencyWithCurrencyConvertion()
		{
			var jobHeader = new WrapperTestHelper().CreateTestCusEntryHeader();

			List<ZString> lstCharges = new List<ZString>();
			lstCharges.Add(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);
			lstCharges.Add(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.AdjustmentCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge);
			lstCharges.Add(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
			Factory.Save();

			CombineAssertions(() =>
		  {
			  var wrapper = AmountAndCurrencyWrapper.New(jobHeader, lstCharges);
			  AssertEquals(new ZDecimal(0.49), wrapper.Amount);
			  AssertEquals("EUR", wrapper.Currency);

			  wrapper = AmountAndCurrencyWrapper.New(jobHeader, lstCharges, AmountAndCurrencyWrapper.ShouldBeVatable);
			  AssertEquals(new ZDecimal(0.49), wrapper.Amount);

			  wrapper = AmountAndCurrencyWrapper.New(jobHeader, lstCharges, AmountAndCurrencyWrapper.ShouldNotBeVatable);
			  AssertEquals(new ZDecimal(0), wrapper.Amount);

			  wrapper = AmountAndCurrencyWrapper.New(jobHeader, lstCharges, AmountAndCurrencyWrapper.ShouldBeIncludedInInvoiceLine);
			  AssertEquals(new ZDecimal(0), wrapper.Amount);

			  wrapper = AmountAndCurrencyWrapper.New(jobHeader, lstCharges, AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoiceLine);
			  AssertEquals(new ZDecimal(0.49), wrapper.Amount);
		  });
		}

		public void TestMergedApportionnedCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cei2 = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var charge1 = invoice1.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4000m, Enterprise.Core.Constants.CurrencyCodes.France);

			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			invoiceLine11.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine11.JI_Tariff = "2203001010";
			invoiceLine11.JI_Description = "Unit Test 1";
			invoiceLine11.JI_LinePrice = 3;
			invoiceLine11.JI_CEI = cei.PK;
			var invoiceLine12 = invoice1.InvoiceLines.AddNew();
			invoiceLine12.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine12.JI_Tariff = "2203001011";
			invoiceLine12.JI_Description = "Unit Test 2";
			invoiceLine12.JI_LinePrice = 1;
			invoiceLine12.JI_CEI = cei2.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var charge2 = invoice2.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 400m, Enterprise.Core.Constants.CurrencyCodes.France);

			var invoiceLine21 = invoice2.InvoiceLines.AddNew();
			invoiceLine21.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine21.JI_Tariff = "2203001011";
			invoiceLine21.JI_Description = "Unit Test 3";
			invoiceLine21.JI_LinePrice = 200;
			invoiceLine21.JI_CEI = cei2.PK;
			var invoiceLine22 = invoice2.InvoiceLines.AddNew();
			invoiceLine22.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine22.JI_Tariff = "2203001010";
			invoiceLine22.JI_Description = "Unit Test 4";
			invoiceLine22.JI_LinePrice = 200;
			invoiceLine22.JI_CEI = cei.PK;

			declaration.ResumeApportionment();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(3000m, invoiceLine11.ApportionedCharges[0].J7_Amount);
				AssertEquals(1000m, invoiceLine12.ApportionedCharges[0].J7_Amount);
				AssertEquals(200m, invoiceLine21.ApportionedCharges[0].J7_Amount);
				AssertEquals(200m, invoiceLine22.ApportionedCharges[0].J7_Amount);
			});

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.AllEntryLines.AddNew();
			invoiceLine11.JI_CL = entryLine1.PK;
			invoiceLine22.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.AllEntryLines.AddNew();
			invoiceLine12.JI_CL = entryLine2.PK;
			invoiceLine21.JI_CL = entryLine2.PK;

			Factory.Save();

			var lstCharges = new ZString[] { FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge };
			var amountAndCurrencyWrapper = AmountAndCurrencyWrapper.New(entry1, lstCharges);
			AssertEquals(3200m, amountAndCurrencyWrapper.Amount);

			amountAndCurrencyWrapper = AmountAndCurrencyWrapper.New(entry2, lstCharges);
			AssertEquals(1200m, amountAndCurrencyWrapper.Amount);
		}

		void SetupDeclaration()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			var cei = declaration.CustomsEntryInstructions.AddNew();

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
