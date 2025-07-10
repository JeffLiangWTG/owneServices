using System;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
