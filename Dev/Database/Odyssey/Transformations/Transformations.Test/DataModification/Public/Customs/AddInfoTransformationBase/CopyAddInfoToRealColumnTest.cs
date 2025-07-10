using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Customs.AddInfoTransformationBase.Testing
{
	class CopyAddInfoToRealColumnDifferentColumnTypesTestClass : CopyAddInfoToRealColumn
	{
		public override string UserDescription => "Copy AddInfo value from Z0_VarCharMax to other columns";
		public override string AdditionalSourceTableJoin => "INNER JOIN (SELECT 1 AS D) Data ON Data.D = Z0_SparseShort";
		public override SchemaStringColumn SourceAddInfoColumn => DummyBizoSchema.Z0_VarCharMax;
		public IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> AddInfoColumnMappingForTesting;
		protected override IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> GetAddInfoColumnMapping()
		{
			if (AddInfoColumnMappingForTesting != null)
			{
				return AddInfoColumnMappingForTesting;
			}
			var result = new Dictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>(16);
			result.Add(DummyBizoSchema.Z0_AnotherDecimal, ("AnotherDecimal", null, null));
			result.Add(DummyBizoSchema.Z0_BitFalse, ("BitFalse", null, null));
			result.Add(DummyBizoSchema.Z0_Bool, ("Bool", null, null));
			result.Add(DummyBizoSchema.Z0_Byte, ("Byte", null, null));
			result.Add(DummyBizoSchema.Z0_Code, ("Code", null, null));
			result.Add(DummyBizoSchema.Z0_Date, ("Date", null, null));
			result.Add(DummyBizoSchema.Z0_DateOnly, ("DateOnly", null, null));
			result.Add(DummyBizoSchema.Z0_DateTimeOffset, ("DateTimeOffset", null, null));
			result.Add(DummyBizoSchema.Z0_Guid, ("Guid", null, null));
			result.Add(DummyBizoSchema.Z0_Long, ("Long", null, null));
			result.Add(DummyBizoSchema.Z0_Money, ("Money", null, null));
			result.Add(DummyBizoSchema.Z0_Number, ("Number", null, null));
			result.Add(DummyBizoSchema.Z0_Short, ("Short", null, null));
			result.Add(DummyBizoSchema.Z0_SmallDateTime, ("SmallDateTime", null, null));
			result.Add(DummyBizoSchema.Z0_SparseNumber, ("SparseNumber", null, null));
			result.Add(DummyBizoSchema.Z0_SparseGuid, ("SparseGuid", "OrgDummy.OH_PK", "LEFT JOIN dbo.OrgHeader OrgDummy ON OrgDummy.OH_PK = {0}"));
			return result;
		}
	}

	[TestedType(typeof(CopyAddInfoToRealColumn))]
	class CopyAddInfoToRealColumnBaseOnlyDifferentColumnTypesTest : CopyAddInfoToRealColumnTest<CopyAddInfoToRealColumnDifferentColumnTypesTestClass>
	{
		public void TestHandlingOfUnsupportedSchemaColumnType()
		{
			AssertHandlingOfUnsupportedSchemaColumnType(DummyBizoSchema.Z0_Geography);
			AssertHandlingOfUnsupportedSchemaColumnType(DummyBizoSchema.Z0_VarBinaryMax);
			AssertHandlingOfUnsupportedSchemaColumnType(DummyBizoSchema.Z0_Xml);
		}

		void AssertHandlingOfUnsupportedSchemaColumnType(SchemaColumn column)
		{
			var transformation = new CopyAddInfoToRealColumnDifferentColumnTypesTestClass();
			transformation.AddInfoColumnMappingForTesting = new Dictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>();
			transformation.AddInfoColumnMappingForTesting.Add(column, (column.Name, null, null));
			AssertExceptionThrown<NotSupportedException>(column.GetType().Name + " is not supported", column.GetType().Name + " is not currently supported in conversion", () => _ = transformation.TargetTableColumnMapping);
		}

		protected override void PrepareTestData()
		{
			ExtProperty.Table.Update(Db.Connection, TransformationToTest.SourceTableSchemaName, TransformationToTest.SourceTableName, TransformationToTest.Identifier, "1");
			UpdateAlreadyProcessedData(Array.Empty<string>());
			var offLineProcessingTableName = TransformationToTest.CreateOffLineProcessingTable(Db.Connection, Db.DatabaseName, false);
			var cmd = Db.Connection.Command(@"INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES(@OrgPK, 'DNTE@#$');");
			cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, OrgPK);
			cmd.ExecuteNonQuery();
			addInfoMapping = new Dictionary<Guid, string>(5);
			string addInfo = null;
			(Source1PK, addInfo) = SetupSourceData(1);
			addInfoMapping.Add(Source1PK, addInfo);
			(Source3PKNotInList, addInfo) = SetupSourceData(3);
			addInfoMapping.Add(Source3PKNotInList, addInfo);
			(Source4PKNoMatchAddInfo, addInfo) = SetupSourceDataWithNoMatchAddInfo(4);
			addInfoMapping.Add(Source4PKNoMatchAddInfo, addInfo);
			(Source5PKExisting, addInfo) = SetupSourceData(5);
			addInfoMapping.Add(Source5PKExisting, addInfo);
			Source9PKInvalidData = SetupSourceData(9, InvalidAddInfo);
			SetupExistingTargetData(Source5PKExisting, 5);
			var sqlSetupData = GetInsertOffLineProcessingTableNameScript("@Source1PK", false, 1)
				+ GetInsertOffLineProcessingTableNameScript("@Source4PKNoMatchAddInfo", false, 4)
				+ GetInsertOffLineProcessingTableNameScript("@Source5PKExisting", false, 5)
				+ GetInsertOffLineProcessingTableNameScript("@Source9PKInvalidData", false, 9);

			cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@Source1PK", SqlDbType.UniqueIdentifier, Source1PK);
			cmd.AddParameter("@Source4PKNoMatchAddInfo", SqlDbType.UniqueIdentifier, Source4PKNoMatchAddInfo);
			cmd.AddParameter("@Source5PKExisting", SqlDbType.UniqueIdentifier, Source5PKExisting);
			cmd.AddParameter("@Source9PKInvalidData", SqlDbType.UniqueIdentifier, Source9PKInvalidData);
			cmd.ExecuteNonQuery();
		}
		Dictionary<Guid, string> addInfoMapping;

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var sourceAddInfoColumn = TransformationToTest.SourceAddInfoColumn;
				AssertSourceAddInfo(1, Source1PK, sourceAddInfoColumn, addInfoMapping[Source1PK]);
				AssertTargetData(1, Source1PK, DummyBizoSchema.PK,
					ValidAddInfoMapping.Select(x => (x.Value.expectedResult, x.Key)).ToArray());
				AssertSourceAddInfo(3, Source3PKNotInList, sourceAddInfoColumn, addInfoMapping[Source3PKNotInList]);
				AssertTargetData(3, Source3PKNotInList, DummyBizoSchema.PK,
					ValidAddInfoMapping.Select(x => (GetEmptyValue(x.Key), x.Key)).ToArray());
				AssertSourceAddInfo(4, Source4PKNoMatchAddInfo, sourceAddInfoColumn, addInfoMapping[Source4PKNoMatchAddInfo]);
				AssertTargetData(4, Source4PKNoMatchAddInfo, DummyBizoSchema.PK,
					ValidAddInfoMapping.Select(x => (GetEmptyValue(x.Key), x.Key)).ToArray());
				AssertSourceAddInfo(5, Source5PKExisting, sourceAddInfoColumn, addInfoMapping[Source5PKExisting]);
				AssertTargetData(5, Source5PKExisting, DummyBizoSchema.PK,
					ValidAddInfoMapping.Select(x => (x.Value.expectedResult, x.Key)).ToArray());
				AssertEquals($"{TransformationToTest.OffLineProcessingTableName} should have been dropped", false, DbObjectCreator.TableExists(Db.Connection, TransformationToTest.OffLineProcessingTableName));
				AssertNull($"{TransformationToTest.Identifier} extension should have been deleted", ExtProperty.Table.Select(Db.Connection, TransformationToTest.SourceTableSchemaName, TransformationToTest.SourceTableName, TransformationToTest.Identifier));
				AssertContainsExactElementsInAnyOrder("already processed data", new[] {
					"AnotherDecimal",
					"BitFalse",
					"Bool",
					"Byte",
					"Code",
					"Date",
					"DateOnly",
					"DateTimeOffset",
					"Guid",
					"Long",
					"Money",
					"Number",
					"Short",
					"SmallDateTime",
					"SparseNumber",
					"SparseGuid"
				}, TransformationToTest.GetAlreadyProcessedAddInfos());
			});
		}

		protected override string ReasonNotToBeMapped => "Test class";

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyAddInfoToRealColumnDifferentColumnTypesTestClass();

		void SetupExistingTargetData(Guid sourcePK, int reference)
		{
			var updateStatement = string.Join(", ", ValidAddInfoMapping.Select(x => $"{x.Key.Name} = {GetEmptyValueString(x.Key)}"));
			var sqlSetupData = $@"
UPDATE dbo.DummyBizo
SET {updateStatement}
WHERE Z0_PK = @SourcePK
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.ExecuteNonQuery();
		}

		string GetEmptyValueString(SchemaColumn column)
		{
			return column.IsNullable ? "NULL" : column is SchemaBoolColumn boolColumn ? (boolColumn.IsBitField ? "0" : "'N'") : column.SqlDbDefault.ToString();
		}

		(Guid, string) SetupSourceDataWithNoMatchAddInfo(int reference)
		{
			var sourcePK = Guid.NewGuid();
			var description = "DEC" + reference.ToString("D2");
			var addInfo = "GREETING=HELLO";
			var sqlSetupData = @"
INSERT dbo.DummyBizo (Z0_PK, Z0_Description, Z0_AnotherNumber, Z0_VarCharMax, Z0_SparseVarChar, Z0_SparseShort)
VALUES (@SourcePK, @Description, @Reference, @AddInfo, 'IMP', 2);
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@Description", SqlDbType.VarChar, description);
			cmd.AddParameter("@Reference", SqlDbType.Int, reference);
			cmd.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
			cmd.ExecuteNonQuery();
			return (sourcePK, addInfo);
		}

		(Guid, string) SetupSourceData(int reference)
		{
			var addInfo = ValidAddInfo;
			var sourcePK = SetupSourceData(reference, addInfo);
			return (sourcePK, addInfo);
		}

		Guid SetupSourceData(int reference, string addInfo)
		{
			var sourcePK = Guid.NewGuid();
			var description = "DEC" + reference.ToString("D2");
			var sqlSetupData = @"
INSERT dbo.DummyBizo (Z0_PK, Z0_Description, Z0_AnotherNumber, Z0_VarCharMax, Z0_SparseVarChar, Z0_SparseShort)
VALUES (@SourcePK, @Description, @Reference, @AddInfo, 'IMP', 1);
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@Description", SqlDbType.VarChar, description);
			cmd.AddParameter("@Reference", SqlDbType.Int, reference);
			cmd.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
			cmd.ExecuteNonQuery();
			return sourcePK;
		}

		object GetEmptyValue(SchemaColumn column)
		{
			switch (column.Name)
			{
				case DummyBizoSchema.Constants.Z0_Bool:
					return "N";
				case DummyBizoSchema.Constants.Z0_Code:
					return "".PadRight(DummyBizoSchema.Z0_Code.MaxLength, ' ');
				default:
					return column.SqlDbDefault;
			}
		}

		string GetAddInfoString(Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)> addInfoMapping) => string.Join("*", addInfoMapping.Select(x => $"{x.Key.Name.Substring(3)}={x.Value.addInfoValue}"));

		string ValidAddInfo
		{
			get
			{
				if (validAddInfo == null)
				{
					validAddInfo = $"*GREETING=HELLO*{GetAddInfoString(ValidAddInfoMapping)}";
				}
				return validAddInfo;
			}
		}
		string validAddInfo;

		Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)> ValidAddInfoMapping
		{
			get
			{
				if (validAddInfoMapping == null)
				{
					validAddInfoMapping = new Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)>();
					validAddInfoMapping.Add(DummyBizoSchema.Z0_AnotherDecimal, (new Decimal(1234567890123.456).ToString(), 1234567890123.46m));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_BitFalse, ("Y", true));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Bool, ("Y", "Y"));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Byte, (byte.MaxValue.ToString(), byte.MaxValue));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Code, ("12AD&#", "12AD&"));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Date, ("2021-10-21 15:31:45.997", new DateTime(2021, 10, 21, 15, 31, 45, 997)));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_DateOnly, ("2021-10-21 15:31:45.997 +10:00", new DateTime(2021, 10, 21)));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_DateTimeOffset, ("2021-10-21 15:31:45.997 +10:00", new DateTimeOffset(2021, 10, 21, 15, 31, 45, 997, TimeSpan.FromHours(10))));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Guid, ("C9F228AB-0F39-4892-9672-096F35B14BE2", new Guid("C9F228AB-0F39-4892-9672-096F35B14BE2")));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Long, (long.MaxValue.ToString(), long.MaxValue));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Money, ("15032.34", 15032.34m));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Number, (int.MaxValue.ToString(), int.MaxValue));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_Short, (short.MaxValue.ToString(), short.MaxValue));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_SmallDateTime, ("2021-10-21 15:31:45.997", new DateTime(2021, 10, 21, 15, 32, 0)));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_SparseNumber, (int.MinValue.ToString(), int.MinValue));
					validAddInfoMapping.Add(DummyBizoSchema.Z0_SparseGuid, (OrgPK.ToString(), OrgPK));
				}
				return validAddInfoMapping;
			}
		}
		Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)> validAddInfoMapping;

		string InvalidAddInfo
		{
			get
			{
				if (invalidAddInfo == null)
				{
					invalidAddInfo = $"*GREETING=HELLO*{GetAddInfoString(InvalidAddInfoMapping)}";
				}
				return invalidAddInfo;
			}
		}
		string invalidAddInfo;

		Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)> InvalidAddInfoMapping
		{
			get
			{
				if (invalidAddInfoMapping == null)
				{
					invalidAddInfoMapping = new Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)>();
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_AnotherDecimal, (decimal.MaxValue.ToString() + "1", 0m));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_BitFalse, ("J", false));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Bool, ("J", "N"));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Byte, (byte.MaxValue.ToString() + "1", (byte)0));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Code, ("12AD&#", "12AD&"));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Date, ("2021-1A-21 15:31:45.997 +10:00", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_DateOnly, ("2021-1A-21 15:31:45.997 +10:00", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_DateTimeOffset, ("2021-1A-21 15:31:45.997 +10:00", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Guid, ("C9F228AB-0F3ZZZZ-4892-9672-096F35B14BE2", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Long, (long.MaxValue.ToString() + "1", (long)0));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Money, ("1503A.34", 0m));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Number, (int.MaxValue.ToString() + "1", 0));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_Short, (short.MaxValue.ToString() + "1", (short)0));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_SmallDateTime, ("2021-1A-21 15:31:45.997 +10:00", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_SparseNumber, (int.MinValue.ToString() + "1", DBNull.Value));
					invalidAddInfoMapping.Add(DummyBizoSchema.Z0_SparseGuid, (Guid.NewGuid().ToString(), DBNull.Value));
				}
				return invalidAddInfoMapping;
			}
		}
		Dictionary<SchemaColumn, (string addInfoValue, object expectedResult)> invalidAddInfoMapping;
		Guid Source1PK;
		Guid Source3PKNotInList;
		Guid Source4PKNoMatchAddInfo;
		Guid Source5PKExisting;
		Guid Source9PKInvalidData;
		Guid OrgPK = Guid.NewGuid();
	}

	class CopyAddInfoToRealColumnTestClass : CopyAddInfoToRealColumn
	{
		public override string UserDescription => $"Copy AddInfo value from JE_AddInfo to CA JobDeclaration (JE_EntryDate, JE_EntrySubmittedDate, JE_EntryAuthorisationDate, JE_WarehouseReleaseDate)";
		public override string AdditionalSourceTableJoin => "INNER JOIN dbo.GlbCompany ON GC_PK = JE_GC AND GC_RN_NKCountryCode = 'CA'";
		public override SchemaStringColumn SourceAddInfoColumn => JobDeclarationSchema.JE_AddInfo;
		protected override IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> GetAddInfoColumnMapping()
		{
			var result = new Dictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>(4);
			result.Add(JobDeclarationSchema.JE_EntryDate, ("EntryDate", null, null));
			result.Add(JobDeclarationSchema.JE_EntrySubmittedDate, ("EntrySubmittedDate", null, null));
			result.Add(JobDeclarationSchema.JE_EntryAuthorisationDate, ("EntryAuthorisationDate", null, null));
			result.Add(JobDeclarationSchema.JE_WarehouseReleaseDate, ("WarehouseReleaseDate", null, null));
			return result;
		}
	}

	[TestedType(typeof(CopyAddInfoToRealColumn))]
	class CopyAddInfoToRealColumnBaseOnlyTest : CopyAddInfoToRealColumnTest<CopyAddInfoToRealColumnTestClass>
	{
		public void TestIgnoredAlreadyProcessedAddInfos()
		{
			PrepareTestData();
			UpdateAlreadyProcessedData("EntryDate", "EntrySubmittedDate");
			RunTransformation();
			AssertTargetData(1, Source1PK, JobDeclarationSchema.PK,
				(DBNull.Value, JobDeclarationSchema.JE_EntryDate),
				(DBNull.Value, JobDeclarationSchema.JE_EntrySubmittedDate),
				(new DateTime(2021, 08, 13), JobDeclarationSchema.JE_EntryAuthorisationDate),
				(new DateTime(2021, 11, 09), JobDeclarationSchema.JE_WarehouseReleaseDate));
			AssertTargetData(4, Source4PKNoMatchAddInfo, JobDeclarationSchema.PK,
				(DBNull.Value, JobDeclarationSchema.JE_EntryDate),
				(DBNull.Value, JobDeclarationSchema.JE_EntrySubmittedDate),
				(DBNull.Value, JobDeclarationSchema.JE_EntryAuthorisationDate),
				(DBNull.Value, JobDeclarationSchema.JE_WarehouseReleaseDate));
			AssertTargetData(5, Source5PKExisting, JobDeclarationSchema.PK,
				(new DateTime(2021, 09, 17), JobDeclarationSchema.JE_EntryDate),
				(new DateTime(2021, 09, 18), JobDeclarationSchema.JE_EntrySubmittedDate),
				(new DateTime(2021, 08, 13), JobDeclarationSchema.JE_EntryAuthorisationDate),
				(new DateTime(2021, 11, 09), JobDeclarationSchema.JE_WarehouseReleaseDate));
			AssertContainsExactElementsInAnyOrder("already processed data", new[] { "EntryDate", "EntrySubmittedDate", "EntryAuthorisationDate", "WarehouseReleaseDate" }, TransformationToTest.GetAlreadyProcessedAddInfos());
		}

		protected override void PrepareTestData()
		{
			ExtProperty.Table.Update(Db.Connection, TransformationToTest.SourceTableSchemaName, TransformationToTest.SourceTableName, TransformationToTest.Identifier, "1");
			UpdateAlreadyProcessedData(Array.Empty<string>());
			var offLineProcessingTableName = TransformationToTest.CreateOffLineProcessingTable(Db.Connection, Db.DatabaseName, true);
			PrepareRelatedTestData();
			addInfoMapping = new Dictionary<Guid, string>(5);
			string addInfo = null;
			(Source1PK, addInfo) = SetupSourceData(1);
			addInfoMapping.Add(Source1PK, addInfo);
			(Source2NotApplicablePK, addInfo) = SetupSourceDataThatWillBeIgnored(2);
			addInfoMapping.Add(Source2NotApplicablePK, addInfo);
			(Source3PKNotInList, addInfo) = SetupSourceData(3);
			addInfoMapping.Add(Source3PKNotInList, addInfo);
			(Source4PKNoMatchAddInfo, addInfo) = SetupSourceDataWithNoMatchAddInfo(4);
			addInfoMapping.Add(Source4PKNoMatchAddInfo, addInfo);
			(Source5PKExisting, addInfo) = SetupSourceData(5);
			addInfoMapping.Add(Source5PKExisting, addInfo);
			SetupExistingTargetData(Source5PKExisting, 5);
			var sqlSetupData = GetInsertOffLineProcessingTableNameScript("@Source1PK", true, 1)
				+ GetInsertOffLineProcessingTableNameScript("@Source2NotApplicablePK", true, 2)
				+ GetInsertOffLineProcessingTableNameScript("@Source4PKNoMatchAddInfo", true, 4)
				+ GetInsertOffLineProcessingTableNameScript("@Source5PKExisting", true, 5);

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@Source1PK", SqlDbType.UniqueIdentifier, Source1PK);
			cmd.AddParameter("@Source4PKNoMatchAddInfo", SqlDbType.UniqueIdentifier, Source4PKNoMatchAddInfo);
			cmd.AddParameter("@Source5PKExisting", SqlDbType.UniqueIdentifier, Source5PKExisting);
			cmd.AddParameter("@Source2NotApplicablePK", SqlDbType.UniqueIdentifier, Source2NotApplicablePK);
			cmd.ExecuteNonQuery();
		}
		Dictionary<Guid, string> addInfoMapping;

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var sourceAddInfoColumn = TransformationToTest.SourceAddInfoColumn;
				AssertSourceAddInfo(1, Source1PK, sourceAddInfoColumn, addInfoMapping[Source1PK]);
				AssertTargetData(1, Source1PK, JobDeclarationSchema.PK,
					(new DateTime(2021, 10, 15), JobDeclarationSchema.JE_EntryDate),
					(new DateTime(2021, 09, 14, 23, 46, 0), JobDeclarationSchema.JE_EntrySubmittedDate),
					(new DateTime(2021, 08, 13), JobDeclarationSchema.JE_EntryAuthorisationDate),
					(new DateTime(2021, 11, 09), JobDeclarationSchema.JE_WarehouseReleaseDate));
				AssertSourceAddInfo(2, Source2NotApplicablePK, sourceAddInfoColumn, addInfoMapping[Source2NotApplicablePK]);
				AssertTargetData(2, Source2NotApplicablePK, JobDeclarationSchema.PK,
					(DBNull.Value, JobDeclarationSchema.JE_EntryDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntrySubmittedDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntryAuthorisationDate),
					(DBNull.Value, JobDeclarationSchema.JE_WarehouseReleaseDate));
				AssertSourceAddInfo(3, Source3PKNotInList, sourceAddInfoColumn, addInfoMapping[Source3PKNotInList]);
				AssertTargetData(3, Source3PKNotInList, JobDeclarationSchema.PK,
					(DBNull.Value, JobDeclarationSchema.JE_EntryDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntrySubmittedDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntryAuthorisationDate),
					(DBNull.Value, JobDeclarationSchema.JE_WarehouseReleaseDate));
				AssertSourceAddInfo(4, Source4PKNoMatchAddInfo, sourceAddInfoColumn, addInfoMapping[Source4PKNoMatchAddInfo]);
				AssertTargetData(4, Source4PKNoMatchAddInfo, JobDeclarationSchema.PK,
					(DBNull.Value, JobDeclarationSchema.JE_EntryDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntrySubmittedDate),
					(DBNull.Value, JobDeclarationSchema.JE_EntryAuthorisationDate),
					(DBNull.Value, JobDeclarationSchema.JE_WarehouseReleaseDate));
				AssertSourceAddInfo(5, Source5PKExisting, sourceAddInfoColumn, addInfoMapping[Source5PKExisting]);
				AssertTargetData(5, Source5PKExisting, JobDeclarationSchema.PK,
					(new DateTime(2021, 10, 15), JobDeclarationSchema.JE_EntryDate),
					(new DateTime(2021, 09, 14, 23, 46, 0), JobDeclarationSchema.JE_EntrySubmittedDate),
					(new DateTime(2021, 08, 13), JobDeclarationSchema.JE_EntryAuthorisationDate),
					(new DateTime(2021, 11, 09), JobDeclarationSchema.JE_WarehouseReleaseDate));
				AssertEquals($"{TransformationToTest.OffLineProcessingTableName} should have been dropped", false, DbObjectCreator.TableExists(Db.Connection, TransformationToTest.OffLineProcessingTableName));
				AssertNull($"{TransformationToTest.Identifier} extension should have been deleted", ExtProperty.Table.Select(Db.Connection, TransformationToTest.SourceTableSchemaName, TransformationToTest.SourceTableName, TransformationToTest.Identifier));
				AssertContainsExactElementsInAnyOrder("already processed data", new[] { "EntryDate", "EntrySubmittedDate", "EntryAuthorisationDate", "WarehouseReleaseDate" }, TransformationToTest.GetAlreadyProcessedAddInfos());
			});
		}

		protected override string ReasonNotToBeMapped => "Test class";

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyAddInfoToRealColumnTestClass();

		void SetupExistingTargetData(Guid sourcePK, int reference)
		{
			var sqlSetupData = @"
UPDATE dbo.JobDeclaration
SET JE_EntryDate = '2021-09-17',
	JE_EntrySubmittedDate = '2021-09-18',
	JE_EntryAuthorisationDate = '2021-09-19',
	JE_WarehouseReleaseDate = '2021-09-20',
	JE_SystemLastEditTimeUtc = GetUtcDate(),
	JE_SystemLastEditUser = '~BP'
WHERE JE_PK = @SourcePK AND JE_ClusterKey = @ClusterKey
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@ClusterKey", SqlDbType.VarChar, reference);
			cmd.ExecuteNonQuery();
		}

		(Guid, string) SetupSourceDataThatWillBeIgnored(int reference)
		{
			var sourcePK = Guid.NewGuid();
			var declarationReference = "DECCA" + reference.ToString("D2");
			var addInfo = "EntryDate=2021-10-15*EntrySubmittedDate=2021-09-14 23:45:30*GREETING=HELLO*EntryAuthorisationDate=2021-08-13*WarehouseReleaseDate=2021-11-09";
			var sqlSetupData = @"
INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GC, JE_GB, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_AddInfo, JE_MessageType)
VALUES (@SourcePK, 'US', @DeclarationReference, @CompanyUS2PK, @BranchUS2PK, @ClusterKey, '2021-10-15 14:56:00', 'BOB', '2021-10-16 09:35:00', 'JOE', @AddInfo, 'IMP');
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@DeclarationReference", SqlDbType.VarChar, declarationReference);
			cmd.AddParameter("@CompanyUS2PK", SqlDbType.UniqueIdentifier, CompanyUS2PK);
			cmd.AddParameter("@BranchUS2PK", SqlDbType.UniqueIdentifier, BranchUS2PK);
			cmd.AddParameter("@ClusterKey", SqlDbType.Int, reference);
			cmd.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
			cmd.ExecuteNonQuery();
			return (sourcePK, addInfo);
		}

		(Guid, string) SetupSourceDataWithNoMatchAddInfo(int reference)
		{
			var sourcePK = Guid.NewGuid();
			var declarationReference = "DECCA" + reference.ToString("D2");
			var addInfo = "GREETING=HELLO";
			var sqlSetupData = @"
INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GC, JE_GB, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_AddInfo, JE_MessageType)
VALUES (@SourcePK, 'CA', @DeclarationReference, @CompanyCA1PK, @BranchCA1PK, @ClusterKey, '2021-10-15 14:56:00', 'BOB', '2021-10-16 09:35:00', 'JOE', @AddInfo, 'IMP');
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@DeclarationReference", SqlDbType.VarChar, declarationReference);
			cmd.AddParameter("@CompanyCA1PK", SqlDbType.UniqueIdentifier, CompanyCA1PK);
			cmd.AddParameter("@BranchCA1PK", SqlDbType.UniqueIdentifier, BranchCA1PK);
			cmd.AddParameter("@ClusterKey", SqlDbType.Int, reference);
			cmd.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
			cmd.ExecuteNonQuery();
			return (sourcePK, addInfo);
		}

		(Guid, string) SetupSourceData(int reference)
		{
			var companyPK = CompanyCA1PK;
			var branchPK = BranchCA1PK;
			if (reference % 2 == 0)
			{
				companyPK = CompanyCA3PK;
				branchPK = BranchCA3PK;
			}
			var sourcePK = Guid.NewGuid();
			var declarationReference = "DECCA" + reference.ToString("D2");
			var addInfo = "EntryDate=2021-10-15*EntrySubmittedDate=2021-09-14 23:45:30*GREETING=HELLO*EntryAuthorisationDate=2021-08-13*WarehouseReleaseDate=2021-11-09";
			var sqlSetupData = @"
INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GC, JE_GB, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_AddInfo, JE_MessageType)
VALUES (@SourcePK, 'CA', @DeclarationReference, @CompanyPK, @BranchPK, @ClusterKey, '2021-10-15 14:56:00', 'BOB', '2021-10-16 09:35:00', 'JOE', @AddInfo, 'IMP');
";

			var cmd = Db.Connection.Command(sqlSetupData);
			cmd.AddParameter("@SourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			cmd.AddParameter("@DeclarationReference", SqlDbType.VarChar, declarationReference);
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
			cmd.AddParameter("@ClusterKey", SqlDbType.Int, reference);
			cmd.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
			cmd.ExecuteNonQuery();
			return (sourcePK, addInfo);
		}

		void PrepareRelatedTestData()
		{
			var sqlSetupData = @"
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyCA1PK, 'CA1', 'CA company1', 'CA', 'CAD');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyUS2PK, 'US2', 'US company2', 'US', 'USD');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyCA3PK, 'CA3', 'CA company3', 'CA', 'CAD');
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@BranchCA1PK, @CompanyCA1PK, 'CA1');
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@BranchUS2PK, @CompanyUS2PK, 'US2');
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@BranchCA3PK, @CompanyCA3PK, 'CA3');
";

			var cmd = Db.Connection.Command(sqlSetupData);
			CompanyCA1PK = Guid.NewGuid();
			CompanyUS2PK = Guid.NewGuid();
			CompanyCA3PK = Guid.NewGuid();
			BranchCA1PK = Guid.NewGuid();
			BranchUS2PK = Guid.NewGuid();
			BranchCA3PK = Guid.NewGuid();
			BranchCA3PK = Guid.NewGuid();
			cmd.AddParameter("@CompanyCA1PK", SqlDbType.UniqueIdentifier, CompanyCA1PK);
			cmd.AddParameter("@CompanyUS2PK", SqlDbType.UniqueIdentifier, CompanyUS2PK);
			cmd.AddParameter("@CompanyCA3PK", SqlDbType.UniqueIdentifier, CompanyCA3PK);
			cmd.AddParameter("@BranchCA1PK", SqlDbType.UniqueIdentifier, BranchCA1PK);
			cmd.AddParameter("@BranchUS2PK", SqlDbType.UniqueIdentifier, BranchUS2PK);
			cmd.AddParameter("@BranchCA3PK", SqlDbType.UniqueIdentifier, BranchCA3PK);
			cmd.ExecuteNonQuery();
		}

		Guid CompanyCA1PK;
		Guid CompanyUS2PK;
		Guid CompanyCA3PK;
		Guid BranchCA1PK;
		Guid BranchUS2PK;
		Guid BranchCA3PK;
		Guid Source1PK;
		Guid Source2NotApplicablePK;
		Guid Source3PKNotInList;
		Guid Source4PKNoMatchAddInfo;
		Guid Source5PKExisting;
	}

	[TestsSubclassesOf(typeof(CopyAddInfoToRealColumn))]
	public abstract class CopyAddInfoToRealColumnTest<T> : DataTransformationTestCase
		where T : CopyAddInfoToRealColumn
	{
		protected new T TransformationToTest => (T)base.TransformationToTest;

		protected string GetInsertOffLineProcessingTableNameScript(string valueName, bool sourceHasClusterKeyColumn, int clusterKey)
		{
			var clusterKeyColumn = sourceHasClusterKeyColumn ? ", ClusterKey" : "";
			var clusterKeyValue = sourceHasClusterKeyColumn ? $", {clusterKey}" : "";
			return $@"INSERT {TransformationToTest.OffLineProcessingTableName} (PK{clusterKeyColumn}) VALUES ({valueName}{clusterKeyValue});
";
		}

		protected void MarkAllAddInfoPropertiesAsAlreadyProcessedExcluding(params string[] excludedAddInfoPropertyNames)
		{
			var excludedNames = excludedAddInfoPropertyNames.ToHashSet();
			var addInfoColumnMapping = (IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>)TransformationToTest.GetType().GetMethod("GetAddInfoColumnMapping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(TransformationToTest, Array.Empty<object>());

			var data = string.Join(System.Environment.NewLine, addInfoColumnMapping.Values.Select(x => x.AddInfoPropertyName).Where(x => !excludedNames.Contains(x)));
			TransformationToTest.UpdateAlreadyProcessedData(data);
		}

		protected void UpdateAlreadyProcessedData(params string[] addInfoPropertyNames)
		{
			var data = string.Join(System.Environment.NewLine, addInfoPropertyNames);
			TransformationToTest.UpdateAlreadyProcessedData(data);
		}

		protected void AssertTargetData(int reference, Guid sourcePK, SchemaGuidColumn foreignKeyColumn, params (object data, SchemaColumn column)[] dataAndColumns)
		{
			var sqlText = $"SELECT {string.Join(", ", dataAndColumns.Select(x => x.column.Name))} FROM {foreignKeyColumn.TableName} WHERE {foreignKeyColumn.Name} = @sourcePK";
			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@sourcePK", SqlDbType.UniqueIdentifier, sourcePK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					foreach ((object data, SchemaColumn column) in dataAndColumns)
					{
						AssertEquals($"{reference} - {column.Name}", data, reader[column.Name]);
					}
				}
			}
		}

		protected void AssertSourceAddInfo(int reference, Guid sourcePK, SchemaStringColumn addInfoColumn, string addInfo)
		{
			var sqlText = $"SELECT {addInfoColumn.Name} FROM {addInfoColumn.TableName} WHERE {addInfoColumn.TableSchema.PK.Name} = @sourcePK";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@sourcePK", SqlDbType.UniqueIdentifier, sourcePK);
			AssertEquals($"{reference} - {addInfoColumn.Name}", addInfo, cmd.ExecuteScalar());
		}

		protected sealed override bool TransformationTestShouldBeRunAgainstNewInstance => true;
	}
}
