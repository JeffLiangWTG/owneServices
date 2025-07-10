using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public partial class TransactionWithCashAdvanceRequestValidationVisitorTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_HasError()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal2 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 300M, 300M);
			charge2.JR_CAL_ARLine = cal2.PK;
			var (cah3, cal3) = CreateCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "EUR");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var errorMessages = new Dictionary<string, List<string>>();
			var invoice = GetInvoice(charge1, charge3, charge4);
			var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
			validator.Visit(invoice);

			var expepctedFullMessage = FormattableString.Invariant($@"One or more charges have a paid Advance Payment in a different currency to the currency of the invoice being posted. Unable to apply Advance Payment to the invoice.
If the Advance Payment was incorrectly created with the wrong currency, please cancel and recreate the Advance Payment in the correct currency.
***Important:
If the currency entered on this invoice is incorrect, the following steps must be taken to post the invoice:
1. Change the currency on this invoice.
2. Go to Actions and Save as Incomplete.
3. Close the invoice and go to the Payables > Incomplete Invoices module.
4. Open the invoice and post.
Note: If you do not change the invoice currency before saving as incomplete, the invoice cannot be posted from the Incomplete Invoices module. It will need to be canceled, and the invoice re-entered.
{ObjectCreator.Debtor.OH_Code}-{ObjectCreator.Debtor.OH_FullName} | {ObjectCreator.CC4.AC_Code} | {ObjectCreator.CC4.AC_DescMultilingual} | AUD | 150.0 | {cah4.CAH_RequestReferenceNumber}

One or more charges have an unpaid Advance Payment. However, not all charges linked to the same Advance Payment are being posted. Please post all charges that relate to a single Advance Payment. Otherwise, cancel the request in order to post this charge.
{ObjectCreator.Debtor.OH_Code}-{ObjectCreator.Debtor.OH_FullName} | {ObjectCreator.CC2.AC_Code} | {ObjectCreator.CC2.AC_DescMultilingual} | AUD | 300 | {cah1.CAH_RequestReferenceNumber}");

			AssertErrorMessage(errorMessages, expepctedFullMessage);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_HasNoInvoiceAmountLessThanCashAdvancePaidError()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);

			charge1.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;
			Factory.Save();

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, -100M, -100M);
			Factory.Save();

			var errorMessages = new Dictionary<string, List<string>>();
			var invoice = GetInvoice(charge1, charge2);
			var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
			validator.Visit(invoice);

			AssertEquals("Expect no error when Invoice Total is less than the Cash Advance Paid", 0, errorMessages.Count);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_NoError()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var (cah3, cal3) = CreateCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var errorMessages = new Dictionary<string, List<string>>();
			var invoice = GetInvoice(charge1, charge2, charge3, charge4);
			var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
			validator.Visit(invoice);
			AssertErrorMessage(errorMessages, string.Empty);
		}

		void AssertErrorMessage(Dictionary<string, List<string>> errorMessages, string expectedFullErrorMessage)
		{
			var fullErrorMessageBuilder = new ZStringBuilder();
			foreach (var errorMessage in errorMessages)
			{
				fullErrorMessageBuilder.Append(errorMessage.Key);
				fullErrorMessageBuilder.Append(string.Join(System.Environment.NewLine, errorMessage.Value));
				fullErrorMessageBuilder.Append(string.Empty);
			}
			var fullErrorMessage = fullErrorMessageBuilder.ToStringWithNewLineBetweenAppends();
			AssertMultilineASCIIEquals(expectedFullErrorMessage, fullErrorMessage);
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, ObjectCreator.Debtor, LedgerTypes.AccountsReceivable, localAmount, osAmount, currency);
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = status;
			return (cah, cal);
		}

		ARInvoice GetInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), currency: ObjectCreator.AUD, organisation: ObjectCreator.Debtor) as ARInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateARInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.SellCurrency, 1.0M, "DESC01", charge.JR_OSSellAmt);
				charge.WIP.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_ARLine = line.PK;
			}
			return invoice;
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
