using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class APInvoicePrintingUserControlPresentationProviderTest : TestCaseWithFactory
	{
		public void TestIsTaxBranchColumnAvailable_DependsOnEnableTaxBranchReportingRegistry()
		{
			var presentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetAPJobInvoicePrintingUserControlPresentationProvider();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition:EnableTaxBranchReporting registry value is false", false, presentationProvider.IsTaxBranchColumnAvailable());

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableTaxBranchReporting registry value is true", true, presentationProvider.IsTaxBranchColumnAvailable());
		}
	}
}
