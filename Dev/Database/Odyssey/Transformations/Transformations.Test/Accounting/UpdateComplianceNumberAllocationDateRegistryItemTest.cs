using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(UpdateComplianceNumberAllocationDateRegistryItem))]
	class UpdateComplianceNumberAllocationDateRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateComplianceNumberAllocationDateRegistryItem();
		}

		protected override void PrepareTestData()
		{
			CreateCompanies();
			InsertRowsInRegistry();
			AssertEquals(5, RegistryHelper.GetStmDataRowCount(ExistingRegistry));
			AssertEquals(4, RegistryHelper.GetStmDataRowCount(NewRegistryForAP));
			AssertEquals(0, RegistryHelper.GetStmDataRowCount(NewRegistryForAR));
		}

		void CreateCompanies()
		{
			CompanyARNot = Guid.NewGuid();
			CompanyARPst = Guid.NewGuid();
			CompanyARInv = Guid.NewGuid();
			CompanyAPNot = Guid.NewGuid();
			CompanyAPPst = Guid.NewGuid();
			CompanyARPstAPPst = Guid.NewGuid();
			CompanyARInvAPPst = Guid.NewGuid();
		}

		void InsertRowsInRegistry()
		{
			//add data in existing registry
			RegistryHelper.InsertStmDataRow(ExistingRegistry, CompanyARNot, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NOT"));
			RegistryHelper.InsertStmDataRow(ExistingRegistry, CompanyARPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("PST"));
			RegistryHelper.InsertStmDataRow(ExistingRegistry, CompanyARInv, Guid.Empty, "STR", Encoding.Unicode.GetBytes("INV"));

			//Add data in AP registry, this registry was only available for support before this transformation
			RegistryHelper.InsertStmDataRow(NewRegistryForAP, CompanyAPNot, Guid.Empty, "STR", Encoding.Unicode.GetBytes("NOT"));
			RegistryHelper.InsertStmDataRow(NewRegistryForAP, CompanyAPPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("PST"));

			//Add conflict data
			RegistryHelper.InsertStmDataRow(ExistingRegistry, CompanyARInvAPPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("INV"));
			RegistryHelper.InsertStmDataRow(ExistingRegistry, CompanyARPstAPPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("PST"));
			RegistryHelper.InsertStmDataRow(NewRegistryForAP, CompanyARPstAPPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("PST"));
			RegistryHelper.InsertStmDataRow(NewRegistryForAP, CompanyARInvAPPst, Guid.Empty, "STR", Encoding.Unicode.GetBytes("PST"));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("old registry rows have been renamed", 0, RegistryHelper.GetStmDataRowCount(ExistingRegistry));
			AssertEquals("New registry row count should be the same as old registry", 5, RegistryHelper.GetStmDataRowCount(NewRegistryForAR));
			AssertEquals(5, RegistryHelper.GetStmDataRowCount(NewRegistryForAP));

			AssertEquals("NOT", GetStringFromStmData(NewRegistryForAR, CompanyARNot));
			AssertEquals("PST", GetStringFromStmData(NewRegistryForAR, CompanyARPst));
			AssertEquals("INV", GetStringFromStmData(NewRegistryForAR, CompanyARInv));
			AssertEquals("INV", GetStringFromStmData(NewRegistryForAR, CompanyARInvAPPst));
			AssertEquals("PST", GetStringFromStmData(NewRegistryForAR, CompanyARPstAPPst));

			//INV value is not possible in new registry. It should be NOT, which is the default registry value so no need to insert row
			//NOT is the default value for this registry so no need to add new row for NOT value
			AssertEquals("PST", GetStringFromStmData(NewRegistryForAP, CompanyARPst));

			//conficts data result
			AssertEquals("PST", GetStringFromStmData(NewRegistryForAP, CompanyARInvAPPst));
			AssertEquals("PST", GetStringFromStmData(NewRegistryForAP, CompanyARPstAPPst));

			string GetStringFromStmData(string registryName, Guid ownerPK) => Encoding.Unicode.GetString(Helper.GetStmDataValue(registryName, ownerPK));
		}

		Guid CompanyARNot;
		Guid CompanyARPst;
		Guid CompanyARInv;
		Guid CompanyAPNot;
		Guid CompanyAPPst;
		Guid CompanyARPstAPPst;
		Guid CompanyARInvAPPst;

		RegistryTransformationHelper RegistryHelper => registryHelper ?? (registryHelper = new RegistryTransformationHelper());
		RegistryTransformationHelper registryHelper;

		const string ExistingRegistry = "ComplianceNumberAllocationDate";
		const string NewRegistryForAP = "ComplianceNumberAllocationDate_AP";
		const string NewRegistryForAR = "ComplianceNumberAllocationDate_AR";
	}
}
