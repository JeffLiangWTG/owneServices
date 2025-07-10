using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceGroupHeader))]
sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
{
	protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);

	public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.TopGroupInvoice;

			declaration.MakeNonPersistent();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(groupHeader.IncoTermAndChargeFactory);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<Common.CommonIncoTermAndCustomsChargeFactory>(groupHeader.IncoTermAndChargeFactory);
		});
	}
}
