using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class EnquiryFilterControlPresentationProvider : TestCaseWithFactory
	{
		public void TestIsTaxBranchColumnAvailable_DependsOnEnableTaxBranchReportingRegistry()
		{
			var provider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetEnquiryFilterControlPresentationProvider();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition:EnableTaxBranchReporting registry value is false", false, provider.IsTaxBranchColumnAvailable());

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableTaxBranchReporting registry value is true", true, provider.IsTaxBranchColumnAvailable());
		}
	}
}
