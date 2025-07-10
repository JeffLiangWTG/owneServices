using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	abstract class ConstraintBase_ParentTableCodeTest<T> : DataTransformationTestCase where T : DataTransformation, new()
	{
		protected ConstraintBase_ParentTableCodeTest()
		{
			tableName = TableName;
			tablePrefix = TablePrefix;
			parentIdColumn = $"{tablePrefix}_{ParentIdColumnWithoutPrefix}";
			parentTableCodeColumn = $"{tablePrefix}_{ParentTableCodeColumnWithoutPrefix}";
			constraintName = $"Constraint_{parentTableCodeColumn}{(UseNoCheck ? "_NoCheck" : "")}";

			parentPrefixes = SupportedParentPrefixes;
			parentPrefixesWithBlank = AllowEmptyParentTableCode ? new[] { string.Empty }.Concat(parentPrefixes).ToArray() : parentPrefixes;
			prefixList = string.Join(",", parentPrefixesWithBlank.Select(p => $"'{p}'"));
			prefixCount = parentPrefixes.Length + (AllowEmptyParentTableCode ? 1 : 0);

			index = CreateIndex();
		}

		readonly string tableName;
		readonly string tablePrefix;
		readonly string parentIdColumn;
		readonly string parentTableCodeColumn;
		readonly string constraintName;
		readonly string[] parentPrefixes;
		readonly string[] parentPrefixesWithBlank;
		readonly string prefixList;
		readonly int prefixCount;
		readonly string index;

		protected abstract string TableName { get; }
		protected abstract string TablePrefix { get; }
		protected abstract string[] SupportedParentPrefixes { get; }
		protected abstract bool UseNoCheck { get; }
		protected abstract bool AllowEmptyParentTableCode { get; }

		protected virtual string[] ExpectedIndexIncludeColumns { get; } = Array.Empty<string>();
		protected virtual string ParentIdColumnWithoutPrefix => "ParentID";
		protected virtual string ParentTableCodeColumnWithoutPrefix => "ParentTableCode";
		protected virtual (string column, string tablePrefix)[] ForeignKeyColumns { get; } = Array.Empty<(string, string)>();
		protected virtual Dictionary<string, string> AdditionalColumnsAndValuesForChild => new Dictionary<string, string>();

		string CreateIndex()
		{
			var indexFilter = string.Join(" AND ", parentPrefixesWithBlank.Select(x => $"[{parentTableCodeColumn}]<>'{x}'"));
			var include = ExpectedIndexIncludeColumns.Length > 0 ? $" INCLUDE ({string.Join(", ", ExpectedIndexIncludeColumns)})" : string.Empty;
			var indexDetail = $"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column {parentTableCodeColumn}_1] ON [dbo].[{tableName}] ([{parentIdColumn}]){include} WHERE ({indexFilter}) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";

			return indexDetail;
		}

		public override string[] expectedIndex => new[] { index };

		public void TestTransformationSection()
		{
			var methods = typeof(T).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(method => method.IsVirtual && method.IsFamily).ToArray();

			AssertGreaterThan(methods.Length, 0);

			foreach (var method in methods)
			{
				switch (method.Name)
				{
					case "OfflinePreUpgradeTransform":
					case "OfflinePostUpgradeTransform":
						Assert("OfflinePostUpgradeTransform should implement ITransformationIndexProvider", typeof(ITransformationIndexProvider).IsAssignableFrom(typeof(T)));
						break;
					case "OnlinePostUpgradeTransform":
						Assert("OnlinePostUpgradeTransform should use NoCheck", UseNoCheck);
						break;
					case "OnlinePreUpgradeTransform":
						break;
					default:
						Assert($"Unrecognized method: {method.Name}", false);
						break;
				}
			}
		}

		protected override void AssertPreConditions()
		{
			CombineAssertions("PRE-REQs", () =>
			{
				var sqlText = $"SELECT count(distinct {parentTableCodeColumn}) FROM dbo.{tableName} WHERE {parentTableCodeColumn} NOT IN ({prefixList})";
				AssertEquals("Invalid records should exist which will later be deleted", 1, (int)Db.Connection.ExecuteScalar(sqlText));

				sqlText = $"SELECT count(distinct {parentTableCodeColumn}) FROM dbo.{tableName} WHERE {parentTableCodeColumn} IN ({prefixList})";
				AssertEquals("Valid data should exist for each prefix", prefixCount, (int)Db.Connection.ExecuteScalar(sqlText));
			});
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("Results", () =>
			{
				var sqlText = $"SELECT count(distinct {parentTableCodeColumn}) FROM dbo.{tableName} WHERE {parentTableCodeColumn} NOT IN ({prefixList})";
				AssertEquals("Invalid data should be deleted", 0, Db.Connection.ExecuteScalar(sqlText));

				sqlText = $"SELECT count(distinct {parentTableCodeColumn}) FROM dbo.{tableName} WHERE {parentTableCodeColumn} IN ({prefixList})";
				AssertEquals("Valid data should be retained", prefixCount, Db.Connection.ExecuteScalar(sqlText));
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = CreateNewTransformationInstance();
			transform.Initialise(null, manager);
			return transform;
		}

		protected virtual T CreateNewTransformationInstance()
		{
			return new T();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(tableName, constraintName);

			var sqlText = new StringBuilder();

			var combos = AddParentTables(sqlText);
			var requiredColumns = AddAdditionalForeignKeyTables(sqlText);
			foreach(var kvp in AdditionalColumnsAndValuesForChild)
			{
				requiredColumns.Add(kvp.Key, kvp.Value);
			}
			AddChildTables(sqlText, combos, requiredColumns);

			var script = sqlText.ToString();
			Db.Connection.ExecuteNonQuery(script);
		}

		List<(string prefix, string pkVar)> AddParentTables(StringBuilder sqlText)
		{
			var combos = new List<(string, string)>();

			foreach (var prefix in parentPrefixes)
			{
				var builder = GetTable(prefix);
				AddTableToScript(sqlText, builder);
				combos.Add((builder.Prefix, $"@{builder.TableName}PK"));
			}

			return combos;
		}

		Dictionary<string, string> AddAdditionalForeignKeyTables(StringBuilder sqlText)
		{
			var foreignKeys = new Dictionary<string, string>();

			foreach (var foreignKeyColumn in ForeignKeyColumns)
			{
				var builder = GetTable(foreignKeyColumn.tablePrefix);
				AddTableToScript(sqlText, builder);
				foreignKeys.Add(foreignKeyColumn.column, $"@{builder.TableName}PK");
			}

			return foreignKeys;
		}

		void AddTableToScript(StringBuilder sqlText, TableBuilder builder)
		{
			if (builder != null && !builder.Scripted)
			{
				foreach (var depPrefix in builder.Dependencies)
				{
					AddTableToScript(sqlText, depPrefix);
				}

				var insertScript = builder.Script;
				if (!string.IsNullOrEmpty(insertScript))
				{
					sqlText.AppendLine($"DECLARE @{builder.TableName}PK UNIQUEIDENTIFIER = NEWID();");
					sqlText.AppendLine(insertScript);
				}
			}
		}

		protected void AddTableToScript(StringBuilder sqlText, string prefix)
		{
			var table = GetTable(prefix);
			AddTableToScript(sqlText, table);
		}

		protected virtual void AddChildTables(StringBuilder sqlText, List<(string prefix, string pkVar)> combos, Dictionary<string, string> requiredColumns)
		{
			var rowIndex = 0;

			foreach (var combo in combos)
			{
				AppendInsertScript(combo.prefix, combo.pkVar, sqlText, requiredColumns, rowIndex++);
			}

			// Invalid
			if (AllowEmptyParentTableCode)
			{
				AppendInsertScript(string.Empty, "NEWID()", sqlText, requiredColumns, rowIndex++);
			}

			AppendInsertScript("!!", "NEWID()", sqlText, requiredColumns, rowIndex++);
		}

		void AppendInsertScript(string parentTableCode, string parentId, StringBuilder sqlText, Dictionary<string, string> requiredColumns, int rowIndex)
		{
			sqlText.AppendLine($"DECLARE @{tablePrefix}PK{rowIndex} UNIQUEIDENTIFIER = NEWID();");
			var columnValues = new Dictionary<string, string>(requiredColumns)
			{
				{ $"{tablePrefix}_PK", $"@{tablePrefix}PK{rowIndex}" },
				{ parentTableCodeColumn, $"'{parentTableCode}'" },
				{ parentIdColumn, parentId },
				{ $"{tablePrefix}_SystemCreateTimeUtc", "GetUtcDate()" },
				{ $"{tablePrefix}_SystemCreateUser", "'~BP'" },
				{ $"{tablePrefix}_SystemLastEditTimeUtc", "GetUtcDate()" },
				{ $"{tablePrefix}_SystemLastEditUser", "'~BP'" }
			};

			AppendInsertScript(sqlText, columnValues);
		}

		protected virtual void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			sqlText.AppendLine($"INSERT INTO dbo.{tableName}({string.Join(", ", columnValues.Keys)}) VALUES ({string.Join(", ", columnValues.Values)})");
		}

		class TableBuilder
		{
			public string Prefix { get; }
			public string TableName { get; }
			public string[] Dependencies { get; }
			public bool Scripted { get; private set; }

			public TableBuilder(string prefix, string tableName, string[] dependencies, string script)
			{
				Prefix = prefix;
				TableName = tableName;
				Dependencies = dependencies ?? Array.Empty<string>();
				this.script = script;
			}

			readonly string script;

			public string Script
			{
				get
				{
					var result = string.Empty;

					if (!Scripted)
					{
						result = script;
						Scripted = true;
					}

					return result;
				}
			}
		}

		TableBuilder GetTable(string prefix) => Tables.TryGetValue(prefix, out var table) ? table : throw new InvalidOperationException($"TableBuilder for prefix '{prefix}' not found in ConstraintBase_ParentTableCodeTest.GetTables()");

		Dictionary<string, TableBuilder> Tables => tables ??= GetTables().ToDictionary(t => t.Prefix, t => t);
		Dictionary<string, TableBuilder> tables;

		static TableBuilder[] GetTables() => new[]
		{
			new TableBuilder("ABL", "AsycudaBill", new [] { "AMA" }, "INSERT INTO dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_ClusterKey, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser) VALUES (@AsycudaBillPK, @AsycudaManifestHeaderPK, 12345, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("AC", "AccChargeCode", null, "INSERT INTO dbo.AccChargeCode (AC_PK, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES(@AccChargeCodePK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("AMA", "AsycudaManifestHeader", new [] { "GB" }, "INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_GB, AMA_JobReference, AMA_ClusterKey, AMA_RN_NKCountry, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser) VALUES (@AsycudaManifestHeaderPK, @GlbBranchPK, 'Manifest001', 12345, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("APA", "AsycudaPack", new [] { "ABL" }, "INSERT INTO dbo.AsycudaPack (APA_PK, APA_ClusterKey, APA_ABL_Bill, APA_SystemCreateTimeUtc, APA_SystemCreateUser, APA_SystemLastEditTimeUtc, APA_SystemLastEditUser) VALUES(@AsycudaPackPK, 12345, @AsycudaBillPK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("API", "AsycudaPackedItem", new [] { "ABL" }, "INSERT INTO dbo.AsycudaPackedItem (API_PK, API_ClusterKey, API_ABL_Bill, API_SystemCreateTimeUtc, API_SystemCreateUser, API_SystemLastEditTimeUtc, API_SystemLastEditUser) VALUES(@AsycudaPackedItemPK, 12345, @AsycudaBillPK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("ASR", "AsycudaBillScreening", new [] { "ABL" }, "INSERT INTO dbo.AsycudaBillScreening (ASR_PK, ASR_ABL, ASR_ClusterKey, ASR_SystemCreateTimeUtc, ASR_SystemCreateUser, ASR_SystemLastEditTimeUtc, ASR_SystemLastEditUser) VALUES(@AsycudaBillScreeningPK, @AsycudaBillPK, 12345, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),

			new TableBuilder("B0", "CusInBondBill", new [] { "BH" }, "INSERT INTO dbo.CusInBondBill(B0_PK, B0_BH, B0_ShipmentType, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser) VALUES (@CusInBondBillPK, @CusInbondHeaderPK, 'IMP', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("B2", "CusStatementHeader", new [] { "GC" }, "INSERT INTO dbo.CusStatementHeader(B2_PK, B2_GC, B2_SystemCreateTimeUtc, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) VALUES (@CusStatementHeaderPK, @GlbCompanyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("B3", "CusStatementLine", new [] { "B2" }, "INSERT INTO dbo.CusStatementLine(B3_PK, B3_B2, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser) VALUES (@CusStatementLinePK, @CusStatementHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("B5", "CusInvPack", new [] { "JE" }, "INSERT INTO dbo.CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser) VALUES (@CusInvPackPK, @JobDeclarationPK, 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("B7", "CusAddInfo", new [] { "JE" }, "INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentId, B7_ParentTableCode, B7_Type, B7_SystemCreateTimeUtc, B7_SystemCreateUser, B7_SystemLastEditTimeUtc, B7_SystemLastEditUser) VALUES (@CusAddInfoPK, @JobDeclarationPK, 'JE', 'GBA', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("B9", "CusInBondMoveDetail", new [] { "B0", "BM" }, "INSERT INTO dbo.CusInBondMoveDetail(B9_PK, B9_B0, B9_BM, B9_SystemCreateTimeUtc, B9_SystemCreateUser, B9_SystemLastEditTimeUtc, B9_SystemLastEditUser) VALUES (@CusInBondMoveDetailPK, @CusInBondBillPK, @CusInBondMoveHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BC", "CusInBondContainer", new [] { "B0" }, "INSERT INTO dbo.CusInBondContainer(BC_PK, BC_ParentID, BC_ParentTableCode, BC_DataModel, BC_SystemCreateTimeUtc, BC_SystemCreateUser, BC_SystemLastEditTimeUtc, BC_SystemLastEditUser) VALUES (@CusInBondContainerPK, @CusInBondBillPK, 'B0', 'AISPT', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BD", "CusSeaManOBLDetail", new [] { "BO" }, "INSERT INTO dbo.CusSeaManOBLDetail(BD_PK, BD_BO, BD_SystemCreateTimeUtc, BD_SystemCreateUser, BD_SystemLastEditTimeUtc, BD_SystemLastEditUser) VALUES (@CusSeaManOBLDetailPK, @CusSeaManOBLHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BH", "CusInbondHeader", new [] { "GB" }, "INSERT INTO dbo.CusInbondHeader(BH_PK, BH_JobReference, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_ApplicationCode) VALUES (@CusInbondHeaderPK, 'REF123', @GlbBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'AMS');"),
			new TableBuilder("BJ", "CusInBondEquipment", new [] { "BH" }, "INSERT INTO dbo.CusInBondEquipment(BJ_PK, BJ_BH_Header, BJ_SystemCreateTimeUtc, BJ_SystemCreateUser, BJ_SystemLastEditTimeUtc, BJ_SystemLastEditUser) VALUES (@CusInBondEquipmentPK, @CusInBondHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BK", "CusSeal", new [] { "BH" }, "INSERT INTO dbo.CusSeal(BK_PK, BK_ParentId, BK_ParentTableCode, BK_SealNumber, BK_SystemCreateTimeUtc, BK_SystemCreateUser, BK_SystemLastEditTimeUtc, BK_SystemLastEditUser) VALUES (@CusSealPK, @CusInBondHeaderPK, 'BH', 'S1234', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BM", "CusInBondMoveHeader",  new [] { "BH" }, "INSERT INTO dbo.CusInBondMoveHeader(BM_PK, BM_BH, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser) VALUES (@CusInBondMoveHeaderPK, @CusInbondHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BN", "CusInBondEvent",  new [] { "BH" }, "INSERT INTO dbo.CusInBondEvent(BN_PK, BN_BH, BN_Type, BN_SystemCreateTimeUtc, BN_SystemCreateUser, BN_SystemLastEditTimeUtc, BN_SystemLastEditUser) VALUES (@CusInBondEventPK, @CusInBondHeaderPK, 'SEL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BO", "CusSeaManOBLHeader", new [] { "BT" }, "INSERT INTO dbo.CusSeaManOBLHeader(BO_PK, BO_BT, BO_SystemCreateTimeUtc, BO_SystemCreateUser, BO_SystemLastEditTimeUtc, BO_SystemLastEditUser) VALUES (@CusSeaManOBLHeaderPK, @CusSeaManTranHeadPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BP", "CusCAeMHMaster",  new [] { "GB" }, "INSERT INTO dbo.CusCAeMHMaster (BP_PK, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser) VALUES (@CusCAeMHMasterPK, @GlbBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BQ", "CusCAeMHContainer",  new [] { "BP" }, "INSERT INTO dbo.CusCAeMHContainer (BQ_PK, BQ_BP_Master, BQ_SystemCreateTimeUtc, BQ_SystemCreateUser, BQ_SystemLastEditTimeUtc, BQ_SystemLastEditUser) VALUES (@CusCAeMHContainerPK, @CusCAeMHMasterPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BT", "CusSeaManTranHead", null, "INSERT INTO dbo.CusSeaManTranHead(BT_PK, BT_SystemCreateTimeUtc, BT_SystemCreateUser, BT_SystemLastEditTimeUtc, BT_SystemLastEditUser) VALUES (@CusSeaManTranHeadPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("BY", "CusInBondCargoDesc", new [] { "BH" }, "INSERT INTO dbo.CusInBondCargoDesc(BY_PK, BY_ParentID, BY_ParentTableCode, BY_SystemCreateTimeUtc, BY_SystemCreateUser, BY_SystemLastEditTimeUtc, BY_SystemLastEditUser) VALUES (@CusInBondCargoDescPK, @CusInBondHeaderPK, 'BH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("C4", "CusUnderbond", null, "INSERT INTO dbo.CusUnderbond(C4_PK, C4_ApplicationCode, C4_SendersMessageReference, C4_SystemCreateTimeUtc, C4_SystemCreateUser, C4_SystemLastEditTimeUtc, C4_SystemLastEditUser) VALUES (@CusUnderbondPK, 'XXX', 'C4Ref1234567', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("C5", "CusOutturn", null, "INSERT INTO dbo.CusOutturn(C5_PK, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) VALUES (@CusOutturnPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CA", "CusSCAHouse", null, "INSERT INTO dbo.CusSCAHouse(CA_PK, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES (@CusSCAHousePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CB", "CusSCAOceanBill", null, "INSERT INTO dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES (@CusSCAOceanBillPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CC", "CusClassification", null, "INSERT INTO dbo.CusClassification(CC_PK, CC_ClassificationType, CC_Description, CC_LookupCode, CC_RN_NKCountryCode, CC_SystemCreateTimeUtc, CC_SystemCreateUser, CC_SystemLastEditTimeUtc, CC_SystemLastEditUser) VALUES (@CusClassificationPK, 'BTH', 'Classy', 'LUC', 'GB', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CCI", "CusExitConsignmentItem", new [] { "CXC" }, "INSERT INTO dbo.CusExitConsignmentItem(CCI_PK, CCI_CXC_Consignment, CCI_ClusterKey, CCI_LineNumber, CCI_SystemCreateTimeUtc, CCI_SystemCreateUser, CCI_SystemLastEditTimeUtc, CCI_SystemLastEditUser) VALUES (@CusExitConsignmentItemPK, @CusExitConsignmentPK, 12332, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CEH", "CusExitControlHeader", new [] { "JE" }, "INSERT INTO dbo.CusExitControlHeader(CEH_PK, CEH_ParentID, CEH_ParentTableCode, CEH_ReferenceNumber, CEH_SystemCreateTimeUtc, CEH_SystemCreateUser, CEH_SystemLastEditTimeUtc, CEH_SystemLastEditUser) VALUES (@CusExitControlHeaderPK, @JobDeclarationPK, 'JE', 'TEST', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CED", "CusExitDetail", new [] { "CEH" }, "INSERT INTO dbo.CusExitDetail(CED_PK, CED_CEH, CED_SystemCreateTimeUtc, CED_SystemCreateUser, CED_SystemLastEditTimeUtc, CED_SystemLastEditUser) VALUES (@CusExitDetailPK, @CusExitControlHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CEI", "CusEntryInstruction", new [] { "JE" }, "INSERT INTO dbo.CusEntryInstruction(CEI_PK, CEI_ClusterKey, CEI_JE, CEI_DataModel, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@CusEntryInstructionPK, 1234567, @JobDeclarationPK, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("CER", "CusExitReport", new [] { "CXH", "CXC" }, "INSERT INTO dbo.CusExitReport(CER_PK, CER_CXH_Header, CER_CXC_Consignment, CER_ClusterKey, CER_SystemCreateTimeUtc, CER_SystemCreateUser, CER_SystemLastEditTimeUtc, CER_SystemLastEditUser) VALUES (@CusExitReportPK, @CusExitHeaderPK, @CusExitConsignmentPK, 12332, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CG", "CusPartShip", null, "INSERT INTO dbo.CusPartShip(CG_PK, CG_SystemCreateTimeUtc, CG_SystemCreateUser, CG_SystemLastEditTimeUtc, CG_SystemLastEditUser) VALUES (@CusPartShipPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CGC", "CusGoodsCatalog", new [] { "GC", "OH" }, "INSERT INTO dbo.CusGoodsCatalog(CGC_PK, CGC_GC_Company, CGC_OH_Owner, CGC_Type, CGC_CatalogCode, CGC_Description, CGC_SystemCreateTimeUtc, CGC_SystemCreateUser, CGC_SystemLastEditTimeUtc, CGC_SystemLastEditUser) VALUES (@CusGoodsCatalogPK, @GlbCompanyPK, @OrgHeaderPK, 'BTH', 'C001', 'CAT 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CH", "CusEntryHeader", new [] { "JE" }, "INSERT INTO dbo.CusEntryHeader(CH_PK, CH_ClusterKey, CH_JE, CH_DataModel, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (@CusEntryHeaderPK, 1234567, @JobDeclarationPK, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("CI", "CusClassPartPivot", new [] { "OP" }, "INSERT INTO dbo.CusClassPartPivot(CI_PK, CI_OP, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser) VALUES (@CusClassPartPivotPK, @OrgSupplierPartPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CJ", "CusSCADepotContainer", null, "INSERT INTO dbo.CusSCADepotContainer(CJ_PK, CJ_SystemCreateTimeUtc, CJ_SystemCreateUser, CJ_SystemLastEditTimeUtc, CJ_SystemLastEditUser) VALUES (@CusSCADepotContainerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CL", "CusEntryLine", new [] { "CH" }, "INSERT INTO dbo.CusEntryLine(CL_PK, CL_CH, CL_ClusterKey, CL_DataModel, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES (@CusEntryLinePK, @CusEntryHeaderPK, 1234567, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("CM", "CusMAWB", null, "INSERT INTO dbo.CusMAWB(CM_PK, CM_SystemCreateTimeUtc, CM_SystemCreateUser, CM_SystemLastEditTimeUtc, CM_SystemLastEditUser) VALUES (@CusMAWBPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CN", "CusSCAContainer", new [] { "CB" }, "INSERT INTO dbo.CusSCAContainer(CN_PK, CN_CB, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) VALUES (@CusSCAContainerPK, @CusSCAOceanBillPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CO", "CusContainer", new [] { "JE" }, "INSERT INTO dbo.CusContainer(CO_PK, CO_JE, CO_ClusterKey, CO_DataModel, CO_SystemCreateTimeUtc, CO_SystemCreateUser, CO_SystemLastEditTimeUtc, CO_SystemLastEditUser) VALUES (@CusContainerPK, @JobDeclarationPK, 1234567, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("CPH", "CusPermitHeader", new [] { "OH" }, "INSERT INTO dbo.CusPermitHeader(CPH_PK, CPH_OH_PermitHolder, CPH_Type, CPH_Number, CPH_RN_NKCountryCode, CPH_StartDate, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) VALUES (@CusPermitHeaderPK, @OrgHeaderPK, 'CPH', 'Permit123', 'ZA', GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CR", "CusDecHouseContainerPivot", new [] { "CU" }, "INSERT INTO dbo.CusDecHouseContainerPivot(CR_PK, CR_CU_HouseBill, CR_ClusterKey, CR_SystemCreateTimeUtc, CR_SystemCreateUser, CR_SystemLastEditTimeUtc, CR_SystemLastEditUser) VALUES (@CusDecHouseContainerPivotPK, @CusDecHouseBillPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CRD", "CusReconDeclaration", new [] { "GB" }, "INSERT INTO dbo.CusReconDeclaration(CRD_PK, CRD_GB_Branch, CRD_DataModel, CRD_SystemCreateTimeUtc, CRD_SystemCreateUser, CRD_SystemLastEditTimeUtc, CRD_SystemLastEditUser, CRD_JobReferenceNumber, CRD_ApplicationCode) VALUES (@CusReconDeclarationPK, @GlbBranchPK, 'AI', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'CRD123', 'CLS');"),
			new TableBuilder("CS", "CusHAWB", null, "INSERT INTO dbo.CusHAWB(CS_PK, CS_SystemCreateTimeUtc, CS_SystemCreateUser, CS_SystemLastEditTimeUtc, CS_SystemLastEditUser) VALUES (@CusHAWBPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CSI", "CusSupportingInfo", new [] { "JE" }, "INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_DataModel, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser) VALUES (@CusSupportingInfoPK, @JobDeclarationPK, 'JE', 'ABC', 'AI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CU", "CusDecHouseBill", new [] { "JE" }, "INSERT INTO dbo.CusDecHouseBill(CU_PK, CU_JE, CU_ClusterKey, CU_SystemCreateTimeUtc, CU_SystemCreateUser, CU_SystemLastEditTimeUtc, CU_SystemLastEditUser) VALUES (@CusDecHouseBillPK, @JobDeclarationPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CUL", "CusPackingList", new [] { "JE" }, "INSERT INTO dbo.CusPackingList(CUL_PK, CUL_JE, CUL_ClusterKey, CUL_SystemCreateTimeUtc, CUL_SystemCreateUser, CUL_SystemLastEditTimeUtc, CUL_SystemLastEditUser) VALUES (@CusPackingListPK, @JobDeclarationPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CV", "CusSCAPivot", null, "INSERT INTO dbo.CusSCAPivot(CV_PK, CV_SystemCreateTimeUtc, CV_SystemCreateUser, CV_SystemLastEditTimeUtc, CV_SystemLastEditUser) VALUES (@CusSCAPivotPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CW", "CusDecHouseContainerPack", new [] { "CR" }, "INSERT INTO dbo.CusDecHouseContainerPack(CW_PK, CW_CR_HouseContainer, CW_ClusterKey, CW_SystemCreateTimeUtc, CW_SystemCreateUser, CW_SystemLastEditTimeUtc, CW_SystemLastEditUser) VALUES (@CusDecHouseContainerPackPK, @CusDecHouseContainerPivotPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CX", "CusSCADepotHouse", null, "INSERT INTO dbo.CusSCADepotHouse(CX_PK, CX_SystemCreateTimeUtc, CX_SystemCreateUser, CX_SystemLastEditTimeUtc, CX_SystemLastEditUser) VALUES (@CusSCADepotHousePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CXC", "CusExitConsignment", new [] { "CXH" }, "INSERT INTO dbo.CusExitConsignment(CXC_PK, CXC_CXH_Header, CXC_ClusterKey, CXC_SystemCreateTimeUtc, CXC_SystemCreateUser, CXC_SystemLastEditTimeUtc, CXC_SystemLastEditUser) VALUES (@CusExitConsignmentPK, @CusExitHeaderPK, 12332, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CXH", "CusExitHeader", new [] { "GC", "GB" }, "INSERT INTO dbo.CusExitHeader(CXH_PK, CXH_ApplicationCode, CXH_GB_Branch, CXH_GC_Company, CXH_ClusterKey, CXH_JobReference, CXH_SystemCreateTimeUtc, CXH_SystemCreateUser, CXH_SystemLastEditTimeUtc, CXH_SystemLastEditUser) VALUES (@CusExitHeaderPK, 'XIT', @GlbBranchPK, @GlbCompanyPK, 12332, 'EH123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CXI", "CusExitItem", new [] { "CED" }, "INSERT INTO dbo.CusExitItem(CXI_PK, CXI_CED, CXI_Status, CXI_LineNumber, CXI_SystemCreateTimeUtc, CXI_SystemCreateUser, CXI_SystemLastEditTimeUtc, CXI_SystemLastEditUser) VALUES (@CusExitItemPK, @CusExitDetailPK, '310', 123, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("CY", "CusCodeData", new [] { "JE" }, "INSERT INTO dbo.CusCodeData(CY_PK, CY_ParentTableCode, CY_ParentID, CY_SystemCreateTimeUtc, CY_SystemCreateUser, CY_SystemLastEditTimeUtc, CY_SystemLastEditUser) VALUES (@CusCodeDataPK, 'JE', @JobDeclarationPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("DH", "SupplierBookingHeader", new [] { "OA" }, "INSERT INTO dbo.SupplierBookingHeader(DH_PK, DH_OA_Consignor, DH_SystemCreateTimeUtc, DH_SystemCreateUser, DH_SystemLastEditTimeUtc, DH_SystemLastEditUser) VALUES (@SupplierBookingHeaderPK, @OrgAddressPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("DL", "SupplierBookingLine", new [] { "DH" }, "INSERT INTO dbo.SupplierBookingLine(DL_PK, DL_DH_BookingHeader, DL_SystemCreateTimeUtc, DL_SystemCreateUser, DL_SystemLastEditTimeUtc, DL_SystemLastEditUser) VALUES (@SupplierBookingLinePK, @SupplierBookingHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("EE", "QuarantineExDocEstablishmentAndTime", new [] { "QL" }, "INSERT INTO dbo.QuarantineExDocEstablishmentAndTime(EE_PK, EE_QL, EE_ClusterKey, EE_SystemCreateTimeUtc, EE_SystemCreateUser, EE_SystemLastEditTimeUtc, EE_SystemLastEditUser) VALUES (@QuarantineExDocEstablishmentAndTimePK, @QuarantineExDocLinePK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("EM", "EDIMessage", new [] { "JE", "GB", "GE" }, "INSERT INTO dbo.EDIMessage(EM_PK, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES (@EDIMessagePK, 'JE', @JobDeclarationPK, @GlbBranchPK, @GlbDepartmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("ERI", "CusExitReportItem", new [] { "CER" }, "INSERT INTO dbo.CusExitReportItem(ERI_PK, ERI_ClusterKey, ERI_CER_Report, ERI_SystemCreateTimeUtc, ERI_SystemCreateUser, ERI_SystemLastEditTimeUtc, ERI_SystemLastEditUser) VALUES (@CusExitReportItemPK, 12345, @CusExitReportPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("EUS", "EUMemberStateCommunication", null, "INSERT INTO dbo.EUMemberStateCommunication(EUS_PK, EUS_ClusterKey, EUS_ParentId, EUS_ParentTableCode, EUS_Identifier, EUS_Type, EUS_SystemCreateTimeUtc, EUS_SystemCreateUser, EUS_SystemLastEditTimeUtc, EUS_SystemLastEditUser) VALUES (@EUMemberStateCommunicationPK, 1234, @AsycudaManifestHeaderPK, 'AMA', 'REF1', 'YY', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("F2", "RefZonePivot", new [] { "FZ", "RL" }, "INSERT INTO dbo.RefZonePivot (F2_PK, F2_FZ, F2_ParentID, F2_ParentTableCode, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser) VALUES (@RefZonePivotPK, @RefZoneHeaderPK, @RefUNLOCOPK, 'RL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');") ,
			new TableBuilder("FZ", "RefZoneHeader", null, "insert into dbo.RefZoneHeader (FZ_PK, FZ_Code, FZ_ZoneType, FZ_Description, FZ_SystemCreateTimeUtc, FZ_SystemCreateUser, FZ_SystemLastEditTimeUtc, FZ_SystemLastEditUser) VALUES (@RefZoneHeaderPK, 'ZZZZ', 'ALL', 'Zayden Zubin Rakhsh Lola', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("GB", "GlbBranch", new [] { "GC" }, "INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES (@GlbBranchPK, @GlbCompanyPK, 'GBC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("GC", "GlbCompany", null, "INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES (@GlbCompanyPK, 'GCC', 'AI company', 'AI', 'XCD', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("GE", "GlbDepartment", null, "INSERT INTO dbo.GlbDepartment(GE_PK, GE_Code, GE_SystemCreateTimeUtc, GE_SystemCreateUser, GE_SystemLastEditTimeUtc, GE_SystemLastEditUser) VALUES (@GlbDepartmentPK, 'GE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("HCH", "HVLVConsignmentHeader", new [] { "JS" }, "INSERT INTO dbo.HVLVConsignmentHeader(HCH_PK, HCH_JS_Shipment, HCH_ClusterKey, HCH_JobNumber, HCH_SystemCreateTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditTimeUtc, HCH_SystemLastEditUser) VALUES (@HVLVConsignmentHeaderPK, @JobShipmentPK, 5, 'HCJN002', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("HVC", "HVLVConsignment", new [] { "HCH" }, "INSERT INTO dbo.HVLVConsignment(HVC_PK, HVC_HCH_Header, HVC_ClusterKey, HVC_ConsignmentId, HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser) VALUES (@HVLVConsignmentPK, @HVLVConsignmentHeaderPK, 5, 'CID005', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JC", "JobContainer", null, "INSERT INTO dbo.JobContainer(JC_PK, JC_SystemCreateTimeUtc, JC_SystemCreateUser, JC_SystemLastEditTimeUtc, JC_SystemLastEditUser) VALUES (@JobContainerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JD", "JobOrderHeader", ["OA"] ,"INSERT INTO dbo.JobOrderHeader (JD_PK, JD_OrderNumber, JD_OrderNumberSplit, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (@JobOrderHeaderPK, 'X1', 0, @OrgAddressPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"),
			new TableBuilder("JE", "JobDeclaration", new [] { "GC", "GB" }, "INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@JobDeclarationPK, @GlbBranchPK, @GlbCompanyPK, 1234567, 'AI', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JI", "JobComInvoiceLine", new [] { "JZ" }, "INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@JobComInvoiceLinePK, @JobComInvoiceHeaderPK, 1234567, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("JK", "JobConsol", null, "INSERT INTO dbo.JobConsol(JK_PK, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser) VALUES (@JobConsolPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JO", "JobOrderLine", ["JD"], "INSERT INTO dbo.JobOrderLine (JO_PK, JO_JD, JO_SystemCreateTimeUtc, JO_SystemCreateUser, JO_SystemLastEditTimeUtc, JO_SystemLastEditUser) VALUES (@JobOrderLinePK, @JobOrderHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"),
			new TableBuilder("JPB", "JPAFRBills", new [] { "JPH" }, "INSERT INTO dbo.JPAFRBills(JPB_PK, JPB_JPH_Header, JPB_SystemCreateTimeUtc, JPB_SystemCreateUser, JPB_SystemLastEditTimeUtc, JPB_SystemLastEditUser) VALUES (@JPAFRBillsPK, @JPAFRHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JPH", "JPAFRHeader", new [] { "GB" }, "INSERT INTO dbo.JPAFRHeader(JPH_PK, JPH_GB_Branch, JPH_JobReference, JPH_SystemCreateTimeUtc, JPH_SystemCreateUser, JPH_SystemLastEditTimeUtc, JPH_SystemLastEditUser) VALUES (@JPAFRHeaderPK, @GlbBranchPK, 'JPHRef123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JS", "JobShipment", null, "INSERT INTO dbo.JobShipment(JS_PK, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser) VALUES (@JobShipmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("JV", "JobVoyage", null, "INSERT INTO dbo.JobVoyage(JV_PK, JV_SystemCreateTimeUtc, JV_SystemCreateUser, JV_SystemLastEditTimeUtc, JV_SystemLastEditUser) VALUES (@JobVoyagePK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("JA", "JobVoyOrigin", new[] { "JV" }, "INSERT INTO dbo.JobVoyOrigin(JA_PK, JA_JV, JA_SystemCreateTimeUtc, JA_SystemCreateUser, JA_SystemLastEditTimeUtc, JA_SystemLastEditUser) VALUES (@JobVoyOriginPK, @JobVoyagePK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("JB", "JobVoyDestination", new[] { "JV" }, "INSERT INTO dbo.JobVoyDestination(JB_PK, JB_JV, JB_SystemCreateTimeUtc, JB_SystemCreateUser, JB_SystemLastEditTimeUtc, JB_SystemLastEditUser) VALUES (@JobVoyDestinationPK, @JobVoyagePK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("JX", "JobSailing", new[] { "JA", "JB" }, "INSERT INTO dbo.JobSailing(JX_PK, JX_JA, JX_JB, JX_SystemCreateTimeUtc, JX_SystemCreateUser, JX_SystemLastEditTimeUtc, JX_SystemLastEditUser) VALUES (@JobSailingPK, @JobVoyOriginPK, @JobVoyDestinationPK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("JZ", "JobComInvoiceHeader", new [] { "JE" }, "INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_ClusterKey, JZ_JE, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@JobComInvoiceHeaderPK, 1234567, @JobDeclarationPK, 'AI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("LI", "LandCostInput", ["LT", "JO", ""] , "INSERT INTO [dbo].[LandCostInput] ( [LI_PK], [LI_IsValid], [LI_AC_ChargeCode], [LI_ChargeDescription], [LI_LandedCostGroup], [LI_DistributeCostBy], [LI_CostAmount], [LI_RX_NKCostCurrency], [LI_ServiceExRate], [LI_IsParentGroupInvoice], [LI_IsUserEntered], [LI_ParentID], [LI_ParentTableCode], [LI_LT], [LI_ClusterKey], [LI_SystemCreateTimeUtc], [LI_SystemCreateUser], [LI_SystemLastEditTimeUtc], [LI_SystemLastEditUser] ) VALUES ( @LandCostInputPK, 1, NULL, 'Desc', 1, 'VOL', '123.45', 'USD', 1, 1, 1, @JobOrderLinePK, 'JO', @LandedCostHeaderPK, 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP' )"),
			new TableBuilder("LT", "LandedCostHeader", ["JE"],"INSERT INTO [dbo].[LandedCostHeader] ( [LT_PK], [LT_IsValid], [LT_LandedCostType], [LT_EstimatedLandedCostComment], [LT_DefaultEstimatedDutyRate], [LT_DateOfEntry], [LT_DateOfProcessing], [LT_ParentID], [LT_ParentTableCode], [LT_GC], [LT_ClusterKey], [LT_SystemCreateTimeUtc], [LT_SystemCreateUser], [LT_SystemLastEditTimeUtc], [LT_SystemLastEditUser] ) VALUES ( @LandedCostHeaderPK, 1, 'ABC', '', 1.23, GETDATE(), GETDATE(), @JobDeclarationPK, 'JE', @GlbCompanyPK, 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP' );"),
			new TableBuilder("OA", "OrgAddress", new [] { "OH" }, "INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) VALUES (@OrgAddressPK, @OrgHeaderPK, '123 Main Address', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("OH", "OrgHeader", null, "INSERT INTO dbo.OrgHeader(OH_PK, OH_CODE, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@OrgHeaderPK, 'OH-CODE', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("OL", "OrgSupplierBuyerLink", new [] { "OH" }, "INSERT INTO dbo.OrgSupplierBuyerLink(OL_PK, OL_OH_Supplier, OL_OH_Buyer, OL_SystemCreateTimeUtc, OL_SystemCreateUser, OL_SystemLastEditTimeUtc, OL_SystemLastEditUser) VALUES (@OrgSupplierBuyerLinkPK, @OrgHeaderPK, @OrgHeaderPK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("OP", "OrgSupplierPart", null, "INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser) VALUES (@OrgSupplierPartPK, 'Part1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("OV", "OrgCountryData", new [] { "OH" }, "INSERT INTO dbo.OrgCountryData(OV_PK, OV_OH_OrgHeader, OV_SystemCreateTimeUtc, OV_SystemCreateUser, OV_SystemLastEditTimeUtc, OV_SystemLastEditUser) VALUES (@OrgCountryDataPK, @OrgHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("PF", "OrgSupBuyLinkTrnMode", new [] { "OL" }, "INSERT INTO dbo.OrgSupBuyLinkTrnMode(PF_PK, PF_OL, PF_SystemCreateTimeUtc, PF_SystemCreateUser, PF_SystemLastEditTimeUtc, PF_SystemLastEditUser) VALUES (@OrgSupBuyLinkTrnModePK, @OrgSupplierBuyerLinkPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("PW", "CusBondDetail", null, "INSERT INTO dbo.CusBondDetail(PW_PK, PW_ParentID, PW_ParentTableCode, PW_SystemCreateTimeUtc, PW_SystemCreateUser, PW_SystemLastEditTimeUtc, PW_SystemLastEditUser) VALUES (@CusBondDetailPK, @JobDeclarationPK, 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("QCH", "QuarantineColsHeader", new [] { "CH" }, "INSERT INTO dbo.QuarantineColsHeader(QCH_PK, QCH_CH_CusEntryHeader, QCH_ClusterKey, QCH_SystemCreateTimeUtc, QCH_SystemCreateUser, QCH_SystemLastEditTimeUtc, QCH_SystemLastEditUser) VALUES (@QuarantineColsHeaderPK, @CusEntryHeaderPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("QH", "QuarantineExDocHeader", new [] { "JZ" }, "INSERT INTO dbo.QuarantineExDocHeader(QH_PK, QH_JZ, QH_ClusterKey, QH_SystemCreateTimeUtc, QH_SystemCreateUser, QH_SystemLastEditTimeUtc, QH_SystemLastEditUser) VALUES (@QuarantineExDocHeaderPK, @JobComInvoiceHeaderPK, 1234567, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("QL", "QuarantineExDocLine", new [] { "JI" }, "INSERT INTO dbo.QuarantineExDocLine(QL_PK, QL_JI, QL_ClusterKey, QL_SystemCreateTimeUtc, QL_SystemCreateUser, QL_SystemLastEditTimeUtc, QL_SystemLastEditUser) VALUES (@QuarantineExDocLinePK, @JobComInvoiceLinePK, 1234567, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),

			new TableBuilder("RL", "RefUNLOCO", null, "insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation, RL_SystemCreateTimeUtc, RL_SystemCreateUser, RL_SystemLastEditTimeUtc, RL_SystemLastEditUser) VALUES (@RefUNLOCOPK, 'XXX', convert(geography, 'POINT EMPTY'), GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),
			new TableBuilder("RN", "RefCountry", null, "insert into dbo.RefCountry (RN_PK, RN_Code, RN_Desc, RN_SystemCreateTimeUtc, RN_SystemCreateUser, RN_SystemLastEditTimeUtc, RN_SystemLastEditUser) VALUES (@RefCountryPK, 'ZZ', 'Zayden Zubin Rakhsh Lola', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');"),

			new TableBuilder("SJH", "CusTempStorageJobHeader", new [] { "OH" }, "INSERT INTO dbo.CusTempStorageJobHeader(SJH_PK, SJH_OH_Customer, SJH_GB, SJH_JobReference, SJH_SystemCreateTimeUtc, SJH_SystemCreateUser, SJH_SystemLastEditTimeUtc, SJH_SystemLastEditUser) VALUES (@CusTempStorageJobHeaderPK, @OrgHeaderPK, @GlbBranchPK, 'SJHRef123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("SRH", "CusTempStorageRegHeader", null, "INSERT INTO dbo.CusTempStorageRegHeader(SRH_PK, SRH_AppCode, SRH_Reference, SRH_SystemCreateTimeUtc, SRH_SystemCreateUser, SRH_SystemLastEditTimeUtc, SRH_SystemLastEditUser) VALUES (@CusTempStorageRegHeaderPK, 'CTS', 'REF123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("STH", "CusTempStorageDec", new [] { "SJH" }, "INSERT INTO dbo.CusTempStorageDec(STH_PK, STH_SJH, STH_DeclarationType, STH_SystemCreateTimeUtc, STH_SystemCreateUser, STH_SystemLastEditTimeUtc, STH_SystemLastEditUser) VALUES (@CusTempStorageDecPK, @CusTempStorageJobHeaderPK, 'DEC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("TH", "RatingHeader", new [] { "OH" }, "INSERT INTO dbo.RatingHeader(TH_PK, TH_OH, TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES (@RatingHeaderPK, @OrgHeaderPK, 'SAL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("TI", "RateEntry", new [] { "TH", "GC" }, "INSERT INTO dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser) VALUES (@RateEntryPK, @RatingHeaderPK, @GlbCompanyPK, 'AIR', 'LSE', GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("TL", "RateLines", new [] { "AC", "TI" }, "INSERT INTO dbo.RateLines (TL_PK, TL_AC, TL_TI, TL_RateCalculator, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser) VALUES (@RateLinesPK, @AccChargeCodePK, @RateEntryPK, 'FLT', 'UAH', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("TSL", "CusTempStorageLine", new [] { "STH" }, "INSERT INTO dbo.CusTempStorageLine(TSL_PK, TSL_STH, TSL_LineNo, TSL_ReferenceNumberType, TSL_SystemCreateTimeUtc, TSL_SystemCreateUser, TSL_SystemLastEditTimeUtc, TSL_SystemLastEditUser) VALUES (@CusTempStorageLinePK, @CusTempStorageDecPK, 1, 'T1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("TW1", "CusTWControllingMessageHeader", new [] { "CEI" }, "INSERT INTO dbo.CusTWControllingMessageHeader(TW1_PK, TW1_CEI, TW1_PermitNumber, TW1_SystemCreateTimeUtc, TW1_SystemCreateUser, TW1_SystemLastEditTimeUtc, TW1_SystemLastEditUser) VALUES (@CusTWControllingMessageHeaderPK, @CusEntryInstructionPK, 'ABC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("ULB", "CusUSLVConsignment", new [] { "ULH" }, "INSERT INTO dbo.CusUSLVConsignment(ULB_PK, ULB_ULH, ULB_ClusterKey, ULB_SystemCreateTimeUtc, ULB_SystemCreateUser, ULB_SystemLastEditTimeUtc, ULB_SystemLastEditUser) VALUES (@CusUSLVConsignmentPK, @CusUSLVClearancePK, 12345, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("ULH", "CusUSLVClearance", new [] { "GB" }, "INSERT INTO dbo.CusUSLVClearance(ULH_PK, ULH_GB, ULH_ClusterKey, ULH_JobNumber, ULH_SystemCreateTimeUtc, ULH_SystemCreateUser, ULH_SystemLastEditTimeUtc, ULH_SystemLastEditUser) VALUES (@CusUSLVClearancePK, @GlbBranchPK, 12345, 'JOB123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("WB", "WhsBondedWarehouseAttribute", null, "INSERT INTO dbo.WhsBondedWarehouseAttribute(WB_PK, WB_ParentID, WB_ParentTableCode, WB_SystemCreateTimeUtc, WB_SystemCreateUser, WB_SystemLastEditTimeUtc, WB_SystemLastEditUser) VALUES (@WhsBondedWarehouseAttributePK, NEWID(), 'WE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WD", "WhsDocket", new [] { "OH", "WW" }, "INSERT INTO dbo.WhsDocket(WD_PK, WD_OH_Client, WD_WW_Whs, WD_BookingDate, WD_DocketID, WD_DocketType, WD_DocketSubType, WD_ExternalReference, WD_SystemCreateTimeUtc, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser) VALUES (@WhsDocketPK, @OrgHeaderPK, @WhsWarehousePK, GetUtcDate(), 'WD001', 'ORD', 'ORD', 'ExRef001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WE", "WhsDocketLine", new [] { "WD", "OP" }, "INSERT INTO dbo.WhsDocketLine(WE_PK, WE_WD, WE_OP, WE_DocketLineType, WE_F3_NKPackType, WE_SystemCreateTimeUtc, WE_SystemCreateUser, WE_SystemLastEditTimeUtc, WE_SystemLastEditUser) VALUES (@WhsDocketLinePK, @WhsDocketPK, @OrgSupplierPartPK, 'ORD', 'PK', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WLT", "WhsLocationType", null, "INSERT INTO dbo.WhsLocationType(WLT_PK, WLT_Code, WLT_Description, WLT_SystemCreateTimeUtc, WLT_SystemCreateUser, WLT_SystemLastEditTimeUtc, WLT_SystemLastEditUser) VALUES (@WhsLocationTypePK, 'LTY', 'LocType', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WOB", "CusWHSOperatorTransactionBatch", new [] { "GC", "OA" }, "INSERT INTO dbo.CusWHSOperatorTransactionBatch(WOB_PK, WOB_GC_Company, WOB_OA_Warehouse, WOB_Batch, WOB_SystemCreateTimeUtc, WOB_SystemCreateUser, WOB_SystemLastEditTimeUtc, WOB_SystemLastEditUser) VALUES (@CusWHSOperatorTransactionBatchPK, @GlbCompanyPK, @OrgAddressPK, 'ABC123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WOL", "CusWHSOperatorTransactionLine", new [] { "WOT" }, "INSERT INTO dbo.CusWHSOperatorTransactionLine(WOL_PK, WOL_WOT_WHSOperatorTransactionReceipt, WOL_WOT_WHSOperatorTransactionOrder, WOL_Quantity, WOL_SystemCreateTimeUtc, WOL_SystemCreateUser, WOL_SystemLastEditTimeUtc, WOL_SystemLastEditUser) VALUES (@CusWHSOperatorTransactionLinePK, @CusWHSOperatorTransactionPK, @CusWHSOperatorTransactionPK, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WOT", "CusWHSOperatorTransaction", new [] { "WOB", "OH" }, "INSERT INTO dbo.CusWHSOperatorTransaction(WOT_PK, WOT_WOB_CusWHSTransactionBatch, WOT_TransactionDate, WOT_OH_ProductOwner, WOT_OwnerReference, WOT_Quantity, WOT_SystemCreateTimeUtc, WOT_SystemCreateUser, WOT_SystemLastEditTimeUtc, WOT_SystemLastEditUser) VALUES (@CusWHSOperatorTransactionPK, @CusWHSOperatorTransactionBatchPK, GetUtcDate(), @OrgHeaderPK, 'ABC123', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
			new TableBuilder("WW", "WhsWarehouse", new [] { "GB", "OA", "WLT" }, "INSERT INTO dbo.WhsWarehouse(WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WLT_DefaultLocationType, WW_WarehouseCode, WW_WarehouseType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES (@WhsWarehousePK, @GlbBranchPK, @OrgAddressPK, @WhsLocationTypePK, 'WHC', 'TRW', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),

			new TableBuilder("XX", "GenPivot", new [] { "JE" }, "INSERT INTO dbo.GenPivot(XX_PK, XX_Relation1ID, XX_Relation1TableCode, XX_Relation2ID, XX_Relation2TableCode, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser) VALUES (@GenPivotPK, @JobDeclarationPK, 'JE', @JobDeclarationPK, 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');"),
		};
	}
}
