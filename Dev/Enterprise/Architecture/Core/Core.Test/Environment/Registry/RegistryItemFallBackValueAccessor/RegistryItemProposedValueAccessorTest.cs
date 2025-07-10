using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemProposedValueAccessorTest : RegistryItemFallBackValueAccessorTest
	{
		// Test does not apply to this class
		public override void TestGetFallBackValueAtAllLevels()
		{
			Assert(true);
		}

		public void TestGetCurrentValueWithoutFallback()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, "");
			IRegistryItemInternals itemInternals = item;
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, RegistryStorageFlags.Company,
				EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			try
			{
				object x = retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				Fail("Expecting an exception because either CompanyPK or BranchPK must be Guid.Empty");
			}
			catch (OdysseyException)
			{
			}

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Enterprise");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "Enterprise Department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Company");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "Company Department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Branch");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Branch Department");

			AssertEquals("GetCurrentValueWithoutFallback()", "Enterprise", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "Enterprise Department", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "Company", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "Company Department", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "Branch", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "Branch Department", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Enterprise");
			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "New Enterprise Department");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "New Company");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "New Company Department");
			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "New Branch");
			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "New Branch Department");

			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			AssertEquals("GetCurrentValueWithoutFallback()", "New Enterprise", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "New Enterprise Department", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "New Company", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "New Company Department", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "New Branch", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "New Branch Department", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));

			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);

			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty));
			AssertEquals("GetCurrentValueWithoutFallback()", "", retriever.GetCurrentValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestGetCurrentValueWithoutFallbackUsesCustomDefaultValue()
		{
			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			DummyStringRegistryItem item = new DummyStringRegistryItem("", "", "", "", RegistryStorageFlags.CompanyDepartment);

			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, RegistryStorageFlags.CompanyDepartment, companyPK, branchPK, departmentPK);
			AssertEquals("GetCurrentValueWithoutFallback", companyPK.ToString() + Guid.Empty.ToString() + departmentPK.ToString(),
				retriever.GetCurrentValueWithoutFallback(companyPK, Guid.Empty, departmentPK));
		}

		public void TestGetCurrentValueEmptyLevel()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, RegistryStorageFlags.Company,
				EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals("GetFallBackValue().Level", new RegistryStorageFlags(), retriever.GetFallBackValue().Level);
			AssertEquals("GetCurrentValue().Level", RegistryStorageFlags.All, retriever.GetCurrentValue().Level);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			AssertEquals("GetFallBackValue().Level", RegistryStorageFlags.System, retriever.GetFallBackValue().Level);
			AssertEquals("GetDefaultNewValue().Level", RegistryStorageFlags.System, retriever.GetCurrentValue().Level);
		}

		public void TestGetDefaultNewValueIsObtainedFromProposedValues()
		{
			StringRegistryItem item = new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All);
			IRegistryItemInternals itemInternals = item;

			#region Test #1

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemSavedValue");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CurrentCompanySavedValue");

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #2

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #3

			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CurrentCompanyProposedValue");
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			item.SetValue(TestCompany.PK, Guid.Empty, Guid.Empty, "TestCompanySavedValue");

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #4

			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "CurrentBranchProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.ProposedValue);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #5

			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #6

			item.SetValue(Guid.Empty, TestBranch.PK, Guid.Empty, "TestBranchSavedValue");

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #7

			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "CurrentCompanyCurrentDepartmentProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #8

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, TestDepartment.PK, "SystemTestDepartmentProposedValue");
			itemInternals.SetProposedValue(Guid.Empty, TestBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "TestBranchCurrentDepartmentProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, TestDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(TestCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, TestBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #9

			itemInternals.ClearCurrentValueToUseCache();

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.All, "", item, FallbackToTest.System);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.CurrentCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompany);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion
		}

		public void TestGetCurrentValue()
		{
			StringRegistryItem item = new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All);
			IRegistryItemInternals itemInternals = item;

			#region Test #1

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemSavedValue");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CurrentCompanySavedValue");

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #2

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "SystemProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #3

			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CurrentCompanyProposedValue");
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			item.SetValue(TestCompany.PK, Guid.Empty, Guid.Empty, "TestCompanySavedValue");

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #4

			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "CurrentBranchProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.ProposedValue);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanyProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #5

			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #6

			item.SetValue(Guid.Empty, TestBranch.PK, Guid.Empty, "TestBranchSavedValue");

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "CurrentBranchProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #7

			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "CurrentCompanyCurrentDepartmentProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #8

			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, TestDepartment.PK, "SystemTestDepartmentProposedValue");
			itemInternals.SetProposedValue(Guid.Empty, TestBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "TestBranchCurrentDepartmentProposedValue");
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, TestDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(TestCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, TestBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.CompanyDepartment, "CurrentCompanyCurrentDepartmentProposedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.BranchDepartment, "TestBranchCurrentDepartmentProposedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemProposedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.SystemDepartment, "SystemTestDepartmentProposedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion

			#region Test #9

			itemInternals.ClearCurrentValueToUseCache();

			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.System);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.System, "SystemSavedValue", item, FallbackToTest.SystemTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompany);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestCompanyTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranch);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "CurrentCompanySavedValue", item, FallbackToTest.CurrentBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranch);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchCurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Branch, "TestBranchSavedValue", item, FallbackToTest.TestBranchTestDepartment);

			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2CurrentDepartment);
			TestGetCurrentValue(RegistryStorageFlags.Company, "TestCompanySavedValue", item, FallbackToTest.TestBranch2TestDepartment);

			#endregion
		}

		public void TestGetProposedFallBackValueMergedLevel()
		{
			StringRegistryItem item = new StringRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new TestRegistryDataType(), RegistryStorageFlags.All);
			IRegistryItemInternals itemInternals = item;

			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "branch department");
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "branch");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "company department");
			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "company");
			item.SetValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "system department");
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "system");

			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "new branch department");
			itemInternals.SetProposedValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "new branch");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "new company department");
			itemInternals.SetProposedValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "new company");
			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, "new system department");
			itemInternals.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "new system");

			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.ProposedValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System,
				"new system", item, RegistryStorageFlags.System);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment,
				"new system department|new system", item, RegistryStorageFlags.SystemDepartment);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company,
				"new company|new system department|new system", item, RegistryStorageFlags.Company);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment, "new company department|new company|new system department|new system", item,
				RegistryStorageFlags.CompanyDepartment);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch,
				"new branch|new company department|new company|new system department|new system", item, RegistryStorageFlags.Branch);

			TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company |
				RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				"new branch department|new branch|new company department|new company|new system department|new system", item,
				RegistryStorageFlags.BranchDepartment);

			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, ValueToUse.DefaultValue);
			itemInternals.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.DefaultValue);

			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.System);
			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.SystemDepartment);
			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.Company);
			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.CompanyDepartment);
			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.Branch);
			TestGetProposedFallBackValueMergedLevel(new RegistryStorageFlags(), "", item, RegistryStorageFlags.BranchDepartment);
		}

		#region Implementation

		protected override RegistryItemFallBackValueAccessor GetFallbackValueAccessor(IRegistryItem item, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new RegistryItemProposedValueAccessor(item, RegistryStorageFlags.BranchDepartment, companyPK, branchPK, departmentPK);
		}

		protected override RegistryItemFallBackValueAccessor GetFallbackValueAccessor(IRegistryItem item, RegistryStorageFlags level, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new RegistryItemProposedValueAccessor(item, level, companyPK, branchPK, departmentPK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();

			var company = factory.New(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_RX_NKLocalCurrency] = "XXX";
			company[GlbCompanySchema.GC_RN_NKCountryCode] = "XX";
			TestCompany = (ICompany)company;

			var branch = factory.New(ObjectFactory.GetType("IGlbBranch"));
			branch[GlbBranchSchema.GB_GC.Name] = TestCompany.PK;
			TestBranch = (IBranch)branch;

			var company2 = factory.New(ObjectFactory.GetType("IGlbCompany"));
			company2[GlbCompanySchema.GC_RX_NKLocalCurrency] = "XXX";
			company2[GlbCompanySchema.GC_RN_NKCountryCode] = "XX";

			var branch2 = factory.New(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_GC.Name] = TestCompany.PK;
			TestBranch2 = (IBranch)branch2;

			var department = factory.New(ObjectFactory.GetType("IGlbDepartment"));
			TestDepartment = (IDepartment)department;
		}

		enum FallbackToTest
		{
			System,
			SystemCurrentDepartment,
			SystemTestDepartment,

			CurrentCompany,
			CurrentCompanyCurrentDepartment,
			CurrentCompanyTestDepartment,

			TestCompany,
			TestCompanyCurrentDepartment,
			TestCompanyTestDepartment,

			CurrentBranch,
			CurrentBranchCurrentDepartment,
			CurrentBranchTestDepartment,

			TestBranch,
			TestBranchCurrentDepartment,
			TestBranchTestDepartment,

			TestBranch2,
			TestBranch2CurrentDepartment,
			TestBranch2TestDepartment
		}

		void TestGetDefaultNewValueIsObtainedFromProposedValues(RegistryStorageFlags expectedLevel, object expectedValue,
			IRegistryItem item, FallbackToTest currentFallback)
		{
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, GetCurrentLevel(currentFallback),
				GetCompanyPK(currentFallback), GetBranchPK(currentFallback), GetDepartmentPK(currentFallback));
			FallbackValue defaultValue = retriever.GetDefaultNewValue();
			AssertEquals("DefaultValue.Level", expectedLevel, defaultValue.Level);
			AssertEquals("DefaultValue.Value", expectedValue, defaultValue.Value);
		}

		void TestGetCurrentValue(RegistryStorageFlags expectedLevel, object expectedValue,
			IRegistryItem item, FallbackToTest currentFallback)
		{
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, GetCurrentLevel(currentFallback),
				GetCompanyPK(currentFallback), GetBranchPK(currentFallback), GetDepartmentPK(currentFallback));
			FallbackValue currentValue = retriever.GetCurrentValue();
			AssertEquals("CurrentValue.Level", expectedLevel, currentValue.Level);
			AssertEquals("CurrentValue.Value", expectedValue, currentValue.Value);
		}

		void TestGetProposedFallBackValueMergedLevel(RegistryStorageFlags expectedLevel, object expectedValue,
			IRegistryItem item, RegistryStorageFlags currentLevel)
		{
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(item, currentLevel,
				EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			FallbackValue fallBackValue = retriever.GetFallBackValue();
			AssertEquals("CurrentValue.Level", expectedLevel, fallBackValue.Level);
			AssertEquals("CurrentValue.Value", expectedValue, fallBackValue.Value);
		}

		RegistryStorageFlags GetCurrentLevel(FallbackToTest currentFallback)
		{
			switch (currentFallback)
			{
				case (FallbackToTest.System):
					return RegistryStorageFlags.System;
				case (FallbackToTest.SystemCurrentDepartment):
					return RegistryStorageFlags.SystemDepartment;
				case (FallbackToTest.SystemTestDepartment):
					return RegistryStorageFlags.SystemDepartment;

				case (FallbackToTest.CurrentCompany):
					return RegistryStorageFlags.Company;
				case (FallbackToTest.CurrentCompanyCurrentDepartment):
					return RegistryStorageFlags.CompanyDepartment;
				case (FallbackToTest.CurrentCompanyTestDepartment):
					return RegistryStorageFlags.CompanyDepartment;

				case (FallbackToTest.TestCompany):
					return RegistryStorageFlags.Company;
				case (FallbackToTest.TestCompanyCurrentDepartment):
					return RegistryStorageFlags.CompanyDepartment;
				case (FallbackToTest.TestCompanyTestDepartment):
					return RegistryStorageFlags.CompanyDepartment;

				case (FallbackToTest.CurrentBranch):
					return RegistryStorageFlags.Branch;
				case (FallbackToTest.CurrentBranchCurrentDepartment):
					return RegistryStorageFlags.BranchDepartment;
				case (FallbackToTest.CurrentBranchTestDepartment):
					return RegistryStorageFlags.BranchDepartment;

				case (FallbackToTest.TestBranch):
					return RegistryStorageFlags.Branch;
				case (FallbackToTest.TestBranchCurrentDepartment):
					return RegistryStorageFlags.BranchDepartment;
				case (FallbackToTest.TestBranchTestDepartment):
					return RegistryStorageFlags.BranchDepartment;

				case (FallbackToTest.TestBranch2):
					return RegistryStorageFlags.Branch;
				case (FallbackToTest.TestBranch2CurrentDepartment):
					return RegistryStorageFlags.BranchDepartment;
				case (FallbackToTest.TestBranch2TestDepartment):
					return RegistryStorageFlags.BranchDepartment;

				default:
					return RegistryStorageFlags.System;
			}
		}

		Guid GetCompanyPK(FallbackToTest currentFallback)
		{
			if (currentFallback == FallbackToTest.System ||
				currentFallback == FallbackToTest.SystemCurrentDepartment ||
				currentFallback == FallbackToTest.SystemTestDepartment)
			{
				return Guid.Empty;
			}
			else if (currentFallback == FallbackToTest.CurrentCompany ||
				currentFallback == FallbackToTest.CurrentCompanyCurrentDepartment ||
				currentFallback == FallbackToTest.CurrentCompanyTestDepartment ||
				currentFallback == FallbackToTest.CurrentBranch ||
				currentFallback == FallbackToTest.CurrentBranchCurrentDepartment ||
				currentFallback == FallbackToTest.CurrentBranchTestDepartment)
			{
				return EnvProxy.Instance.CurrentCompany.PK;
			}
			else
			{
				return TestCompany.PK;
			}
		}

		Guid GetBranchPK(FallbackToTest currentFallback)
		{
			if (currentFallback == FallbackToTest.CurrentBranch ||
				currentFallback == FallbackToTest.CurrentBranchCurrentDepartment ||
				currentFallback == FallbackToTest.CurrentBranchTestDepartment)
			{
				return EnvProxy.Instance.CurrentBranch.PK;
			}
			else if (currentFallback == FallbackToTest.TestBranch ||
				currentFallback == FallbackToTest.TestBranchCurrentDepartment ||
				currentFallback == FallbackToTest.TestBranchTestDepartment)
			{
				return TestBranch.PK;
			}
			else if (currentFallback == FallbackToTest.TestBranch2 ||
				currentFallback == FallbackToTest.TestBranch2CurrentDepartment ||
				currentFallback == FallbackToTest.TestBranch2TestDepartment)
			{
				return TestBranch2.PK;
			}
			else
			{
				return Guid.Empty;
			}
		}

		Guid GetDepartmentPK(FallbackToTest currentFallback)
		{
			if (currentFallback == FallbackToTest.SystemCurrentDepartment ||
				currentFallback == FallbackToTest.CurrentCompanyCurrentDepartment ||
				currentFallback == FallbackToTest.CurrentBranchCurrentDepartment ||
				currentFallback == FallbackToTest.TestCompanyCurrentDepartment ||
				currentFallback == FallbackToTest.TestBranchCurrentDepartment ||
				currentFallback == FallbackToTest.TestBranch2CurrentDepartment)
			{
				return EnvProxy.Instance.CurrentDepartment.PK;
			}
			else if (currentFallback == FallbackToTest.SystemTestDepartment ||
				currentFallback == FallbackToTest.CurrentCompanyTestDepartment ||
				currentFallback == FallbackToTest.CurrentBranchTestDepartment ||
				currentFallback == FallbackToTest.TestCompanyTestDepartment ||
				currentFallback == FallbackToTest.TestBranchTestDepartment ||
				currentFallback == FallbackToTest.TestBranch2TestDepartment)
			{
				return TestDepartment.PK;
			}
			else
			{
				return Guid.Empty;
			}
		}

		ICompany TestCompany;
		IBranch TestBranch;
		IBranch TestBranch2;
		IDepartment TestDepartment;

		#endregion
	}
}
