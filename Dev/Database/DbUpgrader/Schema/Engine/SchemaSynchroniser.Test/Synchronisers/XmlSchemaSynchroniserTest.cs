using System;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class XmlSchemaSynchroniserTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		#region TestSynchroniseXmlSchemas

		public void TestSynchroniseXmlSchemas()
		{
			AssertPreConditions();
			SynchroniseMockMainDbXmlSchemas();
			AssertXmlSchemasSynchronised();
		}

		void SynchroniseMockMainDbXmlSchemas()
		{
			XmlSchemaSynchroniserForTesting testSynchroniser = new XmlSchemaSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.SynchroniseXmlSchemas);
		}

		/// <summary>
		/// PRE-CONDITION Assertions (Asserts schema before synchronisation)
		/// </summary>
		void AssertPreConditions()
		{
			AssertXmlSchemaCollection("XmlSchemaCollection01", xsdSchema1);
			AssertXmlSchemaCollection("XmlSchemaCollection02", xsdSchema2);
			AssertXmlSchemaCollection("XmlSchemaCollection03", xsdSchema34);
			AssertXmlSchemaCollection("XmlSchemaCollection04", null);

			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml01", "XmlSchemaCollection01");
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml02", "XmlSchemaCollection02");
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml03", "XmlSchemaCollection03");
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml04", null);
		}

		void AssertXmlSchemasSynchronised()
		{
			AssertXmlSchemaCollection("XmlSchemaCollection01", xsdSchema1);
			AssertXmlSchemaCollection("XmlSchemaCollection02", xsdSchema2New);
			AssertXmlSchemaCollection("XmlSchemaCollection03", null);
			AssertXmlSchemaCollection("XmlSchemaCollection04", xsdSchema34);

			// Columns are not synchronised in this class, hence new and modified XML schemas 
			// are not (re)linked to columns (ColumnSynchroniser does it)
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml01", "XmlSchemaCollection01");
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml02", null);
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml03", null);
			AssertColumnXmlSchema("TableWithXmlFields", "T1_Xml04", null);

			// Assert data preserved
			AssertEquals("TableWithXmlFields row count", 1, GetRowCount("TableWithXmlFields"));
			AssertEquals("(T1_Xml01 = <firstelement>abc</firstelement>) row count", 1, GetRowCountWithGivenXmlValue("TableWithXmlFields", "T1_Xml01", "<firstelement>abc</firstelement>"));
			AssertEquals("(T1_Xml02 is null) row count", 1, GetRowCountWithNullValues("TableWithXmlFields", "T1_Xml02"));
			AssertEquals("(T1_Xml03 = <anelement>3</anelement>) row count", 1, GetRowCountWithGivenXmlValue("TableWithXmlFields", "T1_Xml03", "<anelement>3</anelement>"));
			AssertEquals("(T1_Xml04 is null) row count", 1, GetRowCountWithNullValues("TableWithXmlFields", "T1_Xml04"));
		}

		#endregion

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			string[] createScripts = new string[] { createTestMainDbXmlSchemasScript, createTestMainDbObjectsScript };
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createScripts);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			string[] createScripts = new string[] { createTestTemplateDbXmlSchemasScript, createTestTemplateDbObjectsScript };
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createScripts);
		}

		void AssertXmlSchemaCollection(string collectionName, string expectedContents)
		{
			string sqlText = String.Format(@"
				IF exists (SELECT name FROM [{0}].sys.xml_schema_collections WHERE name = '{1}')
					SELECT XML_SCHEMA_NAMESPACE('dbo', '{1}');
				ELSE
					SELECT null;",
				mockMainDb, collectionName);

			object scalarResult = UpgCommandRunner.RunScalarCommandOnGivenDb(TestConnection, mockMainDb, sqlText);
			string actualContents = (scalarResult == DBNull.Value) ? null : scalarResult.ToString();
			AssertEquals(collectionName, expectedContents, actualContents);
		}

		void AssertColumnXmlSchema(string tableName, string columnName, string expectedXmlSchema)
		{
			string actualXmlSchema = XmlSchemaSynchroniserForTesting.GetColumnXmlSchema(TestConnection, mockMainDb, tableName, columnName);
			AssertEquals("Column " + tableName + "." + columnName + " XML schema", expectedXmlSchema, actualXmlSchema);
		}

		int GetRowCount(string tableName)
		{
			string sqlText = String.Format("SELECT count(*) FROM [{0}]..[{1}]", mockMainDb, tableName);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		int GetRowCountWithNullValues(string tableName, string columnName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}]..[{1}]
				WHERE {2} is null",
				mockMainDb, tableName, columnName);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		int GetRowCountWithGivenXmlValue(string tableName, string columnName, string expectedXmlValue)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}]..[{1}]
				WHERE convert(varchar(max), {2}) = '{3}'",
				mockMainDb, tableName, columnName, expectedXmlValue);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		#region Scripts

		const string xsdSchema1 = @"<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""firstelement"" type=""xsd:string"" /><xsd:complexType name=""constantValue""><xsd:complexContent><xsd:restriction base=""xsd:anyType""><xsd:sequence /><xsd:attribute name=""value"" type=""xsd:string"" use=""required"" /></xsd:restriction></xsd:complexContent></xsd:complexType></xsd:schema>";
		const string xsdSchema2 = @"<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:attribute name=""anElement"" type=""xsd:byte"" /></xsd:schema>";
		const string xsdSchema34 = @"<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""anelement"" type=""xsd:byte"" /></xsd:schema>";

		const string createTestMainDbXmlSchemasScript =
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection01 AS '" + xsdSchema1 + "';\r\n" +
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection02 AS '" + xsdSchema2 + "';\r\n" +
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection03 AS '" + xsdSchema34 + "';\r\n";

		const string createTestMainDbObjectsScript = @"
			-- This table will have changes in the XML schemas its fields use
			CREATE TABLE TableWithXmlFields
			(
				T1_PK UNIQUEIDENTIFIER NOT NULL,
				T1_Xml01 XML (CONTENT XmlSchemaCollection01) NOT NULL,
				T1_Xml02 XML (CONTENT XmlSchemaCollection02) NULL,
				T1_Xml03 XML (CONTENT XmlSchemaCollection03) NOT NULL DEFAULT (''),
				T1_Xml04 XML NULL
			)
			;

			-- Index to ensure dependent objects are dropped when a column has to be modified (unlinked from XML schema)
			ALTER TABLE TableWithXmlFields
				ADD CONSTRAINT PK_TableWithXmlFields PRIMARY KEY CLUSTERED (T1_PK)
			;
			CREATE PRIMARY XML INDEX IX_TableWithXmlFields_T1_Xml02 ON TableWithXmlFields (T1_Xml02) 
			;

			-- Insert TableWithXmlFields Data
			INSERT INTO TableWithXmlFields VALUES (newid(), '<firstelement>abc</firstelement>', null , '<anelement>3</anelement>', null)
			;
			";

		const string xsdSchema2New = @"<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:attribute name=""element1"" type=""xsd:string"" /><xsd:attribute name=""element2"" type=""xsd:byte"" /></xsd:schema>";

		const string createTestTemplateDbXmlSchemasScript =
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection01 AS '" + xsdSchema1 + "';\r\n" +
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection02 AS '" + xsdSchema2New + "';\r\n" +
			"CREATE XML SCHEMA COLLECTION XmlSchemaCollection04 AS '" + xsdSchema34 + "';\r\n";

		const string createTestTemplateDbObjectsScript = @"
			-- This table had changes in the XML schemas related to it.
			-- This changes will not be synchronised by this class. ColumnSynchroniser should do it.
			CREATE TABLE TableWithXmlFields
			(
				T1_PK UNIQUEIDENTIFIER NOT NULL,
				T1_Xml01 XML (CONTENT XmlSchemaCollection01) NOT NULL,
				T1_Xml02 XML (CONTENT XmlSchemaCollection02) NULL,
				T1_Xml03 XML (CONTENT XmlSchemaCollection04) NOT NULL,
				T1_Xml04 XML (CONTENT XmlSchemaCollection04) NULL
			)
			;
			";

		#endregion

		#endregion
	}
}
