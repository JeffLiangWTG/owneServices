using System;
using CargoWise.Data;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native
{
	public static class TestUtil
	{
		public static DbConnection Connection
		{
			get
			{
				return DataSetContext.Connection;
			}
		}

		#region Prepare Dummy Data

		public static void AddIndexsToDummy()
		{
			Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Code ON DummyBizo(Z0_Code)");
			Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Number_Z0_Guid ON DummyBizo(Z0_Number,Z0_Guid)");
		}

		public static void AddColumnsToDummy()
		{
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyBizo ADD Z0_OH uniqueidentifier");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyBizo ADD Z0_OH_NKOrgCode CHAR(12)");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyBizo ADD Z0_excludedColumn CHAR(12) NULL");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyBizo ADD Z0_ParentTableCode VARCHAR(3)");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyBizo ADD Z0_ParentID uniqueidentifier");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyDependentBizo ADD ZD1_ParentTableCode VARCHAR(3)");
			Connection.ExecuteNonQuery(@"ALTER TABLE dbo.DummyDependentBizo ADD ZD1_ParentID uniqueidentifier");
		}

		public static void AddOrganizationColumnToDummy()
		{
			Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyDependentBizo ADD ZD1_OH_SomeOrg uniqueidentifier");
		}

		/// <summary>
		/// Drops existing DummyBizo tables and recreates to ensure that changes to the actual DummyBizo tables won't break tests.
		/// </summary>
		public static void RecreateDummyBizoTablesWithStandardSchema()
		{
			var sqlText = @"
				DROP VIEW ZZDummyBizo;
				DROP VIEW ZZDummyBizo_Idx;
				DROP TABLE DummyDependentBizo;
				DROP TABLE DummyBizo;

				CREATE TABLE dbo.DummyBizo
				(
					Z0_PK uniqueidentifier NOT NULL,
					Z0_Code char(5) DEFAULT '',
					Z0_Description varchar(100) DEFAULT '' NOT NULL,
					Z0_Number int DEFAULT 0 NOT NULL,
					Z0_Date datetime NULL,
					Z0_DateTimeOffset datetimeoffset NULL,
					Z0_Time smalldatetime NULL,
					Z0_Geography GEOGRAPHY NOT NULL DEFAULT CONVERT(GEOGRAPHY,'POINT EMPTY'),
					Z0_Decimal decimal(18,0) DEFAULT 0 NOT NULL,
					Z0_Short smallint DEFAULT 0 NOT NULL,
					Z0_Byte tinyint DEFAULT 0 NOT NULL,
					Z0_Money money DEFAULT 0 NOT NULL,
					Z0_SmallDateTime smalldatetime NULL,
					Z0_NVarChar nvarchar(20) DEFAULT '' NOT NULL,
					Z0_VarCharMax varchar(MAX) DEFAULT '' NOT NULL,
					Z0_NVarCharMax nvarchar(MAX) DEFAULT '' NOT NULL,
					Z0_VarBinaryMax varbinary(MAX),
					Z0_BitColumn bit NOT NULL DEFAULT 0,
					CONSTRAINT PK_UX__Z0_PK PRIMARY KEY NONCLUSTERED (Z0_PK),
					CONSTRAINT Constraint_Z0_Geography CHECK (Z0_Geography.STIsValid() = 1 and (Z0_Geography.STIsEmpty() = 1 or Z0_Geography.STGeometryType() in ('Point', 'Polygon')))
				);

				CREATE TABLE dbo.DummyDependentBizo
				(
					ZD1_PK uniqueidentifier NOT NULL,
					ZD1_Z0 uniqueidentifier,
					ZD1_Code varchar(5) DEFAULT '' NOT NULL,
					ZD1_Number int default 0 NOT NULL,
					CONSTRAINT PK_UX__ZD1_PK PRIMARY KEY NONCLUSTERED (ZD1_PK),
					CONSTRAINT DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N FOREIGN KEY (ZD1_Z0) REFERENCES DummyBizo (Z0_PK)
				);
				";

			Connection.ExecuteNonQuery(sqlText);
		}

		public static void AddDummyBizoToGlobalDefinitions()
		{
			GlobalDefinition.Instance.AddDummyBizoToTableMappings();
		}

		public static void AlterDummyTable()
		{
			TableBuilder.AllowDummyColumnsForTest.Value = true;
			AddColumnsToDummy();
			AddIndexsToDummy();
			RemoveForeignKeyFromDummy();
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(GetDefinitionLoaderForTest());
		}

		public static void RemoveForeignKeyFromDummy()
		{
			Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo DROP " + GetColumnDefaultConstraintName("DummyBizo", "Z0_FK_Code"));
			Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo DROP COLUMN Z0_FK_Code");
		}

		public static string GetColumnDefaultConstraintName(string tableName, string columnName)
		{
			return (string)Connection.ExecuteScalar(string.Format(@"SELECT
	constobj.name ConstName
FROM
	sys.columns col
	INNER JOIN sys.tables tab ON tab.object_id = col.object_id
	INNER JOIN sys.default_constraints constobj
		ON constobj.parent_object_id = tab.object_id AND constobj.parent_column_id = col.column_id
WHERE
	tab.name = '{0}'
	AND col.name = '{1}'
", tableName, columnName));
		}

		public static Guid PrepareDummyTableData()
		{
			var pk = PrepareDummyBizoData();

			PrepareDummyDependentBizoData(pk);

			PrepareDummyDependentBizoData(pk);

			return pk;
		}

		public static Guid PrepareDummyBizoData()
		{
			return PrepareDummyBizoData("ABC");
		}

		public static Guid PrepareDummyBizoData(string code)
		{
			var orgPK = (Guid)Connection.ExecuteScalar("select top 1 OH_Pk from dbo.OrgHeader where OH_Code = 'ABIGAS'");
			return PrepareDummyBizoData(code, orgPK);
		}

		public static Guid PrepareDummyBizoData(string code, Guid orgPk)
		{
			return PrepareDummyBizoData(code, orgPk, "ABIGAS");
		}

		public static Guid PrepareDummyBizoData(string code, Guid orgPk, string orgCode)
		{
			var pk = new Guid("3b04d4bc-b2ba-4cf7-8871-6d795d6233de");

			Connection.ExecuteNonQuery(@"insert into dbo.DummyBizo (Z0_PK, Z0_Code, Z0_Guid, Z0_Description, Z0_Number, Z0_Date, Z0_AnotherDate, Z0_Decimal, Z0_AnotherDecimal, Z0_OH, Z0_ParentID,Z0_ParentTableCode, Z0_OH_NKOrgCode)
									VALUES ('" + pk + "', '" + code + "', '" + pk + "', '123', 4, '2008-12-03', '2005-11-26', 4.3, 9.7, '" + orgPk + "','" + orgPk + "','OH','" + orgCode + "')");
			return pk;
		}

		public static Guid PrepareDummyDependentBizoData(Guid parentPK)
		{
			var childPK1 = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.DummyDependentBizo (ZD1_PK, ZD1_Z0, ZD1_ParentTableCode, ZD1_Code, ZD1_Number) VALUES ('" + childPK1 + "', '" + parentPK + "', 'Z0', 'RAK', 28)");
			return childPK1;
		}

		public static Guid PrepareDummyDependentSuffixData(Guid parentPK)
		{
			var childPK1 = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.DummyDependentBizo (ZD1_PK, ZD1_Code, ZD1_Number) VALUES ('" + childPK1 + "', 'RAK', 28)");
			return childPK1;
		}

		public static Guid PrepareDummyPivotData(Guid dummyPK, Guid dependentPK)
		{
			var childPK2 = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.DummyPivot (ZDP_PK, ZDP_Z0, ZDP_ZD1) VALUES ('" + childPK2 + "', '" + dummyPK + "', '" + dependentPK + "')");
			return childPK2;
		}

		#endregion

		#region Prepare Org Data

		public static Guid PrepareOrgHeaderTableData()
		{
			var pk = Guid.NewGuid();
			return PrepareOrgHeaderTableData("ZUBORG", "Zayden Zubin Rakhsh Lola", pk);
		}

		public static Guid PrepareOrgHeaderTableData(string code)
		{
			var pk = Guid.NewGuid();
			return PrepareOrgHeaderTableData(code, pk);
		}

		#endregion

		public static Guid PrepareJobOrderHeaderData(Guid orgAddressPk)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.JobOrderHeader (JD_PK, JD_OA_BuyerAddress, JD_OrderNumber) VALUES ('" + pk + "', '" + orgAddressPk + "', 'O0001')");
			return pk;
		}

		#region Prepare Country Data

		public static Guid PrepareCountryData()
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.RefCountry (RN_PK, RN_Code, RN_Desc) 
			VALUES ('" + pk + "', 'ZZ', 'Zayden Zubin Rakhsh Lola')");
			return pk;
		}

		public static Guid PrepareZoneHeaderData()
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.RefZoneHeader (FZ_PK, FZ_Code, FZ_ZoneType, FZ_Description, FZ_SystemCreateTimeUtc, FZ_SystemCreateUser, FZ_SystemLastEditTimeUtc, FZ_SystemLastEditUser) 
			VALUES ('" + pk + "', 'ZZZZ', 'ALL', 'Zayden Zubin Rakhsh Lola', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			return pk;
		}

		public static Guid PrepareZonePivotData(Guid countryPK, Guid zoneHeaderPK)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.RefZonePivot (F2_PK, F2_FZ, F2_ParentID, F2_ParentTableCode, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser)	VALUES ('" + pk + "', '" + zoneHeaderPK + "', '" + countryPK + "', 'RN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			return pk;
		}

		#endregion

		#region Prepare Entity

		public static Entity PrepareDummyBizoEntity(AncillaryImportServices sessionServices)
		{
			return PrepareEntity(FindEntityDefinition("Dummy", "DummyBizo"), sessionServices);
		}

		public static Entity PrepareOrgHeaderEntity(AncillaryImportServices sessionServices)
		{
			return PrepareEntity(FindEntityDefinition("Organization", "OrgHeader"), sessionServices);
		}

		public static Entity PrepareStmNoteEntity(AncillaryImportServices sessionServices)
		{
			return PrepareEntity(FindEntityDefinition("Organization", "OrgHeader.StmNote"), sessionServices);
		}

		public static Entity PrepareRefCountryEntity(AncillaryImportServices sessionServices)
		{
			return PrepareEntity(FindEntityDefinition("Organization", "OrgHeader.OrgCusCode.CodeCountry"), sessionServices);
		}

		static Entity PrepareEntity(IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			var entity = new Entity(definition, sessionServices);
			var pk = Guid.NewGuid();
			entity.InternalPK = pk;
			return entity;
		}

		public static Guid PrepareGlbCompanyTableData(string code)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery($"insert into dbo.GlbCompany (GC_PK, GC_Code, GC_Name) VALUES ('{pk}', '{code}', 'AU company')");
			return pk;
		}

		public static Guid PrepareGlbBranchTableData(string code, Guid fk)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery($"insert into dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES ('{pk}', '{code}', '{fk}')");
			return pk;
		}

		public static Guid PrepareOrgHeaderTableData(string code, string fullName, Guid pk)
		{
			Connection.ExecuteNonQuery(@"insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) 
									VALUES ('" + pk + "', '" + code + "', '" + fullName + "')");
			return pk;
		}

		public static Guid PrepareOrgHeaderTableData(string code, Guid pk)
		{
			string fullName = "Zayden Zubin Rakhsh Lola";
			PrepareOrgHeaderTableData(code, fullName, pk);
			return pk;
		}

		public static Guid PrepareOrgContactTableData(Guid fk)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.OrgContact (OC_PK, OC_ContactName ,OC_OH) 
									VALUES ('" + pk + "', 'Rakhsh', '" + fk + "')");
			return pk;
		}

		public static Guid PrepareOrgAddressTableData(Guid fk)
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into dbo.OrgAddress (OA_PK, OA_Address1, OA_OH)
									VALUES ('" + pk + "', 'Add1', '" + fk + "')");
			return pk;
		}

		public static Guid PrepareRefUNLOCOTableData()
		{
			var pk = Guid.NewGuid();
			Connection.ExecuteNonQuery(@"insert into RefUNLOCO(RL_PK) VALUES ('" + pk + "')");
			return pk;
		}

		#endregion

		public static IDefinitionLoader GetDefinitionLoaderForTest()
		{
			var loader = new DefinitionAssemblyLoader(new[]
			{
				typeof(TestUtil).Assembly.FullName,
				typeof(DefinitionAssemblyLoader).Assembly.FullName
			});
			return loader;
		}

		public static IDefinitionFinder GetEntitySetDefinitionFinder()
		{
			var finder = new DefinitionFinder();
			finder.Cache = EntitySetDefinitionCache.GetInstance();
			return finder;
		}

		public static EntitySetDefinition GetEntitySetDefinition(string name)
		{
			var context = GetEntitySetDefinitionFinder();
			return context.FindByEntitySetName(name);
		}

		public static EntityDefinitionCollection GetEntityDefinitionCollection(string name)
		{
			return GetEntitySetDefinition(name).Entities;
		}

		public static EntityDefinition FindEntityDefinition(string entitySetName, string entityName)
		{
			var definitionCollection = GetEntityDefinitionCollection(entitySetName);
			return definitionCollection.FindDefinition(entityName);
		}
	}
}
