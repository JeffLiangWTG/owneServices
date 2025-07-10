using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	public sealed class IndiaAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestComplianceNumberRegistryDate_IsInAdditionalDataItems()
		{
			var dateValue = new DateTime(2022, 6, 21);
			var (batch, branch, countryFactory, logger) = CreateObjectsForTest();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, dateValue))
			{
				var dataItems = new IndiaAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(batch, branch, null, countryFactory, logger);
				AssertEquals(1, dataItems.Count);
				AssertEquals(dataItems[0].Key, "Reg_ComplianceNumFrom");
				AssertEquals(dataItems[0].Value, "2022-06-21");
			}
		}

		public void TestComplianceNumberRegistryDate_IsNotIncluded_WhenRegistryIsNotSet()
		{
			var (batch, branch, countryFactory, logger) = CreateObjectsForTest();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue))
			{
				var dataItems = new IndiaAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(batch, branch, null, countryFactory, logger);
				AssertEquals(0, dataItems.Count);
			}
		}

		public void TestComplianceNumberRegistryDate_UsesParameterToDetermineRegistryCompany()
		{
			var dateValueForCurrentCompany = new DateTime(2022, 6, 21);
			var dateValueForOtherCompany = new DateTime(2023, 7, 22);

			var (batch, _, countryFactory, logger) = CreateObjectsForTest();
			var otherBranch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			var otherCompany = otherBranch.Company;

			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateValueForCurrentCompany))
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateValueForOtherCompany))
			{
				var dataItemsForCurrentBranch = new IndiaAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(batch, GlbBranch.CurrentBranch, null, countryFactory, logger);
				AssertEquals(1, dataItemsForCurrentBranch.Count);
				AssertEquals(dataItemsForCurrentBranch[0].Key, "Reg_ComplianceNumFrom");
				AssertEquals(dataItemsForCurrentBranch[0].Value, "2022-06-21");

				var dataItemsForOtherBranch = new IndiaAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(batch, otherBranch, null, countryFactory, logger);
				AssertEquals(1, dataItemsForOtherBranch.Count);
				AssertEquals(dataItemsForOtherBranch[0].Key, "Reg_ComplianceNumFrom");
				AssertEquals(dataItemsForOtherBranch[0].Value, "2023-07-22");
			}
		}

		(AccEInvoicingBatch bizoBatch, GlbBranch branch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings) CreateObjectsForTest()
			=> (Factory.New<AccEInvoicingBatch>(), Factory.NewCompanyAndBranchWith(countryCode: "IN"), new IndiaEInvoicingObjectFactory(), new Logger());
	}
}
