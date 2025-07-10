using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		protected override ZString ExpectedFallbackEntrylineDescription => InvoiceLinePartClassificationTariffDescriptionSyncroniserTest.TariffDescriptionCore;

		protected override bool RatesAreReciprocal => true;

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}
