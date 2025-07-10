using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing
{
	class RegistryDataTransformation_Test : NUnit.Framework.TransactionedTestCase
	{
		public void TestGetDataTable()
		{
			InsertTestValue();
			DataTable table = Transformation.GetDataTable("TEST_NAME");
			AssertEquals("Rows.Count", 1, table.Rows.Count);
			AssertEquals("Rows[0][\"SD_PK\"]", pk, (Guid)table.Rows[0][StmDataSchema.Constants.PK]);
			AssertEquals("Rows[0][\"SD_Name\"]", "TEST_NAME", (string)table.Rows[0][StmDataSchema.Constants.SD_Name]);
			AssertEquals("Rows[0][\"SD_Owner\"]", owner, (Guid)table.Rows[0][StmDataSchema.Constants.SD_Owner]);
			AssertEquals("Rows[0][\"SD_DepartmentGuid\"]", department, (Guid)table.Rows[0][StmDataSchema.Constants.SD_DepartmentGuid]);
			AssertEquals("Rows[0][\"SD_Type\"]", "ABC", (string)table.Rows[0][StmDataSchema.Constants.SD_Type]);
			AssertEquals("Rows[0][\"SD_IsLogged\"]", false, table.Rows[0][StmDataSchema.Constants.SD_IsLogged]);
			AssertEquals("Rows[0][\"SD_BinaryValue\"]", new byte[] { 1, 2, 3 }, (byte[])table.Rows[0][StmDataSchema.Constants.SD_BinaryValue]);
			AssertEquals("Rows[0][\"SD_GuidValue\"]", guid, (Guid)table.Rows[0][StmDataSchema.Constants.SD_GuidValue]);
		}

		public void TestGetDataTableFull()
		{
			InsertTestValue();
			DataTable table = RegistryDataTransformation.GetDataTableFull("TEST_NAME");
			AssertEquals("Rows.Count", 1, table.Rows.Count);
			AssertEquals("Rows[0][\"SD_PK\"]", pk, (Guid)table.Rows[0][StmDataSchema.Constants.PK]);
			AssertEquals("Rows[0][\"SD_Name\"]", "TEST_NAME", (string)table.Rows[0][StmDataSchema.Constants.SD_Name]);
			AssertEquals("Rows[0][\"SD_Owner\"]", owner, (Guid)table.Rows[0][StmDataSchema.Constants.SD_Owner]);
			AssertEquals("Rows[0][\"SD_DepartmentGuid\"]", department, (Guid)table.Rows[0][StmDataSchema.Constants.SD_DepartmentGuid]);
			AssertEquals("Rows[0][\"SD_Type\"]", "ABC", (string)table.Rows[0][StmDataSchema.Constants.SD_Type]);
			AssertEquals("Rows[0][\"SD_IsLogged\"]", false, table.Rows[0][StmDataSchema.Constants.SD_IsLogged]);
			AssertEquals("Rows[0][\"SD_BinaryValue\"]", new byte[] { 1, 2, 3 }, (byte[])table.Rows[0][StmDataSchema.Constants.SD_BinaryValue]);
			AssertEquals("Rows[0][\"SD_GuidValue\"]", guid, (Guid)table.Rows[0][StmDataSchema.Constants.SD_GuidValue]);
			AssertEquals("Rows[0][\"SD_IsCancelled\"]", false, (bool)table.Rows[0][StmDataSchema.Constants.SD_IsCancelled]);
			AssertEquals("Rows[0][\"SD_PreserveTestValue\"]", false, (bool)table.Rows[0][StmDataSchema.Constants.SD_PreserveTestValue]);
		}

		public void TestUpdateDatabaseValue()
		{
			InsertTestValue();
			var helper = new RegistryTransformationHelper();
			Transformation.UpdateDatabaseValue(pk, new byte[] { 4, 5, 6 });
			byte[] value = helper.GetStmDataValue("TEST_NAME", owner, department);
			AssertEquals("SD_BinaryValue", new byte[] { 4, 5, 6 }, value);
			Transformation.UpdateDatabaseValue(pk, null);
			value = helper.GetStmDataValue("TEST_NAME", owner, department);
			AssertNull("SD_BinaryValue rest to NULL", value);

			Transformation.UpdateDatabaseValue(pk, "BIN", new byte[] { 7, 8, 9 });
			value = helper.GetStmDataValue("TEST_NAME", owner, department);
			AssertEquals("SD_BinaryValue", new byte[] { 7, 8, 9 }, value);
			Transformation.UpdateDatabaseValue(pk, "BIN", null);
			value = new RegistryTransformationHelper().GetStmDataValue("TEST_NAME", owner, department);
			AssertNull("SD_BinaryValue reset to NULL", value);

			Transformation.UpdateDatabaseValue("TEST_NAME", new byte[] { 4, 5, 6 });
			value = helper.GetStmDataValue("TEST_NAME", owner, department);
			AssertEquals("SD_BinaryValue", new byte[] { 4, 5, 6 }, value);
			Transformation.UpdateDatabaseValue("TEST_NAME", null);
			value = helper.GetStmDataValue("TEST_NAME", owner, department);
			AssertNull("SD_BinaryValue reset to NULL", value);
		}

		public void TestTransformOverridesRegistryItemNameCopiedByCopyProductionToTest()
		{
			var newItemName = "MailboxEmailAddress";
			var oldItemName = "POP3MailboxEmailAddress";
			var helper = new RegistryTransformationTestHelper();
			helper.DeleteStmDataRow(newItemName);
			helper.InsertStmDataRow(newItemName, "STR", Encoding.Unicode.GetBytes("some@mailbox.com"));
			helper.InsertStmDataRow(oldItemName, "STR", Encoding.Unicode.GetBytes("some@pop3.com"));

			Transformation.UpdateRegistryItemName(oldItemName, newItemName);

			AssertEquals("MailboxEmailAddress should exist", true, helper.GetStmDataRowCount(newItemName) == 1);
			AssertEquals("POP3MailboxEmailAddress should not exists", false, helper.GetStmDataRowCount(oldItemName) == 1);
			AssertEquals("MailboxEmailAddress should be 'some@mailbox.com'", "some@mailbox.com", Encoding.Unicode.GetString(helper.GetStmDataValue("MailboxEmailAddress")));
		}

		public void TestUpdateRegistryItemName()
		{
			AssertNull("Precondition", new RegistryTransformationHelper().GetStmDataValue("TEST_NAME", owner, department));
			AssertNull("Precondition", new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_2", owner, department));
			AssertNull("Precondition", new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_3", owner, department));

			InsertTestValue();

			pk = Guid.NewGuid();
			InsertTestValue("TEST_NAME_2", new byte[] { 4, 5, 6 });

			AssertNotNull(new RegistryTransformationHelper().GetStmDataValue("TEST_NAME", owner, department));
			Transformation.UpdateRegistryItemName("TEST_NAME", "TEST_NAME_3");

			AssertNull(new RegistryTransformationHelper().GetStmDataValue("TEST_NAME", owner, department));
			AssertNotNull(new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_2", owner, department));
			AssertNotNull(new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_3", owner, department));

			AssertEquals("SD_BinaryValue", new byte[] { 1, 2, 3 }, new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_3", owner, department));
			AssertEquals("SD_BinaryValue", new byte[] { 4, 5, 6 }, new RegistryTransformationHelper().GetStmDataValue("TEST_NAME_2", owner, department));
		}

		public void TestInsertDatabaseValue()
		{
			var ownerPK = Guid.NewGuid();
			var departmentPK = Guid.NewGuid();

			Transformation.InsertDatabaseValue("O'Name_1", new byte[] { 1 });
			Transformation.InsertDatabaseValue("O'Name_2", ownerPK, new byte[] { 2 });
			Transformation.InsertDatabaseValue("O'Name_3", ownerPK, departmentPK, new byte[] { 3 });
			Transformation.InsertDatabaseValue("O'Name_4", null, null, new byte[] { 4 });
			Transformation.InsertDatabaseValue("O'Name_5", ownerPK, departmentPK, new byte[] { 5 }, "AAA");
			Transformation.InsertDatabaseValue("O'Name_6", null, null, new byte[] { 6 }, "AAA");

			var helper = new RegistryTransformationHelper();
			AssertEquals(new byte[] { 1 }, helper.GetStmDataValue("O'Name_1"));
			AssertEquals(new byte[] { 2 }, helper.GetStmDataValue("O'Name_2", ownerPK));
			AssertEquals(new byte[] { 3 }, helper.GetStmDataValue("O'Name_3", ownerPK, departmentPK));
			AssertEquals(new byte[] { 4 }, helper.GetStmDataValue("O'Name_4", Guid.Empty, Guid.Empty));
			AssertEquals(new byte[] { 5 }, helper.GetStmDataValue("O'Name_5", ownerPK, departmentPK));
			AssertEquals(new byte[] { 6 }, helper.GetStmDataValue("O'Name_6", Guid.Empty, Guid.Empty));
		}

		public void TestUpdateOrCreateDatabaseValue()
		{
			InsertTestValue();

			var helper = new RegistryTransformationHelper();
			AssertEquals("Precondition", new byte[] { 1, 2, 3 }, helper.GetStmDataValue("TEST_NAME", owner, department));
			AssertEquals("Precondition", null, helper.GetStmDataValue("O'Name_2", Guid.Empty, Guid.Empty));

			Transformation.UpdateOrCreateDatabaseValue("TEST_NAME", new byte[] { 1 });
			Transformation.UpdateOrCreateDatabaseValue("O'Name_2", new byte[] { 2 });

			AssertEquals(new byte[] { 1 }, helper.GetStmDataValue("TEST_NAME", owner, department));
			AssertEquals(new byte[] { 2 }, helper.GetStmDataValue("O'Name_2", Guid.Empty, Guid.Empty));

			Transformation.UpdateOrCreateDatabaseValue("TEST_NAME", owner, Guid.Empty, "BIN", true, new byte[] { 3 }, Guid.Empty, true, true);
			Transformation.UpdateOrCreateDatabaseValue("O'Name_3", owner, Guid.Empty, "BIN", true, new byte[] { 4 }, Guid.Empty, false, true);

			DataTable table = RegistryDataTransformation.GetDataTableFull("TEST_NAME");
			AssertEquals("Rows.Count", 1, table.Rows.Count);
			AssertEquals("Rows[0][\"SD_PK\"]", pk, (Guid)table.Rows[0][StmDataSchema.Constants.PK]);
			AssertEquals("Rows[0][\"SD_Name\"]", "TEST_NAME", (string)table.Rows[0][StmDataSchema.Constants.SD_Name]);
			AssertEquals("Rows[0][\"SD_Owner\"]", owner, (Guid)table.Rows[0][StmDataSchema.Constants.SD_Owner]);
			AssertEquals("Rows[0][\"SD_DepartmentGuid\"]", Guid.Empty, (Guid)table.Rows[0][StmDataSchema.Constants.SD_DepartmentGuid]);
			AssertEquals("Rows[0][\"SD_Type\"]", "BIN", (string)table.Rows[0][StmDataSchema.Constants.SD_Type]);
			AssertEquals("Rows[0][\"SD_IsLogged\"]", true, table.Rows[0][StmDataSchema.Constants.SD_IsLogged]);
			AssertEquals("Rows[0][\"SD_BinaryValue\"]", new byte[] { 3 }, (byte[])table.Rows[0][StmDataSchema.Constants.SD_BinaryValue]);
			AssertEquals("Rows[0][\"SD_GuidValue\"]", Guid.Empty, (Guid)table.Rows[0][StmDataSchema.Constants.SD_GuidValue]);
			AssertEquals("Rows[0][\"SD_IsCancelled\"]", true, (bool)table.Rows[0][StmDataSchema.Constants.SD_IsCancelled]);
			AssertEquals("Rows[0][\"SD_PreserveTestValue\"]", true, (bool)table.Rows[0][StmDataSchema.Constants.SD_PreserveTestValue]);

			table = RegistryDataTransformation.GetDataTableFull("O'Name_3");
			AssertEquals("Rows.Count", 1, table.Rows.Count);
			AssertEquals("Rows[0][\"SD_Name\"]", "O'Name_3", (string)table.Rows[0][StmDataSchema.Constants.SD_Name]);
			AssertEquals("Rows[0][\"SD_Owner\"]", owner, (Guid)table.Rows[0][StmDataSchema.Constants.SD_Owner]);
			AssertEquals("Rows[0][\"SD_DepartmentGuid\"]", Guid.Empty, (Guid)table.Rows[0][StmDataSchema.Constants.SD_DepartmentGuid]);
			AssertEquals("Rows[0][\"SD_Type\"]", "BIN", (string)table.Rows[0][StmDataSchema.Constants.SD_Type]);
			AssertEquals("Rows[0][\"SD_IsLogged\"]", true, table.Rows[0][StmDataSchema.Constants.SD_IsLogged]);
			AssertEquals("Rows[0][\"SD_BinaryValue\"]", new byte[] { 4 }, (byte[])table.Rows[0][StmDataSchema.Constants.SD_BinaryValue]);
			AssertEquals("Rows[0][\"SD_GuidValue\"]", Guid.Empty, (Guid)table.Rows[0][StmDataSchema.Constants.SD_GuidValue]);
			AssertEquals("Rows[0][\"SD_IsCancelled\"]", false, (bool)table.Rows[0][StmDataSchema.Constants.SD_IsCancelled]);
			AssertEquals("Rows[0][\"SD_PreserveTestValue\"]", true, (bool)table.Rows[0][StmDataSchema.Constants.SD_PreserveTestValue]);
		}

		#region Implementation

		void InsertTestValue()
		{
			pk = Guid.NewGuid();
			owner = Guid.NewGuid();
			department = Guid.NewGuid();
			guid = Guid.NewGuid();

			InsertTestValue("TEST_NAME", new byte[] { 1, 2, 3 });
		}

		void InsertTestValue(string testRegistryItemName, byte[] testValue)
		{
			string query = string.Format(@"
					INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
					VALUES (@SD_PK, @SD_Name, @SD_Owner, @SD_DepartmentGuid, @SD_Type, @SD_IsLogged, @SD_BinaryValue, @SD_GuidValue)",
				StmDataSchema.Constants.TableName,                                               // 0
				StmDataSchema.Constants.PK, StmDataSchema.Constants.SD_Name,                     // 1, 2
				StmDataSchema.Constants.SD_Owner, StmDataSchema.Constants.SD_DepartmentGuid,     // 3, 4
				StmDataSchema.Constants.SD_Type, StmDataSchema.Constants.SD_IsLogged,            // 5, 6
				StmDataSchema.Constants.SD_BinaryValue, StmDataSchema.Constants.SD_GuidValue);   // 7, 8

			using (DbCommand command = Db.Connection.Command(query)) // It is impossible to use BusinessObjectFactory here
			{
				command.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@SD_Name", testRegistryItemName, StmDataSchema.SD_Name);
				command.AddParameterBasedOnDbColumn("@SD_Owner", owner, StmDataSchema.SD_Owner);
				command.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", department, StmDataSchema.SD_DepartmentGuid);
				command.AddParameterBasedOnDbColumn("@SD_Type", "ABC", StmDataSchema.SD_Type);
				command.AddParameterBasedOnDbColumn("@SD_IsLogged", false, StmDataSchema.SD_IsLogged);
				command.AddParameterBasedOnDbColumn("@SD_BinaryValue", testValue, StmDataSchema.SD_BinaryValue);
				command.AddParameterBasedOnDbColumn("@SD_GuidValue", guid, StmDataSchema.SD_GuidValue);
				command.ExecuteNonQuery();
			}
		}

		DummyBaseRegistryTransformation Transformation
		{
			get
			{
				if (transformation == null)
				{
					transformation = new DummyBaseRegistryTransformation();
				}
				return transformation;
			}
		}

		Guid pk;
		Guid owner;
		Guid department;
		Guid guid;
		DummyBaseRegistryTransformation transformation;

		#region class DummyBaseRegistryTransformation

		class DummyBaseRegistryTransformation : RegistryDataTransformation
		{
			protected override void OfflinePostUpgradeTransform()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override string UserDescription
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}
		}

		#endregion

		#endregion
	}
}
