using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AWH.Testing
{
	[TestedType(typeof(AWHDataRegistry))]
	public class AWHDataRegistryTest : RegistryItemSetTestCase<AWHDataRegistry>
	{
#region TestUserVisibleRegistryItems
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Count", 4, AllItems.Count);
			AssertVisible(ItemSet.fARTransOutputDirectory);
			AssertVisible(ItemSet.fDepartmentList);
			AssertVisible(ItemSet.fBranchList);
			AssertVisible(ItemSet.fExportFilePrefix);
		}

#endregion
		public void TestAWHExportDirectory()
		{
			ItemSet.ARTransExportDirectory = "ExportDirectory";
			AssertEquals("ExportDirectory", ItemSet.ARTransExportDirectory);
		}

		public void TestExportFilePrefix()
		{
			ItemSet.ExportFilePrefix = "PreFix";
			AssertEquals("PreFix", ItemSet.ExportFilePrefix);
		}

		public void TestBranchList()
		{
			GlbBranchDependentCollection branches = GlbCompany.CurrentCompany.Branches;
			ReadOnlyCodeDescriptionPairList defaultBranchList = ItemSet.BranchList;
			AssertEquals("No of branches in default list", defaultBranchList.Count, branches.Count);
			foreach (GlbBranch brh in branches)
			{
				AssertEquals("Default list should have branch " + brh.GB_Code, true, defaultBranchList.ContainsCode(brh.GB_Code));
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "123");
			ItemSet.fBranchList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("should be 1 pair", 1, ItemSet.BranchList.Count);
			AssertEquals("Code should be 'ABC'", "ABC", ItemSet.BranchList[0].Code);
			AssertEquals("Description should be '123'", "123", ItemSet.BranchList[0].Description);
		}

		public void TestDepartmentList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "123");
			ItemSet.fDepartmentList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("should be 1 pair", 1, ItemSet.DepartmentList.Count);
			AssertEquals("Code should be 'ABC'", "ABC", ItemSet.DepartmentList[0].Code);
			AssertEquals("Description should be '123'", "123", ItemSet.DepartmentList[0].Description);
			AssertEquals("Registry should be non-translatable", true, !ItemSet.fDepartmentList.IsTranslatable);
		}
	}
}
