using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Schema.Testing
{
	public class EnterpriseSchemaTest : ThreadSafeAccessTestCase
	{
		#region GetPkColumn

		public void TestGetPkColumn()
		{
			Assertion.AssertEquals("GetPKColumn", "OH_PK", schema.GetPkColumn("OrgHeader").Name);
			Assertion.Assert("Result of GetPKColumn should be cached for performance", schema.GetPkColumn("OrgHeader") == schema.GetPkColumn("OrgHeader"));
		}

		#endregion

		#region GetSchemaColumn

		public void TestSchemaColumnExists()
		{
			Assertion.AssertEquals("SchemaColumnExists for existing column", true, schema.SchemaColumnExists(OrgHeaderSchema.OH_Code.Name, "OrgHeader"));
			Assertion.AssertEquals("SchemaColumnExists for non-existant column", false, schema.SchemaColumnExists("NonExistantColumn", "OrgHeader"));
			Assertion.AssertEquals("SchemaColumnExists for non-existant table", false, schema.SchemaColumnExists(OrgHeaderSchema.OH_Code.Name, "NonExistantTable"));
		}

		public void TestGetSchemaColumn()
		{
			Assertion.AssertEquals("GetSchemaColumn", OrgHeaderSchema.OH_Code, schema.GetSchemaColumn(OrgHeaderSchema.OH_Code.Name, "OrgHeader"));
			Assertion.Assert("Result of GetSchemaColumn should be cached for performance", schema.GetSchemaColumn(OrgHeaderSchema.OH_Code.Name, "OrgHeader") == schema.GetSchemaColumn(OrgHeaderSchema.OH_Code.Name, "OrgHeader"));
		}

		[ExpectException(typeof(InvalidSchemaColumnException))]
		public void TestGetSchemaColumn_WhenColumnNotExists()
		{
			schema.GetSchemaColumn("OH_NonExistant", "OrgHeader");
		}

		public void TestGetSchemaColumnSafe()
		{
			Assertion.AssertEquals("GetSchemaColumnSafe", OrgHeaderSchema.OH_Code, schema.GetSchemaColumnSafe(OrgHeaderSchema.OH_Code.Name, "OrgHeader"));
		}

		public void TestGetSchemaColumnSafe_WhenColumnNotExists()
		{
			Assertion.AssertEquals("GetSchemaColumnSafe when column doesnt exist", null, schema.GetSchemaColumnSafe("OH_NonExistant", "OrgHeader"));
		}

		#endregion

		#region GetSchemaColumns

		public void TestGetSchemaColumns()
		{
			SchemaColumnCollection columns = schema.GetSchemaColumns("OrgHeader");
			Assertion.AssertEquals("GetSchemaColumns", true, columns.Count > 10);
			Assertion.AssertEquals("GetSchemaColumns", "OH_PK", columns[0].Name);
		}

		#endregion

		#region GetColumnNamePrefix

		public void TestGetColumnNamePrefix()
		{
			Assertion.AssertEquals("GetColumnNamePrefix", "OH", schema.GetColumnNamePrefix("OrgHeader"));
			Assertion.AssertEquals("GetColumnNamePrefix for non-existant table", null, schema.GetColumnNamePrefix("NonExistantTable"));
		}

		#endregion

		#region GetTableSchemaFromColumnNamePrefix

		public void TestGetTableSchemaFromColumnNamePrefix()
		{
			Assertion.AssertEquals("GetTableSchemaFromColumnNamePrefix", "OrgHeader", EnterpriseSchema.GetTableSchemaFromColumnNamePrefix("OH").TableName);
			Assertion.AssertEquals("GetTableSchemaFromColumnNamePrefix for non-existant prefix", null, EnterpriseSchema.GetTableSchemaFromColumnNamePrefix("11"));
		}

		#endregion

		#region GetForeignKeyTableSchemaFromColumnName

		public void TestGetForeignKeyTableSchemaFromColumnName()
		{
			AssertEquals("Wrong column name: ZZZZ_ZZZZ", null, schema.GetForeignKeyTableSchemaFromColumnName("ZZZZ_ZZZZ"));
			AssertEquals("Non-existent schema: ZZZ_WWW", null, schema.GetForeignKeyTableSchemaFromColumnName("ZZZ_WWW"));
			AssertEquals("ZD1_Z0", DummyBizoSchema.Constants.TableName, schema.GetForeignKeyTableSchemaFromColumnName("ZD1_Z0").TableName);
		}

		#endregion

		#region AssemblyMetaDataFile

		public void TestEnterpriseSchema_Matches_AssemblyMetaDataFile()
		{
			var excludedTables = new HashSet<string> { "EdiReportingQueue", "EdiERequestDocumentQueue", "IncidentMain", "LicenceDatabase", "EdiIdentityCertificate", "EdiTokenAuthOnBoardingData", "EdiIdentityApplication" };

			var assemblyMetaDataFiles =
				Directory.GetFiles(AssemblyLoader.GetBinPath(), "*AssemblyMetaData.xml", SearchOption.TopDirectoryOnly);

			var allNudgingTableNames = new HashSet<string>();

			foreach (var metaDataFile in assemblyMetaDataFiles)
			{
				var xdoc = XDocument.Load(metaDataFile);
				var nudgingTableNames = xdoc.XPathSelectElements("/AssemblyMetaData//Table").Select(x => x.Value)
					.ToHashSet();

				allNudgingTableNames.UnionWith(nudgingTableNames);
			}

			allNudgingTableNames.RemoveWhere(excludedTables.Contains);
			var missingTableNames = string.Join(Environment.NewLine, allNudgingTableNames.Where(table => EnterpriseSchema.GetTableSchema(table) == null));

			AssertEquals("AutoEnterpriseSchema missing following table names:", string.Empty, missingTableNames);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			schema = new EnterpriseSchemaResolver();
		}

		IApplicationSchemaResolver schema;
	}
}
