using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var invAsProvider = invoice as ICurrencyConverterDataProvider;

			AssertEquals("JE_MessageType = IMP, Rate Type should be", ExchangeRateType.Customs, invAsProvider.RateType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType = EXP, Rate Type should be", ExchangeRateType.CustomsSecondary, invAsProvider.RateType);
		}

		[TestDate(2023, 1, 1)]
		public void TestExchangeRate()
		{
			ReferenceTestDataHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.NewZealand, 33.16, ZDateTime.Today, ExchangeRateType.Customs);
			ReferenceTestDataHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.NewZealand, 21.41, ZDateTime.Today, ExchangeRateType.CustomsSecondary);

			Factory.Save();

			var decExp = Factory.New<JobDeclaration>();
			decExp.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invExp = decExp.Invoices.AddNew();
			invExp.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(21.41m, invExp.JZ_InvoiceCurrExRate);

			var decImp = Factory.New<JobDeclaration>();
			decImp.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invImp = decImp.Invoices.AddNew();
			invImp.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(33.16m, invImp.JZ_InvoiceCurrExRate);
		}

		public override void TestLocalCurrencyCodeCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Mexico, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}
}

