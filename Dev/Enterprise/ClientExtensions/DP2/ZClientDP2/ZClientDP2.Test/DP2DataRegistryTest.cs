using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DP2.Testing
{
	[TestedType(typeof(DP2DataRegistry))]
	public class DP2DataRegistryTest : RegistryItemSetTestCase<DP2DataRegistry>
	{
#region TestUserVisibleRegistryItems
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Count", 5, AllItems.Count);
			AssertVisible(ItemSet.ARTransExportDirectoryItem);
			AssertVisible(ItemSet.DepartmentListItem);
			AssertVisible(ItemSet.BranchListItem);
			AssertVisible(ItemSet.ExportFilePrefixItem);
			AssertVisible(ItemSet.EnableARTransactionExportItem);
		}

#endregion
		public void TestDP2ExportDirectory()
		{
			ItemSet.ARTransExportDirectory = "ExportDirectory";
			AssertEquals("ExportDirectory", ItemSet.ARTransExportDirectory);
			AssertEquals(DP2DataRegistry.ARTransExportSubCategory, ItemSet.ARTransExportDirectoryItem.Category);
		}

		public void TestExportFilePrefix()
		{
			ItemSet.ExportFilePrefix = "PreFix";
			AssertEquals("PreFix", ItemSet.ExportFilePrefix);
			AssertEquals(DP2DataRegistry.ARTransExportSubCategory, ItemSet.ExportFilePrefixItem.Category);
		}

		public void TestBranchList()
		{
			var activeBranches = GlbCompany.GetActiveCompanies().SelectMany(coy => coy.ActiveBranches).ToArray();
			ReadOnlyCodeDescriptionPairList defaultBranchList = ItemSet.BranchList;
			AssertEquals("No of branches in default list", defaultBranchList.Count, activeBranches.Length);
			foreach (GlbBranch brh in activeBranches)
			{
				AssertEquals("Default list should have branch " + brh.GB_Code, true, defaultBranchList.ContainsCode(brh.GB_Code));
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "123");
			ItemSet.BranchListItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("should be 1 pair", 1, ItemSet.BranchList.Count);
			AssertEquals("Code should be 'ABC'", "ABC", ItemSet.BranchList[0].Code);
			AssertEquals("Description should be '123'", "123", ItemSet.BranchList[0].Description);
			AssertEquals(DP2DataRegistry.ARTransExportSubCategory, ItemSet.BranchListItem.Category);
		}

		public void TestDepartmentList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "123");
			ItemSet.DepartmentListItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("should be 1 pair", 1, ItemSet.DepartmentList.Count);
			AssertEquals("Code should be 'ABC'", "ABC", ItemSet.DepartmentList[0].Code);
			AssertEquals("Description should be '123'", "123", ItemSet.DepartmentList[0].Description);
			AssertEquals("Registry should be non-translatable", true, !ItemSet.DepartmentListItem.IsTranslatable);
			AssertEquals(DP2DataRegistry.ARTransExportSubCategory, ItemSet.DepartmentListItem.Category);
		}

		public void TestEnableARTransactionExport()
		{
			AssertEquals("DP2EnableARTransactionExport", ItemSet.EnableARTransactionExportItem.Name);
			AssertEquals("Enable AR Transaction Export", ItemSet.EnableARTransactionExportItem.Caption);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableARTransactionExportItem.Storage);
			AssertEquals("DefaultValue", false, ItemSet.EnableARTransactionExportItem.DefaultValue);
			AssertEquals(DP2DataRegistry.ARTransExportSubCategory, ItemSet.EnableARTransactionExportItem.Category);
		}
	}
}
