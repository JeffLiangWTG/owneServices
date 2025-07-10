using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Enterprise.DbUpgrader.Transformations;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	class RegistryTransformationHelperTest : TransactionedTestCase
	{
		public void TestGetStmDataRowCount()
		{
			var name = "O'TestGetStmDataRowCount";

			Helper.InsertStmDataRow(name, Guid.Empty, Guid.Empty, "INT", new byte[] { 0x01 });
			AssertEquals("There should be 1 row.", 1, Helper.GetStmDataRowCount(name));

			Helper.InsertStmDataRow(name, Guid.NewGuid(), Guid.Empty, "INT", new byte[] { 0x02 });
			AssertEquals("There should be 2 rows.", 2, Helper.GetStmDataRowCount(name));

			Helper.InsertStmDataRow(name, Guid.Empty, Guid.NewGuid(), "INT", new byte[] { 0x03 });
			AssertEquals("There should be 3 rows.", 3, Helper.GetStmDataRowCount(name));
		}

		public void TestGetStmDataValue()
		{
			var name = "O'TestGetStmDataSystemValue";

			var systemValue = Encoding.Unicode.GetBytes("System Value");
			var systemDepartmentValue = Encoding.Unicode.GetBytes("System Department Value");
			var companyValue = Encoding.Unicode.GetBytes("Company Value");
			var companyDepartmentValue = Encoding.Unicode.GetBytes("Company Department Value");
			var branchValue = Encoding.Unicode.GetBytes("Branch Value");
			var branchDepartmentValue = Encoding.Unicode.GetBytes("Branch Department Value");

			var branchKey = Guid.NewGuid();
			var companyKey = Guid.NewGuid();
			var departmentKey = Guid.NewGuid();

			Helper.InsertStmDataRow(name, Guid.Empty, Guid.Empty, "STR", systemValue);
			Helper.InsertStmDataRow(name, Guid.Empty, departmentKey, "STR", systemDepartmentValue);
			Helper.InsertStmDataRow(name, companyKey, Guid.Empty, "STR", companyValue);
			Helper.InsertStmDataRow(name, companyKey, departmentKey, "STR", companyDepartmentValue);
			Helper.InsertStmDataRow(name, branchKey, Guid.Empty, "STR", branchValue);
			Helper.InsertStmDataRow(name, branchKey, departmentKey, "STR", branchDepartmentValue);

			AssertEquals("GetStmDataSystemValue(SD_Name)", systemValue, Helper.GetStmDataValue(name));
			AssertEquals("GetStmDataSystemValue(SD_Name, CompanyPK)", companyValue, Helper.GetStmDataValue(name, companyKey));
			AssertEquals("GetStmDataSystemValue(SD_Name, BranchPK)", branchValue, Helper.GetStmDataValue(name, branchKey));
			AssertEquals("GetStmDataSystemValue(SD_Name, EmptyPK, DepartmentPK)", systemDepartmentValue, Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertEquals("GetStmDataSystemValue(SD_Name, CompanyPK, DepartmentPK)", companyDepartmentValue, Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertEquals("GetStmDataSystemValue(SD_Name, BranchPK, DepartmentPK)", branchDepartmentValue, Helper.GetStmDataValue(name, branchKey, departmentKey));
		}

		public void TestGetStmDataValue_ByPrimaryKey()
		{
			// Arrange 
			Guid pk = Guid.NewGuid();
			string sqlText = @"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (@PK, 'CustomDomainCredentials', NEWID(), NEWID(), CONVERT(varbinary(max), N'Pedram'));";

			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.ExecuteNonQuery();
			}

			// Act  
			var binaryValue = Helper.GetStmDataValue(pk);
			var stringValue = System.Text.Encoding.Unicode.GetString(binaryValue);

			// Assert  
			AssertEquals("The result of GetStmDataValue(pk) should have been 'Pedram'.", "Pedram", stringValue);
		}

		public void TestGetStmDataValue_ByPrimaryKey_WithNull()
		{
			// Arrange 
			Guid pk = Guid.NewGuid();
			string sqlText = @"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (@PK, 'CustomDomainCredentials', NEWID(), NEWID(), NULL);";

			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.ExecuteNonQuery();
			}

			// Act  
			var binaryValue = Helper.GetStmDataValue(pk);

			// Assert  
			Assert("The result of GetStmDataValue(pk) should have been 'null'.", binaryValue == null);
		}

		public void TestGetValuesByName()
		{
			// Arrange 
			byte[] binaryValue1 = System.Text.Encoding.Unicode.GetBytes("Pedram");
			byte[] binaryValue2 = System.Text.Encoding.Unicode.GetBytes("Zadno");
			byte[] binaryValue3 = System.Text.Encoding.Unicode.GetBytes("Azizi");

			string sqlText = @"
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (NEWID(), 'CustomDomainCredentials_Test', NEWID(), NEWID(), @Value1);
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (NEWID(), 'CustomDomainCredentials_Test', NEWID(), NEWID(), @Value2);
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (NEWID(), 'CustomDomainCredentials_Test', NEWID(), NEWID(), @Value3);
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue) VALUES (NEWID(), 'CustomDomainCredentials_Test', NEWID(), NEWID(), @Value4);
";
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@Value1", SqlDbType.VarBinary, 4000, binaryValue1);
				cmd.AddParameter("@Value2", SqlDbType.VarBinary, 4000, binaryValue2);
				cmd.AddParameter("@Value3", SqlDbType.VarBinary, 4000, binaryValue3);
				cmd.AddParameter("@Value4", SqlDbType.VarBinary, 4000, DBNull.Value);
				cmd.ExecuteNonQuery();
			}

			Dictionary<Guid, string> resultDictionary = new Dictionary<Guid, string>();

			// Act  
			foreach (var item in Helper.GetValuesByName("CustomDomainCredentials_Test"))
			{
				resultDictionary.Add(item.Key, (item.Value == null) ? null : System.Text.Encoding.Unicode.GetString(item.Value));
			}

			// Assert  
			Assert("The result of GetValuesByName() should have contained 'Pedram'.", resultDictionary.ContainsValue("Pedram"));
			Assert("The result of GetValuesByName() should have contained 'Zadno'.", resultDictionary.ContainsValue("Zadno"));
			Assert("The result of GetValuesByName() should have contained 'Azizi'.", resultDictionary.ContainsValue("Azizi"));
			Assert("The result of GetValuesByName() should not have contained value.", resultDictionary.ContainsValue(null));
		}

		public void TestCopyRows()
		{
			var sourceRegistryItemName = "O'SourceRegistryItem";
			var destinationRegistryItemName = "O'DestinationRegistryItem";

			var branchKey = Guid.NewGuid();
			var companyKey = Guid.NewGuid();
			var departmentKey = Guid.NewGuid();

			Helper.InsertStmDataRow(sourceRegistryItemName, Guid.Empty, Guid.Empty, "Enterprise");
			Helper.InsertStmDataRow(sourceRegistryItemName, Guid.Empty, departmentKey, "Enterprise Department");
			Helper.InsertStmDataRow(sourceRegistryItemName, companyKey, Guid.Empty, "Company");
			Helper.InsertStmDataRow(sourceRegistryItemName, companyKey, departmentKey, "Company Department");
			Helper.InsertStmDataRow(sourceRegistryItemName, branchKey, Guid.Empty, "Branch");
			Helper.InsertStmDataRow(sourceRegistryItemName, branchKey, departmentKey, "Branch Department");

			Helper.InsertStmDataRow(destinationRegistryItemName, companyKey, Guid.Empty, "Company Old");

			Helper.CopyRows(sourceRegistryItemName, destinationRegistryItemName);

			AssertEquals("DestinationRegistryItem Enterprise Value", "Enterprise", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, Guid.Empty, Guid.Empty)));
			AssertEquals("DestinationRegistryItem Enterprise Department Value", "Enterprise Department", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, Guid.Empty, departmentKey)));
			AssertEquals("DestinationRegistryItem Company Value", "Company Old", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, companyKey, Guid.Empty)));
			AssertEquals("DestinationRegistryItem Company Department Value", "Company Department", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, companyKey, departmentKey)));
			AssertEquals("DestinationRegistryItem Branch Value", "Branch", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, branchKey, Guid.Empty)));
			AssertEquals("DestinationRegistryItem Branch Department Value", "Branch Department", Encoding.Unicode.GetString(Helper.GetStmDataValue(destinationRegistryItemName, branchKey, departmentKey)));
		}

		public void TestDeleteStmDataRow()
		{
			var name = "O'TestGetStmDataSystemValue";

			var systemValue = Encoding.Unicode.GetBytes("System Value");
			var systemDepartmentValue = Encoding.Unicode.GetBytes("System Department Value");
			var companyValue = Encoding.Unicode.GetBytes("Company Value");
			var companyDepartmentValue = Encoding.Unicode.GetBytes("Company Department Value");
			var branchValue = Encoding.Unicode.GetBytes("Branch Value");
			var branchDepartmentValue = Encoding.Unicode.GetBytes("Branch Department Value");

			var branchKey = Guid.NewGuid();
			var companyKey = Guid.NewGuid();
			var departmentKey = Guid.NewGuid();

			Helper.InsertStmDataRow(name, Guid.Empty, Guid.Empty, "STR", systemValue);
			Helper.InsertStmDataRow(name, Guid.Empty, departmentKey, "STR", systemDepartmentValue);
			Helper.InsertStmDataRow(name, companyKey, Guid.Empty, "STR", companyValue);
			Helper.InsertStmDataRow(name, companyKey, departmentKey, "STR", companyDepartmentValue);
			Helper.InsertStmDataRow(name, branchKey, Guid.Empty, "STR", branchValue);
			Helper.InsertStmDataRow(name, branchKey, departmentKey, "STR", branchDepartmentValue);

			Helper.DeleteStmDataRow(name, Guid.Empty);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.DeleteStmDataRow(name, Guid.Empty, departmentKey);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.DeleteStmDataRow(name, companyKey, Guid.Empty);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.DeleteStmDataRow(name, companyKey, departmentKey);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.DeleteStmDataRow(name, branchKey, Guid.Empty);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNotNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.DeleteStmDataRow(name, branchKey, departmentKey);
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));
		}

		public void TestInsertStmDataRow()
		{
			var name = "O'TestGetStmDataSystemValue";

			var systemValue = Encoding.Unicode.GetBytes("System Value");
			var systemDepartmentValue = Encoding.Unicode.GetBytes("System Department Value");
			var companyValue = Encoding.Unicode.GetBytes("Company Value");
			var companyDepartmentValue = Encoding.Unicode.GetBytes("Company Department Value");
			var branchValue = Encoding.Unicode.GetBytes("Branch Value");
			var branchDepartmentValue = Encoding.Unicode.GetBytes("Branch Department Value");

			var branchKey = Guid.NewGuid();
			var companyKey = Guid.NewGuid();
			var departmentKey = Guid.NewGuid();

			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, "STR", systemValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertNull(Helper.GetStmDataValue(name, Guid.Empty, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, Guid.Empty, departmentKey, "STR", systemDepartmentValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertEquals("System Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, departmentKey)));
			AssertNull(Helper.GetStmDataValue(name, companyKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, companyKey, Guid.Empty, "STR", companyValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertEquals("System Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, departmentKey)));
			AssertEquals("Company Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, Guid.Empty)));
			AssertNull(Helper.GetStmDataValue(name, companyKey, departmentKey));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, companyKey, departmentKey, "STR", companyDepartmentValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertEquals("System Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, departmentKey)));
			AssertEquals("Company Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, Guid.Empty)));
			AssertEquals("Company Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, departmentKey)));
			AssertNull(Helper.GetStmDataValue(name, branchKey, Guid.Empty));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, branchKey, Guid.Empty, "STR", branchValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertEquals("System Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, departmentKey)));
			AssertEquals("Company Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, Guid.Empty)));
			AssertEquals("Company Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, departmentKey)));
			AssertEquals("Branch Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, branchKey, Guid.Empty)));
			AssertNull(Helper.GetStmDataValue(name, branchKey, departmentKey));

			Helper.InsertStmDataRow(name, branchKey, departmentKey, "STR", branchDepartmentValue);
			AssertEquals("System Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, Guid.Empty)));
			AssertEquals("System Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, Guid.Empty, departmentKey)));
			AssertEquals("Company Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, Guid.Empty)));
			AssertEquals("Company Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, companyKey, departmentKey)));
			AssertEquals("Branch Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, branchKey, Guid.Empty)));
			AssertEquals("Branch Department Value", Encoding.Unicode.GetString(Helper.GetStmDataValue(name, branchKey, departmentKey)));
		}

		public void TestInsertStmDataRow_ForBooleans()
		{
			Helper.InsertStmDataRow("True Value", Guid.Empty, Guid.Empty, true);
			Helper.InsertStmDataRow("False Value", Guid.Empty, Guid.Empty, false);

			AssertValue("True Value", "True");
			AssertValue("False Value", "False");

			void AssertValue(string registryName, string expectedValue)
			{
				var binaryValue = Helper.GetStmDataValue(registryName);
				AssertEquals(expectedValue, Encoding.Unicode.GetString(binaryValue));
			}
		}

		public void TestInsertStmDataRowNullValue()
		{
			const string name = "O'TestRegistryItemName";
			AssertNoExceptionThrown(() => Helper.InsertStmDataRow(name, "BIN", null));
			AssertNull(Helper.GetStmDataValue(name));
		}

		public void TestGetCompanyPKs()
		{
			Guid companyA = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyA, "ABC", "AU");
			Guid companyB = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyB, "DEF", "NZ");

			List<Guid> guids = Helper.GetCompanyPKs();

			Assert(guids.Contains(companyA));
			Assert(guids.Contains(companyB));
		}

		public void TestGetBranchPKs()
		{
			Guid companyA = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyA, "ABC", "AU");
			Guid companyB = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyB, "DEF", "NZ");
			Guid branchA = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchA, "BAB", "AUSYD", companyA);
			Guid branchB = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchB, "BDE", "NZAKL", companyB);
			Guid branchC = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchC, "BCC", "AUMEL", companyA);
			Guid branchD = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchD, "BDD", "NZWEL", companyB);

			List<Guid> guids = Helper.GetBranchPKs(companyA);

			AssertEquals(2, guids.Count);
			Assert(guids.Contains(branchA));
			Assert(guids.Contains(branchC));
			Assert(!guids.Contains(branchB));
			Assert(!guids.Contains(branchD));

			guids = Helper.GetBranchPKs(companyB);

			AssertEquals(2, guids.Count);
			Assert(!guids.Contains(branchA));
			Assert(!guids.Contains(branchC));
			Assert(guids.Contains(branchB));
			Assert(guids.Contains(branchD));
		}

		public void TestGetCompanyPKFromBranch()
		{
			var companyA = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyA, "ABC", "AU");
			var branchA = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchA, "BAB", "AUSYD", companyA);

			var companyPK = Helper.GetCompanyPKFromBranch(branchA);
			AssertEquals("The company pk should be properly retrieved", companyA, companyPK);

			companyPK = Helper.GetCompanyPKFromBranch(Guid.NewGuid());
			AssertNull("The method should return null", companyPK);
		}

		public void TestGetBranchCodes()
		{
			Guid companyA = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyA, "ABC", "AU");
			Guid companyB = Guid.NewGuid();
			TestDataCreator.CreateCompany(companyB, "DEF", "NZ");
			Guid branchA = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchA, "BAB", "AUSYD", companyA);
			Guid branchB = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchB, "BDE", "NZAKL", companyB);
			Guid branchC = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchC, "BCC", "AUMEL", companyA);
			Guid branchD = Guid.NewGuid();
			TestDataCreator.CreateBranch(branchD, "BDD", "NZWEL", companyB);

			List<string> codes = Helper.GetBranchCodes(companyA);

			AssertEquals(2, codes.Count);
			Assert(codes.Contains("BAB"));
			Assert(codes.Contains("BCC"));
			Assert(!codes.Contains("BDE"));
			Assert(!codes.Contains("BDD"));

			codes = Helper.GetBranchCodes(companyB);

			AssertEquals(2, codes.Count);
			Assert(!codes.Contains("BAB"));
			Assert(!codes.Contains("BCC"));
			Assert(codes.Contains("BDE"));
			Assert(codes.Contains("BDD"));
		}

		public void TestGetDepartmentCodes()
		{
			Guid departmentA = Guid.NewGuid();
			Guid departmentB = Guid.NewGuid();
			TestDataCreator.CreateDepartment(departmentA, "AAA");
			TestDataCreator.CreateDepartment(departmentB, "BBB");

			List<string> codes = Helper.GetDepartmentCodes();

			Assert(codes.Contains("AAA"));
			Assert(codes.Contains("BBB"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Helper = GetNewRegistryTransformationHelper();
		}

		protected virtual RegistryTransformationHelper GetNewRegistryTransformationHelper()
		{
			return new RegistryTransformationHelper();
		}

		RegistryTransformationHelper Helper;

		#region Transformation Test Data Creator

		TransformationTestDataCreator TestDataCreator
		{
			get { return testDataCreator ?? (testDataCreator = new TransformationTestDataCreator()); }
		}

		TransformationTestDataCreator testDataCreator;

		#endregion

		#endregion
	}
}
