using System;
using System.Reflection;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class RegistryItemFallBackValueAccessorTest : TransactionedTestCase
	{
		#region Merged Actual Values Registry Item

		public class MergeValuesTestRegistryItem : StringRegistryItem
		{
			public MergeValuesTestRegistryItem(string name, RegistryStorageFlags storage, string defaultValue)
				: base(new RegistryItemImpl(name, (NoResString)"test", (NoResString)"test", (NoResString)"test", new TestMergeValuesRegistryDataType(), storage, defaultValue))
			{
			}
		}

		class TestMergeValuesRegistryDataType : StringRegistryDataType
		{
			public TestMergeValuesRegistryDataType()
			{
			}

			protected override bool IsFallBackMergeActualValuesImplementedCore
			{
				get { return true; }
			}

			protected override string FallBackMergeValuesCore(string fallBackValue, string value)
			{
				return value + "|" + fallBackValue;
			}
		}

		#endregion

		public void TestGetCurrentValue_MergeActualValue()
		{
			StringRegistryItem nonMergedItem = new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company, "Default");
			nonMergedItem.Options = RegistryOptions.NotCached;

			MergeValuesTestRegistryItem mergedItem = new MergeValuesTestRegistryItem("Name", RegistryStorageFlags.System | RegistryStorageFlags.Company, "Default");
			mergedItem.Options = RegistryOptions.NotCached;

			nonMergedItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Overidden");
			mergedItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Overidden");

			AssertEquals("Overidden", nonMergedItem.Value);
			AssertEquals("Default|Overidden", mergedItem.Value);
		}

		public void TestValidateNoCombinationsOfLevelsInConstructor()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);
			try
			{
				object x = GetFallbackValueAccessor(item, RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expected an exception due to invalid argument");
			}
			catch (ArgumentException) { }

			try
			{
				object x = GetFallbackValueAccessor(item, RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.System, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expected an exception due to invalid argument");
			}
			catch (ArgumentException) { }

			try
			{
				object x = GetFallbackValueAccessor(item, RegistryStorageFlags.Branch | RegistryStorageFlags.SystemDepartment, Guid.Empty, Guid.Empty, Guid.Empty);
				Fail("Expected an exception due to invalid argument");
			}
			catch (ArgumentException) { }

			Assert(true);
		}

		public void TestFallBackParent()
		{
			PropertyInfo propertyInfo = typeof(RegistryItemFallBackValueAccessor).GetProperty("ValueAtThisLevel", BindingFlags.NonPublic | BindingFlags.Instance);

			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.System);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(
				item, RegistryStorageFlags.BranchDepartment, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			AssertEquals("branch department", propertyInfo.GetValue(retriever, null));

			RegistryItemFallBackValueAccessor parentRetriever = retriever.FallBackParent;
			AssertEquals("system", propertyInfo.GetValue(parentRetriever, null));
		}

		public void TestFallBackMerge()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);
			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(
				item, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			AssertEquals("branch department|branch|company department|company|system department|system", retriever.GetFallBackValue().Value);
		}

		public void TestNonDepartmentLevelsDoNotFallBackToDepartments()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "10");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "20");

			RegistryItemFallBackValueAccessor companyRetriever = GetFallbackValueAccessor(
				item, RegistryStorageFlags.Company, EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			RegistryItemFallBackValueAccessor companyDepartmentRetriever = GetFallbackValueAccessor(
				item, RegistryStorageFlags.CompanyDepartment, EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK);

			RegistryItemFallBackValueAccessor branchRetriever = GetFallbackValueAccessor(
				item, RegistryStorageFlags.Branch, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
			RegistryItemFallBackValueAccessor branchDepartmentRetriever = GetFallbackValueAccessor(
				item, RegistryStorageFlags.BranchDepartment, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals("This should fall back to System level", RegistryStorageFlags.System, companyRetriever.GetDefaultNewValue().Level);
			AssertEquals("This should fall back to System Department level", RegistryStorageFlags.SystemDepartment, companyDepartmentRetriever.GetDefaultNewValue().Level);
			AssertEquals("This should fall back to System level", RegistryStorageFlags.System, branchRetriever.GetDefaultNewValue().Level);
			AssertEquals("This should fall back to System Department level", RegistryStorageFlags.SystemDepartment, branchDepartmentRetriever.GetDefaultNewValue().Level);
		}

		public void TestGetDefaultNewValueEmptyLevel()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);
			RegistryItemFallBackValueAccessor retriever = new RegistryItemFallBackValueAccessor(item, RegistryStorageFlags.Company,
				EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals("GetFallBackValue().Level", new RegistryStorageFlags(), retriever.GetFallBackValue().Level);
			AssertEquals("GetDefaultNewValue().Level", RegistryStorageFlags.All, retriever.GetDefaultNewValue().Level);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			AssertEquals("GetFallBackValue().Level", RegistryStorageFlags.System, retriever.GetFallBackValue().Level);
			AssertEquals("GetDefaultNewValue().Level", RegistryStorageFlags.System, retriever.GetDefaultNewValue().Level);
		}

		#region Get Fallback Value Merged Level

		public void TestGetFallBackValueMergedLevelNoDefaultValue()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);

			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.System, false);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.SystemDepartment, false);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.Company, false);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.CompanyDepartment, false);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.Branch, false);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.BranchDepartment, false);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System,
				"system", item, RegistryStorageFlags.System, false);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment,
				"system department|system", item, RegistryStorageFlags.SystemDepartment, false);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company,
				"company|system department|system", item, RegistryStorageFlags.Company, false);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment, "company department|company|system department|system", item, RegistryStorageFlags.CompanyDepartment, false);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch,
				"branch|company department|company|system department|system", item, RegistryStorageFlags.Branch, false);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				"branch department|branch|company department|company|system department|system", item, RegistryStorageFlags.BranchDepartment, false);
		}

		public void TestGetFallBackValueMergedLevelWithDefaultValue()
		{
			TestRegistryDataType dataType = new TestRegistryDataType();

			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, "default");
			item.DataType = dataType;

			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.System, true);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.SystemDepartment, true);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.Company, true);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.CompanyDepartment, true);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.Branch, true);
			TestGetFallBackValueMergedLevel(new RegistryStorageFlags(), "default", item, RegistryStorageFlags.BranchDepartment, true);

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System,
				"system", item, RegistryStorageFlags.System, true);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment,
				"system department|system", item, RegistryStorageFlags.SystemDepartment, true);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company,
				"company|system department|system", item, RegistryStorageFlags.Company, true);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment, "company department|company|system department|system", item, RegistryStorageFlags.CompanyDepartment, true);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch,
				"branch|company department|company|system department|system", item, RegistryStorageFlags.Branch, true);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				"branch department|branch|company department|company|system department|system", item, RegistryStorageFlags.BranchDepartment, true);

			((IRegistryItemInternals)item).DeleteValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			TestGetFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				"branch department|branch|company department|system department|system", item, RegistryStorageFlags.BranchDepartment, true);
		}

		void TestGetFallBackValueMergedLevel(RegistryStorageFlags expectedLevel, object expectedValue,
			IRegistryItem item, RegistryStorageFlags currentLevel, bool isMergedWithDefaultValue)
		{
			TestGetFallBackValueMergedLevel(expectedLevel, expectedValue, item, currentLevel, isMergedWithDefaultValue,
				EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		void TestGetFallBackValueMergedLevel(RegistryStorageFlags expectedLevel, object expectedValue,
			IRegistryItem item, RegistryStorageFlags currentLevel, bool isMergedWithDefaultValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			RegistryItemFallBackValueAccessor retriever = new RegistryItemFallBackValueAccessor(item, currentLevel,
				companyPK, branchPK, departmentPK);
			FallbackValue fallBackValue = retriever.GetFallBackValue();
			AssertEquals("CurrentValue.Level", expectedLevel, fallBackValue.Level);
			AssertEquals("CurrentValue.Value", expectedValue, fallBackValue.Value);
		}

		#endregion

		#region Get Fallback Value at All Levels

		public virtual void TestGetFallBackValueAtAllLevels()
		{
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.BranchDepartment, "branch department");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.Branch, "branch");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.CompanyDepartment, "company department");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.Company, "company");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.SystemDepartment, "system department");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.All, RegistryStorageFlags.System, "system");

			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.BranchDepartment, "system");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.Branch, "system");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.CompanyDepartment, "system");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.Company, "system");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.SystemDepartment, "system");
			TestGetFallBackValueAtAllLevels(RegistryStorageFlags.System, RegistryStorageFlags.System, "system");
		}

		void TestGetFallBackValueAtAllLevels(RegistryStorageFlags storage, RegistryStorageFlags level, string expectedValue)
		{
			StringRegistryItem item = new StringRegistryItem("name" + UniqueNameIndex++, (NoResString)"category", (NoResString)"caption", (NoResString)"hint", storage);

			if ((storage & RegistryStorageFlags.BranchDepartment) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			}

			if ((storage & RegistryStorageFlags.Branch) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			}

			if ((storage & RegistryStorageFlags.CompanyDepartment) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			}

			if ((storage & RegistryStorageFlags.Company) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			}

			if ((storage & RegistryStorageFlags.SystemDepartment) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			}

			if ((storage & RegistryStorageFlags.System) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");
			}

			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(
				item, level, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			AssertEquals(
				"Should retrieve appropriate value at fallback level " + level + " when stored at " + storage,
				expectedValue, retriever.GetFallBackValue().Value);
		}

		static int UniqueNameIndex;

		#endregion

		#region Get Default New Value

		public void TestGetDefaultNewValue()
		{
			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.BranchDepartment, "branch", "Should fallback to next level");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.Branch, "company", "Should get value from the company as the company department may be ambiguous");

			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.CompanyDepartment, "company", "Should fallback to next level");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.Company, "system", "Should get value from the system as the system department may be ambiguous");

			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.SystemDepartment, "system", "Should fallback to next level");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All, RegistryStorageFlags.System, "datatype default", "Should get value from the data type as there is no where else to fallback now");

			TestGetDefaultNewValue(
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.BranchDepartment,
				"system",
				"Should fallback to next level");
			TestGetDefaultNewValue(
				RegistryStorageFlags.CompanyDepartment,
				RegistryStorageFlags.Branch,
				"datatype default",
				"Should default to data type default as the department may be ambiguous and there is no where else to fallback to");
			TestGetDefaultNewValue(
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.BranchDepartment,
				"system department",
				"Should fallback to system department");

			TestGetDefaultNewValue(
				RegistryStorageFlags.All,
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.CompanyDepartment,
				RegistryStorageFlags.BranchDepartment,
				"company department",
				"Should fallback to company department");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All,
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.SystemDepartment,
				RegistryStorageFlags.BranchDepartment,
				"system department",
				"Should fallback to system department");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All,
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.BranchDepartment,
				"system",
				"Should fallback to system");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All,
				RegistryStorageFlags.BranchDepartment,
				RegistryStorageFlags.BranchDepartment,
				"datatype default",
				"Nothing to fallback to so use datatype default");
			TestGetDefaultNewValue(
				RegistryStorageFlags.All,
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.BranchDepartment,
				"system",
				"Should fallback to system");
		}

		public void TestGetDefaultNewValueUsesCustomDefaultValue()
		{
			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			DummyStringRegistryItem item = new DummyStringRegistryItem("", "", "", "", RegistryStorageFlags.All);

			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(item, RegistryStorageFlags.System, companyPK, branchPK, departmentPK);
			FallbackValue value = retriever.GetDefaultNewValue();

			AssertEquals("IsMergedWithDefaultValue", false, value.IsMergedWithDefaultValue);
			AssertEquals("Value", companyPK.ToString() + branchPK.ToString() + departmentPK.ToString(), value.Value);
			AssertEquals("Level", RegistryStorageFlags.All, value.Level);

			item.SetValue(Guid.Empty, branchPK, departmentPK, "branch department");
			retriever = GetFallbackValueAccessor(item, RegistryStorageFlags.BranchDepartment, companyPK, branchPK, departmentPK);
			value = retriever.GetDefaultNewValue();

			AssertEquals("IsMergedWithDefaultValue", false, value.IsMergedWithDefaultValue);
			AssertEquals("Value", companyPK.ToString() + branchPK.ToString() + departmentPK.ToString(), value.Value);
			AssertEquals("Level", RegistryStorageFlags.All, value.Level);
		}

		void TestGetDefaultNewValue(RegistryStorageFlags storage, RegistryStorageFlags level, string expectedValue, string message)
		{
			TestGetDefaultNewValue(storage, storage, level, expectedValue, message);
		}

		void TestGetDefaultNewValue(RegistryStorageFlags storage, RegistryStorageFlags levelsToSetValuesOn, RegistryStorageFlags level, string expectedValue, string message)
		{
			StringRegistryItem item = new StringRegistryItem("name" + UniqueNameIndex++, (NoResString)"category", (NoResString)"caption", (NoResString)"hint", storage, "datatype default");

			if ((levelsToSetValuesOn & RegistryStorageFlags.BranchDepartment) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			}

			if ((levelsToSetValuesOn & RegistryStorageFlags.Branch) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			}

			if ((levelsToSetValuesOn & RegistryStorageFlags.CompanyDepartment) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			}

			if ((levelsToSetValuesOn & RegistryStorageFlags.Company) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			}

			if ((levelsToSetValuesOn & RegistryStorageFlags.SystemDepartment) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			}

			if ((levelsToSetValuesOn & RegistryStorageFlags.System) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");
			}

			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(
				item, level, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			AssertEquals(
				message, expectedValue, retriever.GetDefaultNewValue().Value);
		}

		#endregion

		#region Is Default Value for this Level Ambiguous

		public void TestIsDefaultValueForThisLevelAmbiguous()
		{
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.BranchDepartment, false, "Can get value from branch");
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.Branch, true, "A company department has value");

			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.CompanyDepartment, false, "Can get value from company");
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.Company, true, "A system department has value");

			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.SystemDepartment, false, "Can get value from system");
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.All, RegistryStorageFlags.System, false, "Can get default datatype value as there is nowhere else to fallback on");

			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.BranchDepartment,
				false,
				"Can fallback to system");
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.Branch | RegistryStorageFlags.SystemDepartment,
				RegistryStorageFlags.BranchDepartment,
				false,
				"Should default to system department");
			TestIsDefaultValueForThisLevelAmbiguous(
				RegistryStorageFlags.Branch | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.System,
				RegistryStorageFlags.Branch,
				true,
				"Could be an ambiguous match on which system department");
		}

		void TestIsDefaultValueForThisLevelAmbiguous(RegistryStorageFlags storage, RegistryStorageFlags level, bool expectedValue, string message)
		{
			StringRegistryItem item = new StringRegistryItem("name" + UniqueNameIndex++, (NoResString)"category", (NoResString)"caption", (NoResString)"hint", storage, "datatype default");

			if ((storage & RegistryStorageFlags.BranchDepartment) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			}

			if ((storage & RegistryStorageFlags.Branch) != 0)
			{
				item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			}

			if ((storage & RegistryStorageFlags.CompanyDepartment) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			}

			if ((storage & RegistryStorageFlags.Company) != 0)
			{
				item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			}

			if ((storage & RegistryStorageFlags.SystemDepartment) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			}

			if ((storage & RegistryStorageFlags.System) != 0)
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");
			}

			RegistryItemFallBackValueAccessor retriever = GetFallbackValueAccessor(
				item, level, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			AssertEquals(
				message, expectedValue, retriever.IsDefaultValueForThisLevelAmbiguous());
		}

		#endregion

		#region Implementation

		protected virtual RegistryItemFallBackValueAccessor GetFallbackValueAccessor
			(IRegistryItem item, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new RegistryItemFallBackValueAccessor(item, companyPK, branchPK, departmentPK);
		}

		protected virtual RegistryItemFallBackValueAccessor GetFallbackValueAccessor
			(IRegistryItem item, RegistryStorageFlags level, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new RegistryItemFallBackValueAccessor(item, level, companyPK, branchPK, departmentPK);
		}

		#region class DummyStringRegistryItem

		protected class DummyStringRegistryItem : StringRegistryItem
		{
			public DummyStringRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
				: base(new DummyRegistryItemImpl(name, category, caption, hint, new TestRegistryDataType(), storage))
			{
			}

			public DummyStringRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage, object defaultValue)
				: base(new DummyRegistryItemImpl(name, category, caption, hint, new TestRegistryDataType(), storage, defaultValue))
			{
			}
		}

		#endregion

		#region class TestRegistryDataType

		protected class TestRegistryDataType : StringRegistryDataType
		{
			protected override bool IsFallBackMergeValuesImplementedCore
			{
				get { return true; }
			}

			protected override string FallBackMergeValuesCore(string fallBackValue, string value)
			{
				return fallBackValue + "|" + value;
			}
		}

		#endregion

		#endregion
	}
}
