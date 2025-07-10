namespace Enterprise.AuditDataServices.Business.Testing
{
	using System;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(AuditChangeCollection))]
	class AuditChangeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AuditChangeCollection>
	{
		public void TestConstructorValidation()
		{
			AssertExceptionThrown(
				"Constructor Throws Exception if auditEvent is null",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: auditEvent",
#else
				"Value cannot be null. (Parameter 'auditEvent')",
#endif
				() => new AuditChangeCollection(auditEvent: null));
		}

		public void TestReload()
		{
			var testBizObj = Factory.New<OrgHeader>();

			InsertTestAuditRecords(
				OrgHeaderSchema.Constants.SqlSchemaName,
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.Constants.Prefix,
				testBizObj.PK.ToGuid());

			var lastEditUserFriendlyName = testBizObj.FindPropertyInfo(OrgHeaderSchema.Constants.OH_SystemLastEditUser).HumanReadableName;
			var codeFriendlyName = testBizObj.FindPropertyInfo(OrgHeaderSchema.Constants.OH_Code).HumanReadableName;
			var isActiveFriendlyName = testBizObj.FindPropertyInfo(OrgHeaderSchema.Constants.OH_IsActive).HumanReadableName;
			var oldColumnFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(OrgHeaderSchema.Constants.Prefix + "_OldColumn");

			// LSN = 0x01
			var eventLsn1 = new AuditEventForTest(Factory, OrgHeaderSchema.Instance) { ParentPk = testBizObj.PK, ChangePeriod = 1703, ChangeLsn = bin1, Operation = (int)Audit.ChangeOperation.Insert };
			AssertEquals("[LSN = 0x01 Insert] Change Count > 0", true, eventLsn1.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn1.ChangeCollection, OrgHeaderSchema.Constants.PK, "", testBizObj.PK);
			AssertAuditChange(eventLsn1.ChangeCollection, lastEditUserFriendlyName, "", GlbStaff.CurrentUser.GS_Code);
			AssertAuditChange(eventLsn1.ChangeCollection, codeFriendlyName, "", "A1");
			AssertAuditChange(eventLsn1.ChangeCollection, isActiveFriendlyName, "", ZBool.True);
			AssertAuditChange(eventLsn1.ChangeCollection, oldColumnFriendlyName, "", "@");

			// LSN = 0x02
			var eventLsn2 = new AuditEventForTest(Factory, OrgHeaderSchema.Instance) { ParentPk = testBizObj.PK, ChangePeriod = 1703, ChangeLsn = bin2, Operation = (int)Audit.ChangeOperation.AfterUpdate };
			AssertEquals("[LSN = 0x02 Update] Change Count", 3, eventLsn2.ChangeCollection.Count);
			AssertAuditChange(eventLsn2.ChangeCollection, lastEditUserFriendlyName, GlbStaff.CurrentUser.GS_Code, "#U!");
			AssertAuditChange(eventLsn2.ChangeCollection, isActiveFriendlyName, ZBool.True, ZBool.False);
			AssertAuditChange(eventLsn2.ChangeCollection, oldColumnFriendlyName, "@", "#");

			// LSN = 0x03
			var eventLsn3 = new AuditEventForTest(Factory, OrgHeaderSchema.Instance) { ParentPk = testBizObj.PK, ChangePeriod = 1703, ChangeLsn = bin3, Operation = (int)Audit.ChangeOperation.AfterUpdate };
			AssertEquals("[LSN = 0x03 Update] Change Count", 3, eventLsn3.ChangeCollection.Count);
			AssertAuditChange(eventLsn3.ChangeCollection, lastEditUserFriendlyName, "#U!", GlbStaff.CurrentUser.GS_Code);
			AssertAuditChange(eventLsn3.ChangeCollection, codeFriendlyName, "A1", "A2");
			AssertAuditChange(eventLsn3.ChangeCollection, isActiveFriendlyName, ZBool.False, ZBool.True);

			// LSN = 0x04
			var eventLsn4 = new AuditEventForTest(Factory, OrgHeaderSchema.Instance) { ParentPk = testBizObj.PK, ChangePeriod = 1703, ChangeLsn = bin4, Operation = (int)Audit.ChangeOperation.AfterUpdate };
			AssertEquals("[LSN = 0x04 Update] Change Count", 1, eventLsn4.ChangeCollection.Count);
			AssertAuditChange(eventLsn4.ChangeCollection, codeFriendlyName, "A2", "A3");

			// LSN = 0x05
			var eventLsn5 = new AuditEventForTest(Factory, OrgHeaderSchema.Instance) { ParentPk = testBizObj.PK, ChangePeriod = 1703, ChangeLsn = bin5, Operation = (int)Audit.ChangeOperation.Delete };
			AssertEquals("[LSN = 0x05 Delete] Change Count > 0", true, eventLsn5.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn5.ChangeCollection, OrgHeaderSchema.Constants.PK, testBizObj.PK, "");
			AssertAuditChange(eventLsn5.ChangeCollection, lastEditUserFriendlyName, GlbStaff.CurrentUser.GS_Code, "");
			AssertAuditChange(eventLsn5.ChangeCollection, codeFriendlyName, "A3", "");
			AssertAuditChange(eventLsn5.ChangeCollection, isActiveFriendlyName, ZBool.True, "");
			AssertAuditChange(eventLsn5.ChangeCollection, oldColumnFriendlyName, "", "");
		}

		public void TestDatetimeColumnAccuracy()
		{
			var pk = Guid.NewGuid();
			var gsLastEditTimeFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[GS_PK], [GS_Code], [GS_SystemLastEditTimeUtc]
				)
					VALUES
						(1712, 1, 0x02, 0x01, 3, 0x0, '{3}', '#U1', '2000-01-01 01:01:00.230'),
						(1712, 1, 0x02, 0x01, 4, 0x0, '{3}', '#U1', '2000-01-01 01:01:00.330');",
				/*0*/Db.AuditDatabaseName,
				/*1*/GlbStaffSchema.Constants.SqlSchemaName,
				/*2*/GlbStaffSchema.Constants.TableName,
				/*3*/pk.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
			var eventLsn1 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk, ChangePeriod = 1712, ChangeLsn = bin2, Operation = (int)Audit.ChangeOperation.AfterUpdate };

			AssertEquals("[LSN = 0x01 Insert] Change Count > 0", true, eventLsn1.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn1.ChangeCollection, gsLastEditTimeFriendlyName, "2000-01-01 01:01:00.230", "2000-01-01 01:01:00.330");
		}

		public void TestPasswordFieldsReplacedByStars()
		{
			Guid pk = Guid.NewGuid();
			var gsCodeFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_Code);
			var gsBrokerPasswordFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_BrokerPassword);
			var gsBrokerPasswordStatusFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_BrokerPasswordStatus);
			var gsPasswordHashIterationsFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_PasswordHashIterations);

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[GS_PK], [GS_Code], [GS_BrokerPassword], [GS_BrokerPasswordStatus], [GS_PasswordHashIterations]
				)
					VALUES
						(1712, 1, 0x01, 0x01, 2, 0x0, '{3}', '#U1', 'TEST_BROKER_PASSWORD', 'PWD', 1),
						(1712, 1, 0x02, 0x01, 3, 0x0, '{3}', '#U1', 'TEST_BROKER_PASSWORD', 'PWD', 1),
						(1712, 1, 0x02, 0x01, 4, 0x0, '{3}', '#U1', '', 'NEW', 2),
						(1801, 1, 0x03, 0x01, 1, 0x0, '{3}', '#U1', '', 'NEW', 2);",
				/*0*/Db.AuditDatabaseName,
				/*1*/GlbStaffSchema.Constants.SqlSchemaName,
				/*2*/GlbStaffSchema.Constants.TableName,
				/*3*/pk.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			// LSN = 0x01
			var eventLsn1 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk, ChangePeriod = 1712, ChangeLsn = bin1, Operation = (int)Audit.ChangeOperation.Insert };
			AssertEquals("[LSN = 0x01 Insert] Change Count > 0", true, eventLsn1.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn1.ChangeCollection, GlbStaffSchema.Constants.PK, "", pk);
			AssertAuditChange(eventLsn1.ChangeCollection, gsCodeFriendlyName, "", "#U1");
			AssertAuditChange(eventLsn1.ChangeCollection, gsBrokerPasswordFriendlyName, "", "***");
			AssertAuditChange(eventLsn1.ChangeCollection, gsBrokerPasswordStatusFriendlyName, "", "PWD");
			AssertAuditChange(eventLsn1.ChangeCollection, gsPasswordHashIterationsFriendlyName, "", 1);

			// LSN = 0x02
			var eventLsn2 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk, ChangePeriod = 1712, ChangeLsn = bin2, Operation = (int)Audit.ChangeOperation.AfterUpdate };
			AssertEquals("[LSN = 0x02 Update] Change Count", 3, eventLsn2.ChangeCollection.Count);
			AssertAuditChange(eventLsn2.ChangeCollection, gsBrokerPasswordFriendlyName, "***", "");
			AssertAuditChange(eventLsn2.ChangeCollection, gsBrokerPasswordStatusFriendlyName, "PWD", "NEW");
			AssertAuditChange(eventLsn2.ChangeCollection, gsPasswordHashIterationsFriendlyName, 1, 2);

			// LSN = 0x03
			var eventLsn3 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk, ChangePeriod = 1801, ChangeLsn = bin3, Operation = (int)Audit.ChangeOperation.Delete };
			AssertEquals("[LSN = 0x03 Insert] Change Count > 0", true, eventLsn3.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn3.ChangeCollection, GlbStaffSchema.Constants.PK, pk, "");
			AssertAuditChange(eventLsn3.ChangeCollection, gsCodeFriendlyName, "#U1", "");
			AssertAuditChange(eventLsn3.ChangeCollection, gsBrokerPasswordFriendlyName, "", "");
			AssertAuditChange(eventLsn3.ChangeCollection, gsBrokerPasswordStatusFriendlyName, "NEW", "");
			AssertAuditChange(eventLsn3.ChangeCollection, gsPasswordHashIterationsFriendlyName, 2, "");
		}

		public void TestNullGuidsAreShownAsEmptyString()
		{
			Guid pk1 = Guid.NewGuid();
			Guid pk2 = Guid.NewGuid();
			Guid pk3 = Guid.NewGuid();

			Guid referenceId = Guid.NewGuid();

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[GS_PK], [GS_Code], [GS_ActiveDirectoryObjectGuid]
				)
					VALUES
						(1712, 1, 0x01, 0x01, 2, 0x0, '{3}', '#U1', '{6}'),
						(1712, 1, 0x01, 0x01, 2, 0x0, '{4}', '#U2', '{7}'),
						(1712, 1, 0x01, 0x01, 2, 0x0, '{5}', '#U3', NULL);",
				/*0*/Db.AuditDatabaseName,
				/*1*/GlbStaffSchema.Constants.SqlSchemaName,
				/*2*/GlbStaffSchema.Constants.TableName,
				/*3*/pk1.ToString(),
				/*4*/pk2.ToString(),
				/*5*/pk3.ToString(),
				/*6*/referenceId.ToString(),
				/*7*/Guid.Empty.ToString()
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			var gsActiveDirectoryObjectGuidFriendlyName = ZPropertyInfo.GetFriendlyColumnNameShared(GlbStaffSchema.Constants.GS_ActiveDirectoryObjectGuid);

			// PK1
			var eventLsn1 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk1, ChangePeriod = 1712, ChangeLsn = bin1, Operation = (int)Audit.ChangeOperation.Insert };
			AssertEquals("[PK1 Insert] Change Count > 0", true, eventLsn1.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn1.ChangeCollection, gsActiveDirectoryObjectGuidFriendlyName, "", referenceId);

			// PK2 Empty Guids should be shown as blank
			var eventLsn2 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk2, ChangePeriod = 1712, ChangeLsn = bin1, Operation = (int)Audit.ChangeOperation.Insert };
			AssertEquals("[PK2 Before Update] Change Count > 0", true, eventLsn2.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn2.ChangeCollection, gsActiveDirectoryObjectGuidFriendlyName, "", string.Empty);

			// PK3 - NULL Guids should be shown as blank
			var eventLsn3 = new AuditEventForTest(Factory, GlbStaffSchema.Instance) { ParentPk = pk2, ChangePeriod = 1712, ChangeLsn = bin1, Operation = (int)Audit.ChangeOperation.Insert };
			AssertEquals("[PK3 Before Update] Change Count > 0", true, eventLsn3.ChangeCollection.Count > 0);
			AssertAuditChange(eventLsn3.ChangeCollection, gsActiveDirectoryObjectGuidFriendlyName, "", string.Empty);
		}

		void InsertTestAuditRecords(string tableSchema, string tableName, string tablePrefix, Guid pk1)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"ALTER TABLE [{0}].[{1}].[{2}] ADD [CW!!D-20170313-0000!{3}_OldColumn] CHAR(1);",
				/*0*/Db.AuditDatabaseName,
				/*1*/tableSchema,
				/*2*/tableName,
				/*3*/tablePrefix
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[{1}].[{2}] (
					[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask],
					[{3}_PK], [{3}_SystemLastEditUser],
					[{3}_Code], [{3}_IsActive], [CW!!D-20170313-0000!{3}_OldColumn]
				)
					VALUES
						(1703, 1, 0x01, 0x01, 2, 0x0, '{4}', '{5}', 'A1', 1, '@'),

						(1703, 1, 0x02, 0x01, 3, 0x0, '{4}', '{5}', 'A1', 1, '@'),
						(1703, 1, 0x02, 0x01, 4, 0x0, '{4}', '#U!', 'A1', 0, '#'),

						(1703, 1, 0x03, 0x01, 3, 0x0, '{4}', '#U!', 'A1', 0, null),
						(1703, 1, 0x03, 0x01, 4, 0x0, '{4}', '{5}', 'A2', 1, null),

						(1703, 1, 0x04, 0x01, 3, 0x0, '{4}', '{5}', 'A2', 1, null),
						(1703, 1, 0x04, 0x01, 4, 0x0, '{4}', '{5}', 'A3', 1, null),

						(1703, 1, 0x05, 0x01, 1, 0x0, '{4}', '{5}', 'A3', 1, null);",
				/*0*/Db.AuditDatabaseName,
				/*1*/tableSchema,
				/*2*/tableName,
				/*3*/tablePrefix,
				/*4*/pk1.ToString(),
				/*5*/GlbStaff.CurrentUser.GS_Code
			);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
		}

		public static void AssertAuditChange(AuditChangeCollection changeCollection, ZString columnName, params object[] values)
		{
			CombineAssertions(() =>
			{
				var auditChanges = changeCollection.Where((bo) => bo.ColumnName == columnName).ToArray();
				AssertNotNull("AuditChange where ColumnName = " + columnName, auditChanges);
				AssertEquals("AuditChanges should have expected object.", values.Length - 1, auditChanges.Length);
				for (var i = 0; i < auditChanges.Length; i++)
				{
					if (auditChanges[i] != null)
					{
						AssertEquals("ColumnName", columnName, auditChanges[i].ColumnName);
						AssertEquals("ValueBefore", values[i].ToString(), auditChanges[i].ValueBefore);
						AssertEquals("ValueAfter", values[i + 1].ToString(), auditChanges[i].ValueAfter);
					}
				}
			});
		}

		protected override AuditChangeCollection GetCollectionToTest()
		{
			return new AuditChangeCollection(new AuditEventForTest(Factory, DummyLoggedSchema.Instance));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AuditChange(Factory);
		}

		readonly byte[] bin1 = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin2 = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin3 = new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin4 = new byte[] { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		readonly byte[] bin5 = new byte[] { 5, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	}
}
