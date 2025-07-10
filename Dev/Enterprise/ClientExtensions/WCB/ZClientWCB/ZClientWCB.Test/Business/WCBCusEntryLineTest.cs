using System;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(WCBCusEntryLine))]
	class WCBCusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<WCBCusEntryLine, JobComInvoiceLine>
	{
		public void TestDescription()
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			JobComInvoiceLine line = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			WCBCusEntryLine entryLine = (WCBCusEntryLine)dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			line.JI_Description = "LineDescription";
			AssertEquals("EntryLine.Description", entryLine.Description, "LineDescription");
			line.JI_PartAttrib1 = "Attribute1   ";
			AssertEquals("EntryLine.Description", entryLine.Description, "Attribute1 LineDescription");
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);
	}
}
