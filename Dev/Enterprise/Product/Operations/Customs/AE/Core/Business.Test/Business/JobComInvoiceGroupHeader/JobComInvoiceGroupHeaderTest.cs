using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobComInvoiceGroupHeader))]
class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
{
	public void TestTypeDecider()
	{
		Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == typeof(JobComInvoiceGroupHeader));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		return declaration.JobComInvoiceGroupHeaders[0];
	}
}
