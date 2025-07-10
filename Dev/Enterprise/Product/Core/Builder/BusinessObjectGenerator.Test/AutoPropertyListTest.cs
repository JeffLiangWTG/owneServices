using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoPropertyListTest : TransactionedTestCase
	{
		public void TestAutoClass()
		{
			var query = @"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'cdc')
	EXEC (N'CREATE SCHEMA [cdc]');
CREATE TABLE [cdc].[CloneTable] (GS_Code char(3))
";
			TestConnection.ExecuteNonQuery(query);

			var dataInfo = new BusinessObjectInfo(
				fileName: "DummyFileName"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: new DataTable()
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: ""
				, refDbType: null
				, dbTypes: new Dictionary<string, string>()
				, uniqueKeys: new HashSet<string>()
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: new BuildXmlBizOEntryCollection()
				, pkIndex: null
				, indexes: Array.Empty<string>()
				, masterFileReference: true
				, preventDelete: false
				, literalOnlyColumns: Array.Empty<string>()
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());
			var autoPropertyList = new AutoPropertyList(dataInfo);

			var column = new DataColumn("SL_GS_NKUser");
			AssertEquals("Should return table from user schema", "GlbStaff", autoPropertyList.GetForeignKeyTableName(column, null));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUsesTypeNotTableName()
		{
			BuildXmlBizOEntryCollection collection = new BuildXmlBizOEntryCollection();
			BuildXmlBizOEntry entry = new BuildXmlBizOEntryOnMainDbForTesting("ProcessTasks", "MasterFiles", false, true, false);
			collection.Add(entry);

			DataTable dataTable = new DataTable("Property");
			var pK = new DataColumn("ID", typeof(Guid));
			dataTable.Columns.Add(pK);
			dataTable.Columns.Add(new DataColumn("Password", typeof(Guid)));
			dataTable.Columns.Add(new DataColumn("Email", typeof(Guid)));
			dataTable.Columns.Add(new DataColumn("FullName", typeof(string)));
			dataTable.Columns.Add(new DataColumn("Address", typeof(string)));
			dataTable.Columns.Add(new DataColumn("AA_Data", typeof(string)));
			dataTable.Columns.Add(new DataColumn("AA_1_AutoVersion", typeof(short)));
			dataTable.Columns.Add(new DataColumn("AA_2_AutoVersion", typeof(short)));

			dataTable.PrimaryKey = new DataColumn[] { pK };

			DataTable foreignKeys = new DataTable("ForeignKeys");
			foreignKeys.Columns.Add(new DataColumn("Property", typeof(string)));
			foreignKeys.Columns.Add(new DataColumn("ForeignTable", typeof(string)));
			foreignKeys.Columns.Add(new DataColumn("AB_AutoVersion", typeof(short)));

			var row = foreignKeys.NewRow();
			row["Property"] = "Password";
			row["ForeignTable"] = "ProcessTasks";
			foreignKeys.Rows.Add(row);

			var dataInfo = new BusinessObjectInfo(
				fileName: "DummyFileName"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: dataTable
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: foreignKeys
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: ""
				, refDbType: null
				, dbTypes: new Dictionary<string, string>()
				, uniqueKeys: new HashSet<string>()
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: collection
				, pkIndex: null
				, indexes: Array.Empty<string>()
				, masterFileReference: false
				, preventDelete: false
				, literalOnlyColumns: Array.Empty<string>()
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());

			string testBuildXmlFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\Builder\BusinessObjectGenerator.Test\Testing\TestBuild.xml");
			BuildXml.SetInstanceForTesting(new BuildXml(testBuildXmlFile));

			var autoPropertyList = new AutoPropertyList(dataInfo);
			AssertEquals(autoPropertyList.Properties.Count(), 5);
			AssertEquals(autoPropertyList.Properties.Any(p => p.ColumnName == "Email"), true);
			AssertEquals(autoPropertyList.Properties.Any(p => p.ColumnName == "Password"), true);
			AssertEquals(autoPropertyList.Properties.Any(p => p.ColumnName == "Address"), true);

			Assert("The type (ProcessTask), not the table name (ProcessTasks), should be the related business object generated.", autoPropertyList.CodeForProperties.Contains($"[RelatedBusinessObject(\"ProcessTask\")]"));
			Assert("The table name (ProcessTasks), not the type (ProcessTask), should be used for the lookup.", autoPropertyList.CodeForProperties.Contains($"[List(\"Lookups.ProcessTasks\")]"));
			Assert("ProcessTask should be the type and the name of the property", autoPropertyList.CodeForProperties.Contains($"public virtual ProcessTask ProcessTask"));
			Assert("Should be loading ProcessTask from the factory with type ProcessTask and PK Password", autoPropertyList.CodeForProperties.Contains($"get {{ return (ProcessTask) Factory.Load(typeof(ProcessTask), Password); }}"));
		}

		class BuildXmlBizOEntryOnMainDbForTesting : BuildXmlBizOEntry
		{
			public BuildXmlBizOEntryOnMainDbForTesting(string tableName, string solutionName, bool masterFileReference, bool preventDelete, bool convertZStringToWesternEuropeanCharacters = false)
				: base(null, null, tableName, solutionName, masterFileReference, preventDelete, convertZStringToWesternEuropeanCharacters)
			{
			}
		}
	}
}
