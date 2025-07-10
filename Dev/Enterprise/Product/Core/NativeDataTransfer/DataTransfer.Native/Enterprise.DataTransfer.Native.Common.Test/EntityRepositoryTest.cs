using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Common.Operations;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	public class EntityRepositoryTest : TransactionedTestCase
	{
		public void TestCreateEntity()
		{
			dummyBizo.InternalPK = Guid.Empty;

			repository.OpenSession();
			repository.Insert(dummyBizo);
			repository.CloseSession();

			AssertNotEquals(Guid.Empty, dummyBizo.InternalPK);
			var row = rowFactory.FindByPK(dummyBizo.InternalPK, "DummyBizo");
			AssertNotNull(row);
		}

		public void TestCreateEntity_WithSystemLogColumns()
		{
			var connection = TestUtil.Connection;
			const string sql = "select count(*) FROM dbo.OrgHeader Where OH_FullName = 'Zayden Zubin Rakhsh Lola'";

			var orgHeader = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			orgHeader.InternalPK = Guid.Empty;
			orgHeader["Code"] = "ZUBORG";
			orgHeader["FullName"] = "Zayden Zubin Rakhsh Lola";
			orgHeader.Action = EntityAction.INSERT;

			var beforeCount = (int)connection.ExecuteScalar(sql);
			AssertEquals("Precondition", 0, beforeCount);

			repository.OpenSession();
			repository.Insert(orgHeader);
			repository.CloseSession();

			var afterCount = (int)connection.ExecuteScalar(sql);
			AssertEquals(beforeCount + 1, afterCount);

			var row = rowFactory.FindByPK(orgHeader.InternalPK, "OrgHeader");
			AssertNotNull("Should log Create Time when record save to the db", row["OH_SystemCreateTimeUtc"]);
			AssertEquals("Should log who create record when record save to the db", Env.CurrentUser.Initials, row["OH_SystemCreateUser"]);
			AssertEquals("Should log branch record created in when record save to the db", Env.CurrentBranch.Code, row["OH_SystemCreateBranch"]);
			AssertEquals("Should log department record created in when record save to the db", Env.CurrentDepartment.Code, row["OH_SystemCreateDepartment"]);
		}

		public void TestCreateEntity_Twice()
		{
			var connection = TestUtil.Connection;
			const string sql = "select count(*) FROM dbo.OrgHeader Where OH_FullName = 'Zayden Zubin Rakhsh Lola'";

			var orgHeader = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			orgHeader.InternalPK = Guid.Empty;
			orgHeader["Code"] = "ZUBORG";
			orgHeader["FullName"] = "Zayden Zubin Rakhsh Lola";
			orgHeader.Action = EntityAction.INSERT;

			var beforeCount = (int)connection.ExecuteScalar(sql);

			repository.OpenSession();
			repository.Insert(orgHeader);
			repository.CloseSession();

			orgHeader.InternalPK = Guid.Empty;
			orgHeader.Action = EntityAction.INSERT;
			orgHeader["Code"] = "ZUBCOM";

			repository.OpenSession();
			repository.Insert(orgHeader);
			repository.CloseSession();

			var afterCount = (int)connection.ExecuteScalar(sql);

			AssertEquals(beforeCount + 2, afterCount);
		}

		public void TestUpdateEntity_PrimaryKey()
		{
			Assert("Should be able to update entity base on its primary Key", true);

			var propertyDefinitions = dummyBizo.Definition.PropertyDefinitions;
			var propertyDef = propertyDefinitions["Description"];
			var column = propertyDef.ColumnDef;
			var pk = dummyBizo.InternalPK;
			dummyBizo["Code"] = "ABC";
			dummyBizo[propertyDef.PropertyName] = "Testing";

			repository.OpenSession();
			repository.Update(dummyBizo);
			repository.CloseSession();

			var row = rowFactory.FindByPK(pk, "DummyBizo");
			AssertEquals("Corresponding Row should be update", dummyBizo[propertyDef.PropertyName], row[column.Name]);
		}

		public void TestMergeEntityWithNoCriteriaDoesNotThrowException()
		{
			var definition = info.Entities.FindDefinition("DummyBizo");
			var testBizo = new Entity(definition, sessionServices);
			testBizo.Action = EntityAction.MERGE;
			testBizo.InternalPK = Guid.Empty;

			repository.OpenSession();
			AssertNoExceptionThrown(() => repository.MergeBatch(new Entity[] { testBizo }));
			repository.CloseSession();
		}

		public void TestUpdateParentThenChild()
		{
			repository.OpenSession();
			try
			{
				Assert("Updating parent and then updating a dependent child should be a valid operation", true);
				var propertyDefinitions = dummyBizo.Definition.PropertyDefinitions;
				var propertyDef = propertyDefinitions["Description"];
				var column = propertyDef.ColumnDef;
				var pk = dummyBizo.InternalPK;
				dummyBizo["Code"] = "ABC";
				dummyBizo[propertyDef.PropertyName] = "Testing";

				repository.Update(dummyBizo);

				var row = rowFactory.FindByPK(pk, "DummyBizo");
				AssertEquals("Corresponding Row should be update", dummyBizo[propertyDef.PropertyName], row[column.Name]);

				var childPropertyDefinitions = dummyDependentBizo.Definition.PropertyDefinitions;
				var childPropertyDef = childPropertyDefinitions["Code"];
				var childColumn = childPropertyDef.ColumnDef;
				var childPk = dummyDependentBizo.InternalPK;
				dummyDependentBizo["Code"] = "XYZ";
				dummyDependentBizo[childPropertyDef.PropertyName] = "ABC";

				repository.Update(dummyDependentBizo);

				var childrow = rowFactory.FindByPK(childPk, "DummyDependentBizo");
				AssertEquals("Corresponding row in child should be updated", dummyDependentBizo[childPropertyDef.PropertyName], childrow[childColumn.Name]);

				dummyBizo["Code"] = "123";
				dummyBizo[propertyDef.PropertyName] = "Testing1";

				AssertNoExceptionThrown(() => repository.Update(dummyBizo));
			}
			finally
			{
				repository.CloseSession();
			}
		}

		public void TestUpdateParentDirectly()
		{
			repository.OpenSession();
			try
			{
				Assert("Updating parent and then updating a dependent child thourgh an UpdateOperation on an EntitySet should be a valid operation", true);
				var entitySet = new EntitySet("dummies");
				entitySet.Root = dummyBizo;
				var pk = dummyBizo.InternalPK;

				var row = rowFactory.FindByPK(pk, "DummyBizo");

				var childPk = dummyDependentBizo.InternalPK;

				var parentDbEntity = new DBEntity("DummyBizo", pk, DBEntity.DbAction.Unchanged);
				var childDbEntity = new DBEntity("DummyDependentBizo", childPk, DBEntity.DbAction.Update);
				((StatisticsImpl)repository.Statistics).Add(parentDbEntity);
				((StatisticsImpl)repository.Statistics).Add(childDbEntity);

				var updateOperation = new UpdateOperation { EntityRepository = repository };
				row.SetModified();
				AssertNoExceptionThrown(() => updateOperation.Update(entitySet));
			}
			finally
			{
				repository.CloseSession();
			}
		}

		public void TestUpdateEntity_SingleCandidateKey()
		{
			Assert("Should be able to update entity base on its candidateKey", true);

			var propertyDefinitions = dummyBizo.Definition.PropertyDefinitions;
			var propertyDef = propertyDefinitions["Description"];
			var column = propertyDef.ColumnDef;
			var pk = dummyBizo.InternalPK;

			// Use Candidate Key to update when InternalPK is empty
			dummyBizo.InternalPK = Guid.Empty;
			dummyBizo["Code"] = "ABC";
			dummyBizo[propertyDef.PropertyName] = "Testing";

			repository.OpenSession();
			repository.Update(dummyBizo);
			repository.CloseSession();

			var row = rowFactory.FindByPK(pk, "DummyBizo");
			AssertEquals("Corresponding Row should be update", dummyBizo[propertyDef.PropertyName], row[column.Name]);
		}

		public void TestUpdateEntity_MultiColumnCandidateKey()
		{
			Assert("Should be able to update entity base on its candidateKey", true);

			var propertyDefinitions = dummyBizo.Definition.PropertyDefinitions;
			var propertyDef = propertyDefinitions["Description"];
			var column = propertyDef.ColumnDef;

			// Use Multi Column Candidate Key to update when InternalPK is empty
			var pk = dummyBizo.InternalPK;
			dummyBizo.InternalPK = Guid.Empty;
			dummyBizo["Number"] = 4;
			dummyBizo["Guid"] = pk;
			dummyBizo[propertyDef.PropertyName] = "Testing";

			repository.OpenSession();
			repository.Update(dummyBizo);
			repository.CloseSession();

			var row = rowFactory.FindByPK(pk, "DummyBizo");
			AssertEquals("Corresponding Row should be update", dummyBizo[propertyDef.PropertyName], row[column.Name]);
		}

		public void TestUpdateEntity_WithSystemLogColumns()
		{
			var factory = new BusinessObjectFactory();
			var prevUser = Env.CurrentUser.Initials;
			var testUser = factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TES";
			var prevBranch = Env.CurrentBranch.Code;
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			var prevDepartment = Env.CurrentDepartment.Code;
			var testDepartment = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();

			var connection = TestUtil.Connection;
			const string sql = "select count(*) FROM dbo.OrgHeader Where OH_FullName = 'Zayden Zubin Rakhsh Lola'";
			// Insert Record
			var orgHeader = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			orgHeader.InternalPK = Guid.Empty;
			orgHeader["Code"] = "ZUBORG";
			orgHeader["FullName"] = "Zayden Zubin Rakhsh Lola";
			orgHeader.Action = EntityAction.INSERT;

			var beforeCount = (int)connection.ExecuteScalar(sql);
			AssertEquals("Precondition: Should be a new record", 0, beforeCount);

			repository.OpenSession();
			repository.Insert(orgHeader);
			repository.CloseSession();

			var afterCount = (int)connection.ExecuteScalar(sql);
			AssertEquals("Precondition: Record should be inserted to db", beforeCount + 1, afterCount);

			var row = rowFactory.FindByPK(orgHeader.InternalPK, "OrgHeader");
			AssertNotNull("Precondition: Create Time should not be empty after recored was created", row["OH_SystemCreateTimeUtc"]);
			AssertEquals("Precondition: Create User should not be empty after record was created", prevUser, row["OH_SystemCreateUser"]);
			AssertEquals("Precondition: Create Branch should not be empty after record was created", prevBranch, row["OH_SystemCreateBranch"]);
			AssertEquals("Precondition: Create Department should not be empty after record was created", prevDepartment, row["OH_SystemCreateDepartment"]);
			AssertNotNull("Precondition: Last Edit Time should not be empty after recored was created", row["OH_SystemLastEditTimeUtc"]);
			AssertEquals("Precondition: Last Edit User should not be empty after record was created", Env.CurrentUser.Initials, row["OH_SystemLastEditUser"]);

			var createTimeBeforeUpdate = row["OH_SystemCreateTimeUtc"];
			var editTimeBeforeUpdate = row["OH_SystemLastEditTimeUtc"];

			Thread.Sleep(200);

			// Update Record
			orgHeader["FullName"] = "Test Test Test";
			orgHeader.Action = EntityAction.UPDATE;
			
			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, testBranch.PK.ToGuid(), testDepartment.PK.ToGuid()))
			{
				repository.OpenSession();
				repository.Update(orgHeader);
				repository.CloseSession();
			}

			var updatedRow = rowFactory.FindByPK(orgHeader.InternalPK, "OrgHeader");
			AssertNotNull("Precondition: Create Time should not be empty after recored was created", updatedRow["OH_SystemCreateTimeUtc"]);
			AssertNotNull("Precondition: Last Edit Time should not be empty after recored was created", updatedRow["OH_SystemLastEditTimeUtc"]);
			AssertEquals("Precondition: Create User should not be updated after record was created", Env.CurrentUser.Initials, updatedRow["OH_SystemCreateUser"]);
			AssertEquals("Precondition: Create Branch should not be updated after record was created", Env.CurrentBranch.Code, row["OH_SystemCreateBranch"]);
			AssertEquals("Precondition: Create Department should not be updated after record was created", Env.CurrentDepartment.Code, row["OH_SystemCreateDepartment"]);
			AssertEquals("Precondition: Last Edit User should not be empty after record was created", testUser.GS_Code, updatedRow["OH_SystemLastEditUser"]);

			AssertEquals("Create Time would not be changed", createTimeBeforeUpdate, updatedRow["OH_SystemCreateTimeUtc"]);
			AssertNotEquals("Last Update Time should be changed", editTimeBeforeUpdate, updatedRow["OH_SystemLastEditTimeUtc"]);
		}

		public void TestDeleteSingleEntity()
		{
			repository.OpenSession();
			repository.Delete(dummyDependentBizo);
			repository.CloseSession();

			var row = rowFactory.FindByPK(dummyDependentBizo.InternalPK, "DummyDependentBizo");
			AssertNull(row);
		}

		public void TestDeleteSingleEntity_JunctionTable()
		{
			repository.OpenSession();
			repository.Delete(dummyDependentSuffixBizo);
			repository.CloseSession();
			var junctionRow = rowFactory.FindByPK(pivotPk, "DummyPivot");
			AssertNull(junctionRow);
		}

		public void TestDeleteEntitySet()
		{
			repository.OpenSession();
			repository.Delete(dummyDependentSuffixBizo);
			repository.Delete(dummyDependentBizo);
			repository.Delete(dummyBizo);
			repository.CloseSession();
			var row = rowFactory.FindByPK(dummyBizo.InternalPK, "DummyBizo");
			AssertNull(row);
			row = rowFactory.FindByPK(dummyDependentBizo.InternalPK, "DummyDependentBizo");
			AssertNull(row);
			row = rowFactory.FindByPK(dummyDependentSuffixBizo.InternalPK, "DummyDependentBizo");
			AssertNull(row);
		}

		public void TestDeleteParentWithoutDeletingChildTriggersException()
		{
			repository.OpenSession();
			repository.Delete(dummyBizo);
			AssertExceptionThrown<ZDataException>(() => repository.CloseSession());
		}

		public void TestFindEntity()
		{
			//Entity InternalPK should not be emtpy
			repository.OpenSession();
			var entity = repository.Find(dummyBizo);
			AssertNotEquals(Guid.Empty, entity);
			repository.CloseSession();

			var internalPK = dummyBizo.InternalPK;
			dummyBizo.InternalPK = Guid.Empty;

			repository.OpenSession();
			entity = repository.Find(dummyBizo);
			repository.CloseSession();

			AssertNotEquals(Guid.Empty, entity);
			AssertEquals(internalPK, entity.InternalPK);
		}

		public void TestFindEntity_MultiCandidateKeys()
		{
			var pk = dummyBizo.InternalPK;
			dummyBizo.InternalPK = Guid.Empty;
			dummyBizo["Code"] = "WRONG";
			dummyBizo["Number"] = 4;
			dummyBizo["Guid"] = pk;

			repository.OpenSession();
			try
			{
				var entity = repository.Find(dummyBizo);
				AssertNotEquals(Guid.Empty, entity);
				AssertEquals("Should find the entity even if the first candidate key is wrong", pk, entity.InternalPK);
			}
			catch
			{
				Fail("Should find the entity even if the first candidate key is wrong");
			}
			finally
			{
				repository.CloseSession();
			}
		}

		public void TestFindEntity_NonCustomized_StmNote()
		{
			var connection = TestUtil.Connection;
			var orgPK = TestUtil.PrepareOrgHeaderTableData();

			var note = TestUtil.PrepareStmNoteEntity(sessionServices);
			note.InternalPK = Guid.Empty;
			note["Description"] = "Description";
			note["NoteContext"] = "AAA";
			note["NoteType"] = "AAA";
			note["IsCustomDescription"] = "false";
			note["NoteText"] = "Text";
			repository.OpenSession();
			repository.Insert(note);
			repository.CloseSession();
			var notePK = note.InternalPK;
			AssertNotEquals(notePK, Guid.Empty);

			note.InternalPK = Guid.Empty;
			note["NoteText"] = "NewText";
			repository.OpenSession();
			repository.Find(note);
			repository.CloseSession();
			var noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "Text");

			note.InternalPK = Guid.Empty;
			note["Description"] = "Other Description";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["Description"] = "Description";

			note.InternalPK = Guid.Empty;
			note["NoteContext"] = "BBB";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["NoteContext"] = "AAA";

			note.InternalPK = Guid.Empty;
			note["NoteType"] = "BBB";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["NoteType"] = "AAA";

			note.InternalPK = Guid.Empty;
			note["IsCustomDescription"] = "true";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["IsCustomDescription"] = "false";
		}

		public void TestFindEntity_Customized_StmNote()
		{
			var connection = TestUtil.Connection;
			var orgPK = TestUtil.PrepareOrgHeaderTableData();

			var note = TestUtil.PrepareStmNoteEntity(sessionServices);
			note.InternalPK = Guid.Empty;
			note["Description"] = "Description";
			note["NoteContext"] = "AAA";
			note["NoteType"] = "AAA";
			note["IsCustomDescription"] = "true";
			note["NoteText"] = "Text";
			repository.OpenSession();
			repository.Insert(note);
			repository.CloseSession();
			var notePK = note.InternalPK;
			AssertNotEquals(notePK, Guid.Empty);

			note.InternalPK = Guid.Empty;
			note["NoteType"] = "BBB";
			note["NoteContext"] = "BBB";
			note["NoteText"] = "NewText";
			repository.OpenSession();
			repository.Find(note);
			repository.CloseSession();
			var noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "Text");

			note.InternalPK = Guid.Empty;
			note["Description"] = "Other Description";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["Description"] = "Description";

			note.InternalPK = Guid.Empty;
			note["IsCustomDescription"] = "false";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Find(note));
			repository.CloseSession();
			note["IsCustomDescription"] = "true";
		}

		public void TestUpdateEntity_NonCustomized_StmNote()
		{
			var connection = TestUtil.Connection;
			var orgPK = TestUtil.PrepareOrgHeaderTableData();

			var note = TestUtil.PrepareStmNoteEntity(sessionServices);
			note.InternalPK = Guid.Empty;
			note["Description"] = "Description";
			note["NoteContext"] = "AAA";
			note["NoteType"] = "AAA";
			note["IsCustomDescription"] = "false";
			note["NoteText"] = "Text";
			repository.OpenSession();
			repository.Insert(note);
			repository.CloseSession();
			var notePK = note.InternalPK;
			AssertNotEquals(notePK, Guid.Empty);

			note.InternalPK = Guid.Empty;
			note["NoteText"] = "NewText";
			repository.OpenSession();
			repository.Update(note);
			repository.CloseSession();
			var noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "NewText");

			note.InternalPK = Guid.Empty;
			note["NoteType"] = "BBB";
			note["NoteText"] = "NewText - 2";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Update(note));
			repository.CloseSession();
			noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "NewText");
		}

		public void TestUpdateEntity_Customized_StmNote()
		{
			var connection = TestUtil.Connection;
			var orgPK = TestUtil.PrepareOrgHeaderTableData();

			var note = TestUtil.PrepareStmNoteEntity(sessionServices);
			note.InternalPK = Guid.Empty;
			note["Description"] = "Description";
			note["NoteContext"] = "AAA";
			note["NoteType"] = "AAA";
			note["IsCustomDescription"] = "true";
			note["NoteText"] = "Text";
			repository.OpenSession();
			repository.Insert(note);
			repository.CloseSession();
			var notePK = note.InternalPK;
			AssertNotEquals(notePK, Guid.Empty);

			note.InternalPK = Guid.Empty;
			note["NoteType"] = "BBB";
			note["NoteContext"] = "BBB";
			note["NoteText"] = "NewText";
			repository.OpenSession();
			repository.Update(note);
			repository.CloseSession();
			var noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "NewText");

			note.InternalPK = Guid.Empty;
			note["Description"] = "Other Description";
			note["NoteText"] = "NewText - 2";
			repository.OpenSession();
			AssertExceptionThrown<NativeXMLUserVisibleException>(() => repository.Update(note));
			repository.CloseSession();
			noteText = (string)connection.ExecuteScalar(string.Format("select ST_NoteText from dbo.StmNote where ST_PK = '{0}'", notePK));
			AssertEquals(noteText, "NewText");
		}

		public void TestNoLogsAreAddedToDbOnSessionClose()
		{
			var orgHeader1 = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			orgHeader1.InternalPK = Guid.Empty;
			orgHeader1["Code"] = "ORGTST1";
			orgHeader1["FullName"] = "Test Org Full Name 1";
			orgHeader1.Action = EntityAction.INSERT;
			var orgHeader2 = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			orgHeader2.InternalPK = Guid.Empty;
			orgHeader2["Code"] = "ORGTST2";
			orgHeader2["FullName"] = "Test Org Full Name 2";
			orgHeader2.Action = EntityAction.INSERT;

			var logsInitialCount = GetRowsCount(StmALogSchema.Constants.TableName);
			var orgsInitialCount = GetRowsCount(OrgHeaderSchema.Constants.TableName);

			repository.OpenSession();
			repository.Insert(orgHeader1);
			AssertEquals("No new logs in DB", logsInitialCount, GetRowsCount(StmALogSchema.Constants.TableName));
			repository.Insert(orgHeader2);
			AssertEquals("No new logs in DB", logsInitialCount, GetRowsCount(StmALogSchema.Constants.TableName));
			repository.CloseSession();

			var logsFinalCount = GetRowsCount(StmALogSchema.Constants.TableName);
			var orgsFinalCount = GetRowsCount(OrgHeaderSchema.Constants.TableName);

			AssertEquals("No logs were inserted", 0, logsFinalCount - logsInitialCount);
			AssertEquals("Two orgs were inserted", 2, orgsFinalCount - orgsInitialCount);
		}

		public void TestRefCountryCanOnlyBeInserted()
		{
			var refCountry = TestUtil.PrepareRefCountryEntity(sessionServices);//PrepareOrgHeaderEntity(sessionServices);
			refCountry.InternalPK = Guid.Empty;
			refCountry["Code"] = "A1";
			refCountry.Action = EntityAction.INSERT;

			repository.OpenSession();
			repository.Insert(refCountry);
			repository.CloseSession();
			var count = (int)TestUtil.Connection.ExecuteScalar("select count(1) from dbo.RefCountry where RN_Code = 'A1'");
			AssertEquals("RefCountry is inserted without error", 1, count);

			repository.OpenSession();
			repository.Update(refCountry);
			repository.CloseSession();
			AssertEquals("Warning: Cannot use Native XML to update/delete RefCountry.", string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()[0]));

			repository.OpenSession();
			repository.Delete(refCountry);
			repository.CloseSession();
			AssertEquals("Warning: Cannot use Native XML to update/delete RefCountry.", string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()[1]));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestUtil.AddColumnsToDummy();
			TestUtil.AddIndexsToDummy();
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(TestUtil.GetDefinitionLoaderForTest());
			sessionServices = new AncillaryImportServices();
			info = TestUtil.GetEntitySetDefinition("Dummy");

			var connection = TestUtil.Connection;
			var context = new EntityContext(sessionServices, new FactoryProvider()) { Connection = connection };
			rowFactory = new RowRepository(connection, context.RowFactory);
			repository = new EntityRepository(context, sessionServices);

			var pk = TestUtil.PrepareDummyBizoData("ABC");
			var definition = info.Entities.FindDefinition("DummyBizo");
			dummyBizo = new Entity(definition, sessionServices);
			dummyBizo.InternalPK = pk;

			var dependentPk = TestUtil.PrepareDummyDependentBizoData(pk);
			dummyDependentBizo =
				new Entity(info.Entities.FindDefinition("DummyBizo.DummyDependentBizo"), sessionServices)
				{
					InternalPK = dependentPk
				};

			dependentPk = TestUtil.PrepareDummyDependentBizoData(pk);
			pivotPk = TestUtil.PrepareDummyPivotData(pk, dependentPk);
			dummyDependentSuffixBizo =
				new Entity(info.Entities.FindDefinition("DummyBizo.DummyDependentBizo_Suffix"), sessionServices)
				{
					InternalPK = dependentPk
				};

			dummyBizo.ChildrenCollection.Add(dummyDependentBizo);
			dummyBizo.ChildrenCollection.Add(dummyDependentSuffixBizo);

			dummyDependentBizo.Parent = dummyBizo;
			dummyDependentSuffixBizo.Parent = dummyBizo;
		}

		int GetRowsCount(string tableName)
		{
			using (var command = TestUtil.Connection.Command("select count(*) FROM " + tableName))
			{
				return (int)command.ExecuteScalar();
			}
		}

		RowRepository rowFactory;
		EntityRepository repository;
		EntitySetDefinition info;
		AncillaryImportServices sessionServices;
		Entity dummyBizo;
		Entity dummyDependentBizo;
		Entity dummyDependentSuffixBizo;

		Guid pivotPk;

		#endregion
	}
}
