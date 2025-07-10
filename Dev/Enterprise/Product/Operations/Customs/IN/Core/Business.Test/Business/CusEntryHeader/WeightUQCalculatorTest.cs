using System;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(WeightUQCalculator))]

sealed class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
{
	protected override Type JobComInvoiceHeaderType => typeof(JobComInvoiceHeader);

	protected override Type CusEntryHeaderType => typeof(CusEntryHeader);
}
