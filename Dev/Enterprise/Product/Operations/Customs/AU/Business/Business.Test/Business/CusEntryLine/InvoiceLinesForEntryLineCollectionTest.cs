using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceLinesForEntryLineCollection))]
	public class InvoiceLinesForEntryLineCollectionTest : Customs.Business.Testing.InvoiceLinesForEntryLineCollectionTest
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = EntryLine.PK;
			if (TestDec.InvoiceLines.Contains(invoiceLine))
			{
				TestDec.InvoiceLines.Remove(invoiceLine);
			}

			return invoiceLine;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLinesForEntryLineCollection((CusEntryLine)EntryLine);
		}

		#endregion
	}
}
