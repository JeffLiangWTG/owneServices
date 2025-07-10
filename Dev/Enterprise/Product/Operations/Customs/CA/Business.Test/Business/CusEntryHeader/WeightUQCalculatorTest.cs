using System;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		protected override Type JobComInvoiceHeaderType
		{
			get { return typeof(JobComInvoiceHeader); }
		}

		protected override Type CusEntryHeaderType
		{
			get { return typeof(CusEntryHeader); }
		}
	}
}
