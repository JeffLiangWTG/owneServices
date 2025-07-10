using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(ELGDataRegistry))]
	class ELGDataRegistryTest : RegistryItemSetTestCaseWithFactory<ELGDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Number of registry items", 4, AllItems.Count);
			AssertVisible("ELGDataTransferSwitchRegistryItem");
			AssertVisible("ELGBranchDepartmentCodeTranslation");
			AssertVisible("ELGTransportAndChargeCodeTranslation");
			AssertVisible("ELGSageAccountCodeTranslation");
		}

		public void TestRegistryItems()
		{
			AssertEquals("ELGBranchDepartmentCodeTranslation", 0, ItemSet.BranchDepartmentCodeCollectionItem.Value.Count);
			TestHelper.SetValidRegistryBranchDepartmentCodeCollectionItem();
			AssertEquals("ELGBranchDepartmentCodeTranslation", "ABC", ItemSet.BranchDepartmentCodeCollectionItem.Value.FindProfitCentre("SYD", "BRN"));
			AssertEquals("ELGBranchDepartmentCodeTranslation", "ASD", ItemSet.BranchDepartmentCodeCollectionItem.Value.FindNominalDepartmentCentre("SYD", "BRN"));
			AssertEquals("ELGTransportAndChargeCodeTranslation", 0, ItemSet.TransportAndChargeCodeCollectionItem.Value.Count);
			TestHelper.SetValidRegistryTransportAndChargeCodeCollectionItem();
			AssertEquals("ELGTransportAndChargeCodeTranslation", "ABC", ItemSet.TransportAndChargeCodeCollectionItem.Value.FindNominalCostCode("SEA", "FRT"));
			AssertEquals("ELGTransportAndChargeCodeTranslation", "XYZ", ItemSet.TransportAndChargeCodeCollectionItem.Value.FindNominalRevenueCode("SEA", "FRT"));
			AssertEquals("ELGSageAccountCodeTranslation", 0, ItemSet.SageAccountCodeCollectionItem.Value.Count);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem();
			AssertEquals("ELGSageAccountCodeTranslation", "DEANO.AUD.AR", ItemSet.SageAccountCodeCollectionItem.Value.FindSageAccountCode("DEAENTSYD", "AUD", "AR"));
		}

		[TestDate(2006, 11, 10)]
		public void TestDataExportRegistryItem()
		{
			AssertEquals("Default ExportSagAccountsDirectory", ZString.Empty, ItemSet.SagExportDirectory);
			AssertEquals("Default ExportSagAccountsNotifyGroup", true, ItemSet.SagExportNotifyGroup.IsEmpty);
			ItemSet.SagDataTransferSwitchRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestHelper.GetValidDataTransferSwitchRegistryBusinessObject());
			AssertEquals("ExportSagAccountsDirectory", Env.TempPath, ItemSet.SagExportDirectory);
			AssertEquals("ExportSagAccountsNotifyGroup", false, ItemSet.SagExportNotifyGroup.IsEmpty);
		}

		public void TestDataExportRegistryItemForCompany()
		{
			TestGenericRegistryItem(ItemSet.SagDataTransferSwitchRegistryItem, "ELGDataTransferSwitchRegistryItem", "ELG Client Extensions/Sage Accounts Interface", "Data Export Settings", "Please fill in all the fields provided below.", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached);
			TestHelper.SetRegistry(Env.TempPath);
			GlbBranch[] branches = GlbBranch.GetOneActiveBranchPerCompany();
			foreach (GlbBranch branch in branches)
			{
				using (branch.SetAsTemporaryContext())
				{
					AssertEquals("The directory should follow the one registered for system", Env.TempPath, ELGDataRegistry.Instance.SagExportDirectory);
				}
			}

			foreach (GlbBranch branch in branches)
			{
				using (branch.SetAsTemporaryContext())
				{
					TestHelper.SetExportDirectoryForCompany(branch.Company.PK.ToGuid(), Path.Combine(Env.TempPath, branch.Company.GC_Code));
				}
			}

			foreach (GlbBranch branch in branches)
			{
				using (branch.SetAsTemporaryContext())
				{
					AssertEquals("After directory registered for company, it should use the company registered directory instead of system registered directory.", Path.Combine(Env.TempPath, branch.Company.GC_Code), ELGDataRegistry.Instance.SagExportDirectory);
				}
			}
		}

		ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper());
			}
		}

		ELGTestHelper testHelper;
	}
}
