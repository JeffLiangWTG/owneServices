using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationPaymentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationPaymentProvider()
		{
			ReferenceTestDataHelper.CreateTaxRevenueTypeList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var payments = DeclarationPaymentProvider.New(cusEntryHeader);
			AssertEquals("No DeclarationPaymentProvider created", 0, payments.Count());

			int amount = 1;
			var mappings = BRRefCusMapper.GetTaxRevenueCodeMapping(cusEntryHeader.Factory);
			mappings.ForEach(mapping =>
			{
				foreach (var entryLine in cusEntryHeader.MergedLines)
				{
					var entryLineFee1 = entryLine.Fees.GetOrAddFeeByFeeType(mapping.Key);
					entryLineFee1.CF_ChargeAmount = amount * 10m;
				}
				amount++;
			});

			payments = DeclarationPaymentProvider.New(cusEntryHeader);
			AssertEquals("DeclarationPaymentProvider created", mappings.Count, payments.Count());

			CombineAssertions(() =>
			{
				amount = 1;
				mappings.ForEach(mapping =>
				{
					var payment = payments.FirstOrDefault(e => e.TaxRevenueCode == mapping.Value);
					AssertEquals($"{mapping.Key} TaxRevenueCode", mapping.Value, payment.TaxRevenueCode);
					AssertEquals($"{mapping.Key} Amount", amount++ * 20m, payment.Amount);
				});
			});
		}

		public void TestDeclarationPaymentWithNoAmountProvider()
		{
			ReferenceTestDataHelper.CreateTaxRevenueTypeList(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var payments = DeclarationPaymentProvider.New(cusEntryHeader);
			AssertEquals("No DeclarationPaymentProvider created", 0, payments.Count());

			var mappings = BRRefCusMapper.GetTaxRevenueCodeMapping(cusEntryHeader.Factory);
			mappings.ForEach(mapping => cusEntryHeader.MergedLines.ForEach(line => line.Fees.GetOrAddFeeByFeeType(mapping.Key)));

			payments = DeclarationPaymentProvider.New(cusEntryHeader);
			AssertEquals("Tax Revenue list", 8, mappings.Count);
			AssertEquals("DeclarationPaymentProvider created", 0, payments.Count());
		}
	}
}
