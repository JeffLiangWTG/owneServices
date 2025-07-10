using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingPenaltyTaxInfo))]
	public class KoreaSouthEInvoicingPenaltyTaxInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitializePenaltyTaxDetails()
		{
			var penaltyTaxInfo = new KoreaSouthEInvoicingPenaltyTaxInfo();
			AssertEquals(3, penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices.Count);
			AssertEquals("Not Issued", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[0].Type);
			AssertEquals("Delayed Issued", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[1].Type);
			AssertEquals("Issued by paper", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[2].Type);
			AssertEquals(2, penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices.Count);
			AssertEquals("Not Transmitted", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[0].Type);
			AssertEquals("Delayed Transmitted", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[1].Type);
			AssertEquals("Where a tax invoice is not issued by the deadline for filing a final return for the taxable period during which the relevant goods or services are supplied after the elapse of the time limit for issuing tax invoice.", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[0].Explanation);
			AssertEquals("Where a tax invoice is issued by the deadline for filing a final return for the taxable period during which the relevant goods or services are supplied after the elapse of the time limit for issuing tax invoices.", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[1].Explanation);
			AssertEquals("Where a tax invoice is issued other than the required electronic tax invoice, during the time limit for issuing tax invoices.", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[2].Explanation);
			AssertEquals("2%", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[0].Supplier);
			AssertEquals("Non-deductible input tax", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[0].Receiver);
			AssertEquals("1%", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[1].Supplier);
			AssertEquals("0.50%", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[1].Receiver);
			AssertEquals("1%", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[2].Supplier);
			AssertEquals("Not applicable", penaltyTaxInfo.PenaltyTaxDetailsForNotIssuedOrDelayedIssuedInvoices[2].Receiver);
			AssertEquals("Where the electronic tax invoice was not transmitted to NTS within the eleventh day of the month following the month in which the date of supply of goods or services falls.", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[0].Explanation);
			AssertEquals("1%", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[0].Supplier);
			AssertEquals("Not applicable", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[0].Receiver);
			AssertEquals("Where an electronic tax invoice is transmitted to the NTS after the eleventh day of the month following the month in which the date of supply of goods or services falls.", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[1].Explanation);
			AssertEquals("0.50%", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[1].Supplier);
			AssertEquals("Not applicable", penaltyTaxInfo.PenaltyTaxDetailsForNotTransmittedOrDelayedTransmittedInvoices[1].Receiver);
		}
	}
}
