using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		// to be overridden once the Tariff is setup for a new country
		protected override ZString ExpectedFallbackEntrylineDescription => "TARIFF_DESCRIPTION";

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}

