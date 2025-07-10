using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing;

public abstract class BaseJobComInvoiceGroupHeaderValidationTest : TestCaseWithFactory
{
	public void TestExistence()
	{
		AssertNotNull("I Don't Think", testInvoiceGroupHeaderValidation);
	}

	#region Implementation

	protected JobDeclaration testJobDeclaration;
	protected JobComInvoiceGroupHeader testInvoiceGroupHeader;
	protected JobComInvoiceGroupHeaderValidation testInvoiceGroupHeaderValidation;
	protected abstract Customs.Business.JobComInvoiceGroupHeaderValidation GetNewValidationProvider(JobComInvoiceGroupHeader groupHeader);

	protected override void SetUp()
	{
		base.SetUp();
		testJobDeclaration = Factory.New<JobDeclaration>();
		testJobDeclaration.JE_ExportDate = ZDateTime.Today;
		testInvoiceGroupHeader = testJobDeclaration.JobComInvoiceGroupHeaders[0];
		testInvoiceGroupHeaderValidation = (JobComInvoiceGroupHeaderValidation)GetNewValidationProvider(testInvoiceGroupHeader);
	}
	#endregion
}
