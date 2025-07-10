namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(AuditEvent))]
	public class AuditEventTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if auditServerFactory is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: auditServerFactory",
#else
				"Value cannot be null. (Parameter 'auditServerFactory')",
#endif
				() => new AuditEvent(Factory, auditServerFactory: null, auditedEntity: null));

			AssertExceptionThrown(
				"Constructor Throws Exception if auditedTable is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: auditedEntity",
#else
				"Value cannot be null. (Parameter 'auditedEntity')",
#endif
				() => new AuditEvent(Factory, auditServerFactory: Factory, auditedEntity: null));
		}

		public void TestTimeLocal()
		{
			var testAuditEvent = new AuditEventForTest(Factory);
			AssertEquals("TimeLocal when TimeUtc is not set", ZDateTime.Empty, testAuditEvent.TimeLocal);

			testAuditEvent.TimeUtc = ZDateTime.UtcNow;

			AssertEquals(
				"TimeLocal when UTC = " + testAuditEvent.TimeUtc.ToISO8601String(),
				testAuditEvent.TimeUtc.ToLocationTime(GlbBranch.CurrentBranch.HomePort).ToZDateTime(),
				testAuditEvent.TimeLocal);

			AssertEquals(
				"TimeUtc can be reverse calculated from local = " + testAuditEvent.TimeLocal.ToISO8601String(),
				testAuditEvent.TimeLocal.ToUniversalBranchTime(Factory),
				testAuditEvent.TimeUtc);
		}

		public void TestUserName()
		{
			var testAuditEvent = new AuditEventForTest(Factory);
			AssertEquals("UserName when UserCode is empty", ZString.Empty, testAuditEvent.UserName);

			testAuditEvent.UserCode = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("UserName", GlbStaff.CurrentUser.GS_FullName + " (" + testAuditEvent.UserCode + ")", testAuditEvent.UserName);
		}

		public void TestSourceName()
		{
			var testAuditEvent = new AuditEventForTest(Factory, GlbStaffSchema.Instance);
			AssertEquals("SourceName (GlbStaff)", Factory.New<GlbStaff>().HumanReadableName, testAuditEvent.SourceName);

			testAuditEvent = new AuditEventForTest(Factory, OrgHeaderSchema.Instance);
			AssertEquals("SourceName (OrgHeader)", Factory.New<OrgHeader>().HumanReadableName, testAuditEvent.SourceName);
		}

		public void TestOperationSymbol()
		{
			var testAuditEvent = new AuditEventForTest(Factory);
			AssertEquals("OperationSymbol when Operation has not been set", ZString.Empty, testAuditEvent.OperationSymbol);

			testAuditEvent.Operation = (int)Audit.ChangeOperation.Insert;
			AssertEquals("OperationSymbol (Insert)", "+", testAuditEvent.OperationSymbol);

			testAuditEvent.Operation = (int)Audit.ChangeOperation.Delete;
			AssertEquals("OperationSymbol (Delete)", "-", testAuditEvent.OperationSymbol);

			testAuditEvent.Operation = (int)Audit.ChangeOperation.AfterUpdate;
			AssertEquals("OperationSymbol (AfterUpdate)", ZString.Empty, testAuditEvent.OperationSymbol);
		}

		public void TestShouldUseAuditServerFactoryAccessAuditDatabase()
		{
			var auditServerFactory = new BusinessObjectFactory(Db.AuditDatabaseName);

			var wrapperConnection = ((IDbConnected)auditServerFactory).Connection;
			var connection = Db.NewExtraConnectionToMainDb();
			using (wrapperConnection.TrackExecutedCommands())
			using (connection.TrackExecutedCommands())
			{
				var factory = new BusinessObjectFactory(connection);

				var testAuditEvent = new AuditEventForTest(factory, auditServerFactory);
				var tableInfo = new TableInfo("TestAuditTable", null, null, null, "Test_Code", null, null);
				testAuditEvent.TableAndColumnExists_exposed(tableInfo);

				AssertEquals("Must use AuditServerFactory to access the Audit Database", false, connection.ExecutedCommands.Any(x => x.Contains($"[{Db.AuditDatabaseName}].INFORMATION_SCHEMA.TABLES")));
				AssertEquals("Must use AuditServerFactory to access the Audit Database", true, wrapperConnection.ExecutedCommands.Any(x => x.Contains($"[{Db.AuditDatabaseName}].INFORMATION_SCHEMA.TABLES")));
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AuditEventForTest(Factory);
		}

		#endregion
	}

	public class AuditEventForTest : AuditEvent
	{
		public AuditEventForTest(BusinessObjectFactory factory) : this(factory, GlbStaffSchema.Instance)
		{
		}

		public AuditEventForTest(BusinessObjectFactory factory, ITableSchema auditedTable) : base(factory, factory, new AuditEntity(auditedTable.PK, null))
		{
		}

		public AuditEventForTest(BusinessObjectFactory factory, BusinessObjectFactory dwServerFactory) : base(factory, dwServerFactory, new AuditEntity(GlbStaffSchema.PK, null))
		{
		}

		public bool TableAndColumnExists_exposed(TableInfo parameter)
		{
			return TableAndColumnExists(parameter);
		}
	}
}
