namespace Enterprise.DbUpgrader.Schema
{
	public class MainUpgradeDbCreatorForTesting : AuxiliaryDbCreatorForTesting
	{
		public MainUpgradeDbCreatorForTesting(string dbName)
			: base(
			dbName,
			EnableDbChangeTrackingScript,
			CreateTestDbXmlSchemasScript,
			CreateTestDbTablesScript,
			CreateAlienSchema,
			CreateAlienObjects,
			CreateSchemaBoundView01,
			CreateSchemaBoundView02,
			CreateSchemaBoundView03,
			CreateSchemaBoundView04,
			CreateSchemaBoundFunction01,
			CreateSchemaBoundFunction02,
			CreateSchemaBoundFunction03,
			CreateSchemaBoundFunction04,
			InsertTestDbDataScript)
		{
		}

		#region Scripts

		const string EnableDbChangeTrackingScript = "ALTER DATABASE CURRENT SET CHANGE_TRACKING = ON";

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE TEST DATABASE XML SCHEMAS
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		const string CreateTestDbXmlSchemasScript = @"
			CREATE XML SCHEMA COLLECTION XsdEql AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';
			CREATE XML SCHEMA COLLECTION XsdMod AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';
			CREATE XML SCHEMA COLLECTION XsdDel AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';
		";

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE TEST DATABASE TABLES
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		const string CreateTestDbTablesScript = @"
			-- This table will not be changed
			-- And its also needed to get/set version info
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

			-- This table will no longer exist in the new schema
			CREATE TABLE Old
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL
			)
			;
			ALTER TABLE Old
				ADD CONSTRAINT PK_Old PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- This table will have:
			--  4 columns changed - Col3, Col4, Col5 and Col6
			--  1 column removed  - Col7
			CREATE TABLE Match_ColDiff
			( 
				Col1  INT          NOT NULL,
				Col2  CHAR(1)      NOT NULL,
				Col3  VARCHAR(30)  NULL,
				Col4  CHAR(10)     NULL,
				Col5  VARCHAR(10)  NULL,
				Col6  DATETIME     NULL,
				Col7  DECIMAL(3,1) NULL,
				Col9  CHAR(1)      NULL,
				SystemLastEditTimeUtc SMALLDATETIME NULL,
			)
			;
			ALTER TABLE Match_ColDiff
				ADD CONSTRAINT PK_Match_ColDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			CREATE NONCLUSTERED INDEX IX_Match_ColDiff_01 ON Match_ColDiff (Col7)
			;
			CREATE NONCLUSTERED INDEX IX_Match_ColDiff_02 ON Match_ColDiff (Col3, Col4, Col5, Col6)
			;

			-- This table will have a character-case change in its name (MatcH_TableNameCaseDiff)
			CREATE TABLE MatcH_TableNameCaseDiff
			( 
				Col1 INT NOT NULL
			)
			;
			ALTER TABLE MatcH_TableNameCaseDiff
				ADD CONSTRAINT PK_MatcH_TableNameCaseDiff PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- This table will have 1 character-case change in a column name (col2)
			CREATE TABLE Match_ColCaseDiff
			( 
				Col1 INT         NOT NULL,
				col2 VARCHAR(30) NULL
			)
			;

			-- This table will have
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
				ADD CONSTRAINT PK_Match_PKDiff PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE Match_PKDiff
				ADD CONSTRAINT UK_Match_PKDiff UNIQUE NONCLUSTERED (Col2)
			;
			CREATE NONCLUSTERED INDEX IX_Match_PKDiff_01 ON Match_PKDiff (Col4)
			;

			-- This table will have
			--   IX_Match_IndexDiff_01 - changed to NOT UNIQUE, CLUSTERED
			--   IX_Match_IndexDiff_02 - col3 added to the index
			--   IX_Match_IndexDiff_03 - removed
			--   IX_Match_IndexDiff_04 - changed to NONCLUSTERED
			--   IX_Match_IndexDiff_05 - WILL be created
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
			CREATE UNIQUE NONCLUSTERED INDEX IX_Match_IndexDiff_01 ON Match_IndexDiff (Col4)
			;
			CREATE NONCLUSTERED INDEX IX_Match_IndexDiff_02 ON Match_IndexDiff (Col2)
			;
			CREATE NONCLUSTERED INDEX IX_Match_IndexDiff_03 ON Match_IndexDiff (Col3)
			;
			CREATE CLUSTERED INDEX IX_Match_IndexDiff_04 ON Match_IndexDiff (Col5)
			;

			-- This table will have
			-- a default constraint changed - default on Col2 to 'B'
			-- a check constraint added - CK_Match_DefaultDiff
			CREATE TABLE Match_DefaultDiff
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL DEFAULT ('A'),
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL
			)
			;
			ALTER TABLE Match_DefaultDiff
				ADD CONSTRAINT PK_Match_DefaultDiff PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- This table will have its column order changed
			CREATE TABLE Match_OrderDiff
			( 
				Col1_Was1 TINYINT NULL,
				Col3_Was2 TINYINT NULL,
				Col4_Was3 TINYINT NULL,
				Col2_Was4 TINYINT NULL
			)
			;

			-- This is client-specific table.
			-- The table itself and any of its constraints MUST NOT be removed although it's not in the test template schema
			CREATE TABLE ClientTable
			( 
				Col1 INT     NOT NULL,
				Col2 CHAR(1) NOT NULL DEFAULT ('A'),
				Col3 INT     NULL
			)
			;
			ALTER TABLE ClientTable
				ADD CONSTRAINT PK_ClientTable PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE ClientTable
				ADD CONSTRAINT CK_ClientTable_Col2 CHECK (Col2 != '')
			;
			CREATE NONCLUSTERED INDEX IX_ClientTable_01 ON ClientTable (Col2)
			;
			ALTER TABLE ClientTable
				ADD CONSTRAINT FK_ClientTable_TO_ClientTable01 FOREIGN KEY (Col3)
						REFERENCES ClientTable (Col1)
			;

			-- This is client-specific table that matches with a table in the test template schema
			-- The table MUST be upgraded to the schema on the test template schema
			CREATE TABLE ClientMatch
			( 
				Col1 INT     NOT NULL,
				Col2 CHAR(1) NULL,
				Col3 INT     NULL,
				Col4 INT     NULL DEFAULT (0)
			)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT PK_ClientMatch PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT CK_ClientMatch_Col2 CHECK (Col2 != '')
			;
			CREATE NONCLUSTERED INDEX IX_ClientMatch_01 ON ClientMatch (Col2)
			;
			ALTER TABLE ClientMatch
				ADD CONSTRAINT FK_ClientMatch_Col3_TO_ClientMatch_Col1 FOREIGN KEY (Col3)
						REFERENCES ClientMatch (Col1)
			;

			-- This table will have all original columns dropped, and new ones added
			CREATE TABLE DropAllColumnsIssue
			( 
				ColOld INT NOT NULL
			)
			;
			ALTER TABLE DropAllColumnsIssue
				ADD CONSTRAINT PK_DropAllColumnsIssue PRIMARY KEY NONCLUSTERED (ColOld)
			;

			-- Foreign Keys
			ALTER TABLE Match_IndexDiff
				ADD CONSTRAINT FK_Match_IndexDiff_TO_Match_ColDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_ColDiff (Col1)
			;

			ALTER TABLE Match_DefaultDiff
				ADD CONSTRAINT FK_Match_DefaultDiff_TO_Match_PKDiff_01 FOREIGN KEY (Col4)
						REFERENCES Match_PKDiff (Col1)
			;

			-- This FK will no longer exist in the new schema
			ALTER TABLE Match_PKDiff
				ADD CONSTRAINT FK_Match_PKDiff_TO_Old_01 FOREIGN KEY (Col4)
						REFERENCES Old (Col1)
			;

			-- This table will have
			--   A number of secondary XML index changes
			CREATE TABLE XmlTypeIssues
			( 
				Col1 INT          NOT NULL,
				Col2 XML (XsdEql) NULL,
				Col3 XML (XsdMod) NULL,
				Col4 XML          NULL,
				Col5 XML (XsdDel) NULL,
			)
			;
			ALTER TABLE XmlTypeIssues
				ADD CONSTRAINT PK_XmlTypeIssues PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col2 ON XmlTypeIssues (Col2) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Old ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR PATH
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col2_Value2Property ON XmlTypeIssues (Col2) 
				USING XML INDEX IX_XmlTypeIssues_Col2 FOR VALUE
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Col3_Old ON XmlTypeIssues (Col3) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Col3_Old_Path ON XmlTypeIssues (Col3) 
				USING XML INDEX IX_XmlTypeIssues_Col3_Old FOR PATH
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeIssues_Changing ON XmlTypeIssues (Col4) 
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Col ON XmlTypeIssues (Col4) 
				USING XML INDEX IX_XmlTypeIssues_Changing FOR PROPERTY
			;
			CREATE XML INDEX IX_XmlTypeIssues_Changing_Parent ON XmlTypeIssues (Col4) 
				USING XML INDEX IX_XmlTypeIssues_Changing FOR VALUE
			;

			-- This table will have
			--   PK changed (to NONclustered)
			--   Col3 added just to trigger CopyTableBasedColumnSynchroniser
			CREATE TABLE XmlTypeWithPkChange
			( 
				Col1 INT         NOT NULL,
				Col2 XML	     NULL
			)
			;
			ALTER TABLE XmlTypeWithPkChange
				ADD CONSTRAINT PK_XmlTypeWithPkChange PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_XmlTypeWithPkChange_01 ON XmlTypeWithPkChange (Col2) 
			;
			CREATE XML INDEX IX_XmlTypeWithPkChange_01_Path ON XmlTypeWithPkChange (Col2) 
				USING XML INDEX IX_XmlTypeWithPkChange_01 FOR PATH
			;
			-- Foreign Keys
			ALTER TABLE XmlTypeWithPkChange
				ADD CONSTRAINT FK_XmlTypeWithPkChange_TO_XmlTypeIssues_01 FOREIGN KEY (Col1)
						REFERENCES XmlTypeIssues (Col1)
			;

			CREATE TABLE [JobDocAddress]
			(
				[E2_AddressType]		[varchar](3)		NOT NULL,
				[E2_Contact]			[nvarchar](50)		NOT NULL,
				[E2_Address1]			[nvarchar](50)		NOT NULL,
				[E2_Address2]			[nvarchar](50)		NOT NULL,
				[E2_City]				[nvarchar](25)		NOT NULL,
				[E2_Postcode]			[nvarchar](10)		NOT NULL,
				[E2_State]				[nvarchar](25)		NOT NULL,
				[E2_RN_NKCountryCode]	[varchar](2)		NOT NULL,
				[E2_Phone]				[varchar](20)		NOT NULL,
				[E2_Fax]				[varchar](20)		NOT NULL,
				[E2_ParentID]			[uniqueidentifier]	NOT NULL,
				[E2_Mobile]				[varchar](20)		NOT NULL,
				[E2_CompanyName]		[nvarchar](100)		NOT NULL,
				[E2_ParentTableCode]	[varchar](3)		NOT NULL,
				[E2_Email]				[nvarchar](254)		NOT NULL
			)
			;

			CREATE TABLE [SupplierBookingLine]
			(
				[DL_PK]							[uniqueidentifier]	NOT NULL
				,[DL_ConsigneeName]				[nvarchar]	(100)	NOT NULL DEFAULT ''
				,[DL_ConsigneeAddress1]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsigneeAddress2]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsigneeCity]				[nvarchar]	(25)	NOT NULL DEFAULT ''
				,[DL_ConsigneeState]			[nvarchar]	(25)	NOT NULL DEFAULT ''
				,[DL_ConsigneePostCode]			[nvarchar]	(10)	NOT NULL DEFAULT ''
				,[DL_RN_NKConsigneeCountryCode]	[varchar]	(2)		NOT NULL DEFAULT ''
				,[DL_ConsigneeContact]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsigneeEmail]			[nvarchar]	(254)	NOT NULL DEFAULT ''
				,[DL_ConsigneePhone]			[varchar]	(20)	NOT NULL DEFAULT ''
				,[DL_ConsigneeMobile]			[varchar]	(20)	NOT NULL DEFAULT ''
				,[DL_ConsigneeFax]				[varchar]	(20)	NOT NULL DEFAULT ''
				,[DL_ConsignorName]				[nvarchar]	(100)	NOT NULL DEFAULT ''
				,[DL_ConsignorAddress1]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsignorAddress2]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsignorCity]				[nvarchar]	(25)	NOT NULL DEFAULT ''
				,[DL_ConsignorState]			[nvarchar]	(25)	NOT NULL DEFAULT ''
				,[DL_ConsignorPostCode]			[nvarchar]	(10)	NOT NULL DEFAULT ''
				,[DL_RN_NKConsignorCountryCode]	[varchar]	(2)		NOT NULL DEFAULT ''
				,[DL_ConsignorContact]			[nvarchar]	(50)	NOT NULL DEFAULT ''
				,[DL_ConsignorEmail]			[nvarchar]	(254)	NOT NULL DEFAULT ''
				,[DL_ConsignorPhone]			[varchar]	(20)	NOT NULL DEFAULT ''
				,[DL_ConsignorMobile]			[varchar]	(20)	NOT NULL DEFAULT ''
				,[DL_ConsignorFax]				[varchar]	(20)	NOT NULL DEFAULT ''
				,CONSTRAINT [PK_UX__DL_PK] PRIMARY KEY NONCLUSTERED 
				(
					[DL_PK] ASC
				) ON [PRIMARY]
			)
			;

			-- 1. new pk and indexes ----------------------------------------------------------------------------------------

			CREATE TABLE ClientSpatial -- must be skipped by upgrader
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_ClientSpatial PRIMARY KEY CLUSTERED (S_PK)
			)
			;
			CREATE SPATIAL INDEX IX_Spatial ON ClientSpatial (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			CREATE TABLE Spatial_NewIndex
			(
				S_PK        uniqueidentifier NOT NULL,
				S_Geometry  geometry             NULL,
				S_Geography geography            NULL,
			)
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
			CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Spatial_AlterIndex (S_Geometry)  USING GEOMETRY_GRID      WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Spatial_AlterIndex (S_Geometry)  USING GEOMETRY_AUTO_GRID WITH (BOUNDING_BOX = (0, 0, 1, 1))
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography      ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_ForRemove      ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_GRID
			;
			CREATE SPATIAL INDEX IX_Spatial_Geography_2    ON Spatial_AlterIndex (S_Geography) USING GEOGRAPHY_AUTO_GRID
			;

			-- 3. alter pk           ----------------------------------------------------------------------------------------

			CREATE TABLE Spatial_AlterPK
			(
				S_PK        uniqueidentifier NOT NULL,
				S_PK_2      uniqueidentifier NOT NULL,
				S_Geometry  geometry             NULL,
				S_Geography geography            NULL,

				CONSTRAINT PK_Spatial_AlterPK PRIMARY KEY CLUSTERED (S_PK)
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
				S_Geometry  geometry         NOT NULL DEFAULT CONVERT(geometry, 'POINT (0 0)'),
				S_Geography geography        NOT NULL DEFAULT CONVERT(geography, 'POINT (0 0)'),

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

		const string CreateAlienSchema = "CREATE SCHEMA Alien;";

		const string CreateAlienObjects = @"
			-------------------------
			-- Alien Schema Tables --
			-------------------------
			CREATE TABLE Alien.TabAlien1 (Col1 INT NOT NULL);
			ALTER TABLE Alien.TabAlien1 ADD CONSTRAINT PK_StmData PRIMARY KEY NONCLUSTERED (Col1);
			CREATE TABLE Alien.TabAlien2 (Col1 INT);
			CREATE TABLE Alien.StmData (Col1 int);
			-- Alien Schema FK
			ALTER TABLE Alien.TabAlien2 ADD CONSTRAINT TabAlien2_2_TabAlien1 FOREIGN KEY (Col1) REFERENCES Alien.TabAlien1 (Col1);
			-- DBO to Alien FK
			ALTER TABLE dbo.ClientTable ADD CONSTRAINT ClientTable_2_TabAlien1 FOREIGN KEY (Col3) REFERENCES Alien.TabAlien1 (Col1);
			";

		const string CreateSchemaBoundView01 = "CREATE VIEW vw_SchemaBound01 WITH SCHEMABINDING AS SELECT Col1 FROM dbo.Old;";
		const string CreateSchemaBoundView02 = "CREATE VIEW vw_SchemaBound02 WITH SCHEMABINDING AS SELECT Col1 FROM dbo.vw_SchemaBound01;";
		const string CreateSchemaBoundView03 = "CREATE VIEW vw_SchemaBound03 WITH SCHEMABINDING AS SELECT Col1 FROM dbo.Match_ColDiff;";
		const string CreateSchemaBoundView04 = "CREATE VIEW vw_SchemaBound04 WITH SCHEMABINDING AS SELECT Col1 FROM dbo.vw_SchemaBound03;";
		const string CreateSchemaBoundFunction01 = "CREATE FUNCTION fn_SchemaBound01() RETURNS TABLE WITH SCHEMABINDING AS RETURN (SELECT Col1 FROM dbo.Old);";
		const string CreateSchemaBoundFunction02 = "CREATE FUNCTION fn_SchemaBound02() RETURNS TABLE WITH SCHEMABINDING AS RETURN (SELECT Col1 FROM dbo.fn_SchemaBound01());";
		const string CreateSchemaBoundFunction03 = "CREATE FUNCTION fn_SchemaBound03() RETURNS TABLE WITH SCHEMABINDING AS RETURN (SELECT Col1 FROM dbo.Match_ColDiff);";
		const string CreateSchemaBoundFunction04 = "CREATE FUNCTION fn_SchemaBound04() RETURNS TABLE WITH SCHEMABINDING AS RETURN (SELECT Col1 FROM dbo.fn_SchemaBound03());";

		/// <summary>
		/// -----------------
		/// -- INSERT DATA --
		/// -----------------
		/// </summary>
		const string InsertTestDbDataScript = @"
			-- Old
			INSERT INTO Old VALUES (1,'A','Row One')
			INSERT INTO Old VALUES (2,'B','Row Two')
			INSERT INTO Old VALUES (3,'C','Row Three')
			-- Match_ColDiff
			INSERT INTO Match_ColDiff VALUES (1,'A','Row One','0.10', null        , '1800-11-17 13:51', 10.5, 'Y', NULL)
			INSERT INTO Match_ColDiff VALUES (2,'B','Row Two',null  , '1234567890', '2003-11-17 13:51', null, '#', NULL)
			INSERT INTO Match_ColDiff VALUES (3,'C',null     ,'3.99', 'ABCDEFGHIJ', '2090-11-17 13:51', 9   , 'N', NULL)
			-- MatcH_TableNameCaseDiff
			INSERT INTO MatcH_TableNameCaseDiff VALUES (1)
			-- Match_ColCaseDiff
			INSERT INTO Match_ColCaseDiff VALUES (1,'Row One')
			INSERT INTO Match_ColCaseDiff VALUES (2,null)
			-- Match_PKDiff
			INSERT INTO Match_PKDiff VALUES (1,'A','Row One',1)
			INSERT INTO Match_PKDiff VALUES (2,'B','Row Two',2)
			INSERT INTO Match_PKDiff VALUES (3,'C','Row Three',3)
			-- Match_IndexDiff
			INSERT INTO Match_IndexDiff VALUES (1,'A','Row One',1,1)
			INSERT INTO Match_IndexDiff VALUES (2,'B','Row Two',2,2)
			INSERT INTO Match_IndexDiff VALUES (3,'C','Row Three',3,3)
			-- Match_DefaultDiff
			INSERT INTO Match_DefaultDiff VALUES (1,'A','Row One',1)
			INSERT INTO Match_DefaultDiff VALUES (2,'B','Row Two',2)
			INSERT INTO Match_DefaultDiff VALUES (3,'C','Row Three',3)
			-- ClientTable
			INSERT INTO ClientTable VALUES (1,'A',null)
			INSERT INTO ClientTable VALUES (2,'B',null)
			INSERT INTO ClientTable VALUES (3,'C',null)
			-- ClientMatch
			INSERT INTO ClientMatch VALUES (1,'D',null, null)
			INSERT INTO ClientMatch VALUES (2,'E',1, null)
			INSERT INTO ClientMatch VALUES (3,'F',1, null)
			-- XmlTypeIssues
			INSERT INTO XmlTypeIssues VALUES (1, '<e>SingleElement</e>', '<e>whatever</e>', 'AnotherSingle', '<e>blah blah</e>')
			INSERT INTO XmlTypeIssues VALUES (2, null, '<e>line2</e>', 'SingleElement2', null)
			-- XmlTypeWithPkChange
			INSERT INTO XmlTypeWithPkChange VALUES (1, 'SingleElement')
			INSERT INTO XmlTypeWithPkChange VALUES (2, '<tag>whatever</tag>')

			INSERT INTO [JobDocAddress] ([E2_AddressType],[E2_Contact],[E2_Address1],[E2_Address2],[E2_City],[E2_Postcode],[E2_State],[E2_RN_NKCountryCode],[E2_Phone],[E2_Fax],[E2_ParentID],[E2_Mobile],[E2_CompanyName],[E2_ParentTableCode],[E2_Email])
			VALUES('CEA','Contact','Address1','Address2','Sydney','2020','NSW','AU','322223','322223','8D545E9C-917D-4B64-A7CA-E1EEC23C5ECB','322223','Company','DL','cw@gmail.com')

			INSERT INTO [SupplierBookingLine] (DL_PK) VALUES ('8D545E9C-917D-4B64-A7CA-E1EEC23C5ECB')
			";

		#endregion
	}
}
