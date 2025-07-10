using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[TestedType(typeof(NonPersistentFee))]
	sealed class NonPersistentFeeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null Fee", () => new NonPersistentFee(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("Null Factory", () => new NonPersistentFee(Factory.New<CusEntryLineFee>(), null));
			});
		}

		public void TestProperties()
		{
			var newFee = GetFee("A00", 50m, "A");
			EntryLine.Fees.Add(newFee);
			var nonPersistentFee = new NonPersistentFee(newFee, Factory);

			CombineAssertions("NonPersistentFee", () =>
			{
				AssertEquals("Type", newFee.CF_ChargeType, nonPersistentFee.Type);
				AssertEquals("TaxBase", ZString.Empty, nonPersistentFee.TaxBase);
				AssertEquals("Rate", ZString.Empty, nonPersistentFee.Rate);
				AssertEquals("RateDuty", ZString.Empty, nonPersistentFee.RateDuty);
				AssertEquals("RateOverride", ZString.Empty, nonPersistentFee.RateOverride);
				AssertEquals("AmountInDeclarationCurrency", newFee.CF_ChargeAmount.ToString("N2"), nonPersistentFee.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", newFee.CF_MethodOfPayment, nonPersistentFee.MethodOfPayment);
				AssertEquals("NationalFeeTypeCode", ZString.Empty, nonPersistentFee.NationalFeeTypeCode);
				AssertEquals("DeclarationMethodOfPayment", ZString.Empty, nonPersistentFee.DeclarationMethodOfPayment);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fee = GetFee("A00", 50m, "A");
			EntryLine.Fees.Add(fee);
			return new NonPersistentFee(fee, Factory);
		}

		CusEntryLineFee GetFee(ZString type, ZDecimal amount, ZString methodOfPay)
		{
			var fee = Factory.New<CusEntryLineFee>();

			fee.CF_ChargeType = type;
			fee.CF_ChargeAmount = amount;
			fee.CF_MethodOfPayment = methodOfPay;
			return fee;
		}

		protected override void SetUp()
		{
			Declaration = Factory.New<JobDeclaration>();
			Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			JobComInvoiceHeader invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.ZG_MethodOfPayment = "A";

			Declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders[0];
			EntryLine = entryHeader.MergedLines[0];
		}

		JobDeclaration Declaration;

		CusEntryLine EntryLine;
	}
}
