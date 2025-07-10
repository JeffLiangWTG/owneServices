using System;
using System.Collections.Immutable;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using Enterprise.DbUpgrader.Shared;
using Moq;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Manages creation of Unit Test template for the main database
	/// </summary>
	public class MainTemplateDbCreatorForTesting : AuxiliaryDbCreator
	{
		public MainTemplateDbCreatorForTesting(string templateDbName)
			: base(templateDbName)
		{
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			conn.ExecuteNonQuery(CreateTemplateDbXmlSchemasScript);
			conn.ExecuteNonQuery(CreateObjectsScript);
			CreateClientSpecificTables(conn);
		}

		void CreateClientSpecificTables(DbConnection conn)
		{
			CreateWithMockExtensions(conn);
		}

		#region Create Objects Script

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE TEST DATABASE XML SCHEMAS
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		const string CreateTemplateDbXmlSchemasScript = @"
			CREATE XML SCHEMA COLLECTION XsdEql AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';
			CREATE XML SCHEMA COLLECTION XsdMod AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /><xsd:element name=""e1"" type=""xsd:byte"" /></xsd:schema>';
			CREATE XML SCHEMA COLLECTION XsdNew AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';
		";

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE TEST TEMPLATE DATABASE OBJECTS
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		const string CreateObjectsScript = @"
			-- This table hasn't been changed
			-- Its needed to get/set version info
			CREATE TABLE dbo.StmData
			( 
					SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
					SD_Name VARCHAR(300) NOT NULL,
					SD_Owner UNIQUEIDENTIFIER NULL,
					SD_DepartmentGuid UNIQUEIDENTIFIER NULL,
					SD_Type CHAR(3) NOT NULL,
					SD_IsLogged CHAR(1) NULL DEFAULT ('N'),
					SD_BinaryValue VARBINARY(max) NULL,
					SD_GuidValue UNIQUEIDENTIFIER NULL,
					SD_PreserveTestValue BIT NULL,
			)
			;
			ALTER TABLE dbo.StmData
				ADD CONSTRAINT PK_StmData PRIMARY KEY NONCLUSTERED (SD_PK)
			;

			-- This table is a NEW one
			CREATE TABLE New
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL
			)
			;
			ALTER TABLE New
				ADD CONSTRAINT PK_New PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- This table had:
			--  4 columns changed - Col3 (to varchar(40) not null), Col4 (to money), Col5 (to varchar(5)) and Col6 (to SmallDateTime)
			--  1 column removed  - Col7
			CREATE TABLE Match_ColDiff
			( 
				Col1 INT           NOT NULL,
				Col2 CHAR(1)       NOT NULL,
				Col3 VARCHAR(40)   NOT NULL DEFAULT ('Col3 Default'),
				Col4 MONEY         NULL,
				Col5 VARCHAR(5)    NULL,
				Col6 SMALLDATETIME NULL,
				Col8 SMALLINT      NOT NULL DEFAULT (0),
				Col9 BIT           NOT NULL,
				SystemLastEditTimeUtc SMALLDATETIME NULL,
			)
			;
			ALTER TABLE Match_ColDiff
				ADD CONSTRAINT PK_Match_ColDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			CREATE NONCLUSTERED INDEX IX_Match_ColDiff_02 ON Match_ColDiff (Col3, Col4, Col5, Col6)
			;

			-- This table had a character-case change in its name (MatcH_TableNameCaseDiff -> Match_TableNameCaseDiff)
			CREATE TABLE Match_TableNameCaseDiff
			( 
				Col1 INT NOT NULL
			)
			;

			-- This table had 1 character-case change in a column name (col2 -> Col2)
			CREATE TABLE Match_ColCaseDiff
			( 
				Col1 INT         NOT NULL,
				Col2 VARCHAR(30) NULL
			)
			;

			-- This table had 
			--   PK changed (to clustered) 
			--   Unique Constraint column changed (to Col3)
			CREATE TABLE Match_PKDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL
			)
			;
			ALTER TABLE Match_PKDiff
				ADD CONSTRAINT PK_Match_PKDiff PRIMARY KEY CLUSTERED (Col1)
			;
			ALTER TABLE Match_PKDiff
				ADD CONSTRAINT UK_Match_PKDiff UNIQUE NONCLUSTERED (Col3)
			;
			CREATE NONCLUSTERED INDEX IX_Match_PKDiff_01 ON Match_PKDiff (Col4)
			;

			-- This table had
			--   IX_Match_IndexDiff_01 - changed to NOT UNIQUE, CLUSTERED
			--   IX_Match_IndexDiff_02 - col3 added to the index
			--   IX_Match_IndexDiff_03 - removed
			--   IX_Match_IndexDiff_04 - changed to NONCLUSTERED
			--   IX_Match_IndexDiff_05 - created
			CREATE TABLE Match_IndexDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL,
				Col5 INT         NULL
			)
			;
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT PK_Match_IndexDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			CREATE CLUSTERED INDEX IX_Match_IndexDiff_01 ON Match_IndexDiff (Col4)
			;
			CREATE NONCLUSTERED INDEX IX_Match_IndexDiff_02 ON Match_IndexDiff (Col3,Col2)
			;
			CREATE NONCLUSTERED INDEX IX_Match_IndexDiff_04 ON Match_IndexDiff (Col5)
			;
			CREATE UNIQUE NONCLUSTERED INDEX IX_Match_IndexDiff_05 ON Match_IndexDiff (Col3)
			;

			-- This table had
			-- a default constraint changed - default on Col2 to 'B'
			-- a check constraint added - CK_Match_DefaultDiff
			CREATE TABLE Match_DefaultDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL DEFAULT ('B'),
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL
			)
			;
			ALTER TABLE Match_DefaultDiff
				ADD CONSTRAINT PK_Match_DefaultDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE Match_DefaultDiff
				ADD CONSTRAINT CK_Match_DefaultDiff_Col2 CHECK (Col2 != '')
			;

			-- This table had its column order changed
			CREATE TABLE Match_OrderDiff
			( 
				Col1_Was1 TINYINT NULL,
				Col2_Was4 TINYINT NULL,
				Col3_Was2 TINYINT NULL,
				Col4_Was3 TINYINT NULL
			)
			;

			-- This is client-specific table that matches with a table in the test template schema
			-- The table MUST be upgraded to the schema on the test template schema
			CREATE TABLE ClientMatch
			( 
				Col1 INT         NOT NULL,
				Col2 VARCHAR(15) NOT NULL,
				Col3 CHAR(10)    NULL,
				Col4 INT         NULL DEFAULT (1)
			)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT PK_ClientMatch PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT CK_ClientMatch_Col3 CHECK (Col3 != '')
			;
			CREATE NONCLUSTERED INDEX IX_ClientMatch_01 ON ClientMatch (Col3, Col4)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT FK_ClientMatch_Col4_TO_ClientMatch_Col1 FOREIGN KEY (Col4)
						REFERENCES ClientMatch (Col1)
			;

			-- This table had all original columns dropped, and new ones added
			CREATE TABLE DropAllColumnsIssue
			( 
				ColNew CHAR(5) NOT NULL
			)
			;
			ALTER TABLE DropAllColumnsIssue
				ADD CONSTRAINT PK_DropAllColumnsIssue PRIMARY KEY NONCLUSTERED (ColNew)
			;

			-- Foreign Keys
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_TO_Match_ColDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_ColDiff (Col1)
			;

			ALTER TABLE Match_DefaultDiff
				ADD CONSTRAINT FK_Match_DefaultDiff_TO_Match_PKDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_PKDiff (Col1) ON DELETE CASCADE
			;

			-- This FK is a NEW one
			ALTER TABLE New
				ADD CONSTRAINT FK_New_TO_Match_PKDiff_01 FOREIGN KEY (Col1)
						REFERENCES Match_PKDiff (Col1)
			;

			-- This table had
			--   A number of secondary XML index changes
			CREATE TABLE XmlTypeIssues
			( 
				Col1 INT          NOT NULL,
				Col2 XML (XsdEql) NULL,
				Col3 XML (XsdMod) NULL,
				Col4 XML	        NULL,
				Col5 XML (XsdNew) NULL,
				Col6 XML (XsdNew) NULL,
			)
			;
			ALTER TABLE XmlTypeIssues
				ADD CONSTRAINT PK_XmlTypeIssues PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col2 ON XmlTypeIssues (Col2) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Value2Property ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR PROPERTY
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_New ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col3_New ON XmlTypeIssues (Col3) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col3_New_Value ON XmlTypeIssues (Col3) 
				USING XML INDEX IX_XmlTypeIssues_Col3_New FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Changing ON XmlTypeIssues (Col5) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Col ON XmlTypeIssues (Col5) 
				USING XML INDEX IX_XmlTypeIssues_Changing FOR PATH
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_NewPrimary ON XmlTypeIssues (Col4) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Parent ON XmlTypeIssues (Col4) 
				USING XML INDEX IX_XmlTypeIssues_NewPrimary FOR VALUE
			;

			-- This table had
			--   PK changed (to NONclustered) 
			--   Col3 added just to trigger CopyTableBasedColumnSynchroniser
			CREATE TABLE XmlTypeWithPkChange
			( 
				Col1 INT         NOT NULL,
				Col2 XML	     NULL,
				Col3 INT	     NULL
			)
			;
			ALTER TABLE XmlTypeWithPkChange
				ADD CONSTRAINT PK_XmlTypeWithPkChange PRIMARY KEY NONCLUSTERED (Col1)
			;
			-- Foreign Keys
			ALTER TABLE XmlTypeWithPkChange
				ADD CONSTRAINT FK_XmlTypeWithPkChange_TO_XmlTypeIssues_01 FOREIGN KEY (Col1)
						REFERENCES XmlTypeIssues (Col1)
			;

			-- This table is new with typed XML fields
			CREATE TABLE XmlNewWithSchema
			( 
				Col1 INT          NOT NULL,
				Col2 XML (XsdEql) NULL,
			)
			;

			-- 1. new pk and indexes ----------------------------------------------------------------------------------------

			CREATE TABLE Spatial_NewIndex
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geometry  geometry             NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_NewIndex PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Spatial_NewIndex (S_Geometry)  USING GEOMETRY_GRID      WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Spatial_NewIndex (S_Geometry)  USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography      ON Spatial_NewIndex (S_Geography) USING GEOGRAPHY_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Spatial_NewIndex (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			-- 2. alter index def    ----------------------------------------------------------------------------------------

			CREATE TABLE Spatial_AlterIndex
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geometry  geometry             NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_AlterIndex PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Spatial_AlterIndex (S_Geometry)  USING GEOMETRY_GRID       WITH (BOUNDING_BOX = (1, 1, 2, 2))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Spatial_AlterIndex (S_Geometry)  USING GEOMETRY_AUTO_GRID  WITH (BOUNDING_BOX = (1, 1, 2, 2))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography      ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_GRID      WITH (GRIDS = (LOW, LOW, LOW, LOW))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 2)
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_2    ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (ALLOW_PAGE_LOCKS = OFF)
			;

			-- 3. alter pk           ----------------------------------------------------------------------------------------

			CREATE TABLE Spatial_AlterPK
			(
				S_PK        uniqueidentifier NOT NULL,
				S_PK_2      uniqueidentifier NOT NULL,
				S_Geometry  geometry             NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_AlterPK PRIMARY KEY CLUSTERED (S_PK_2)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Spatial_AlterPK (S_Geometry)  USING GEOMETRY_GRID      WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Spatial_AlterPK (S_Geometry)  USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography      ON Spatial_AlterPK (S_Geography) USING GEOGRAPHY_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Spatial_AlterPK (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			-- 4. alter column default --------------------------------------------------------------------------------------

			CREATE TABLE Spatial_AlterColumnDefault
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geometry  geometry         NOT NULL DEFAULT CONVERT(geometry, 'POINT EMPTY'),
				S_Geography geography        NOT NULL DEFAULT CONVERT(geography, 'POINT EMPTY'),

				CONSTRAINT PK_Spatial_AlterColumnDefault PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Spatial_AlterColumnDefault (S_Geometry)  USING GEOMETRY_GRID      WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Spatial_AlterColumnDefault (S_Geometry)  USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography      ON Spatial_AlterColumnDefault (S_Geography) USING GEOGRAPHY_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Spatial_AlterColumnDefault (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			";

		#endregion

		#region Client Specific Tables

		/// <summary>
		/// Sets the client hook to the Mock ClientHook and
		/// calls the Create method to create client specific tables
		/// </summary>
		void CreateWithMockExtensions(DbConnection conn)
		{
			var mockClientExtensions = new MockClientSpecificExtensionObjectsSourceForTesting();

			var serviceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => serviceProvider.GetService(x));
			mockServiceProvider.Setup(x => x.GetService(typeof(IExtensionObjectsSource))).Returns(mockClientExtensions);

			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				new ClientSpecificTableCreator().Create(conn);
			}
		}

		sealed class MockClientSpecificExtensionObjectsSourceForTesting : IExtensionObjectsSource
		{
			public string DisplayName => "Test";

			public string ExtensionCode => "XXX";

			public IExtensionObjects ExtensionObjects { get; } = new ExtensionObjects(
				ImmutableArray.Create(new DatabaseObjectCreateScript("ClientNew", "CREATE TABLE ClientNew (Col1 INT NOT NULL DEFAULT 0)", "drop table ClientNew")),
				ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty);
		}

		#endregion
	}
}
