using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoBusinessObjectSchemaTest : AutoCodeTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneratedSourceCode()
		{
			var testBuildXmlFile = Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\Builder\BusinessObjectGenerator.Test\Testing\TestBuild.xml");
			BuildXml.SetInstanceForTesting(new BuildXml(testBuildXmlFile));

			string[] indexes = { "NR_UX__AB_Code", "NR_UX__RZ_Code" };
			string[] literalOnlyColumns = { "TT_Bit", "TT_Guid", "TT_SmallDateTime" };
			string[] nonBlankFilteredIndexColumns = { "TT_String" };
			BusinessObjectInfo info = CreateInfo(CreateTestDataTable(), indexes, literalOnlyColumns, nonBlankFilteredIndexColumns);
			AutoBusinessObjectSchema schema = new AutoBusinessObjectSchema(info);
			AssertASCIIFileSameAsString(BaseSourcePath + @"Enterprise\Product\Core\Builder\BusinessObjectGenerator.Test\Testing\TestGeneratedAutoBusinessObjectSchema.cs", schema.SourceCode.Trim());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneratedSourceCodeWithNumericName()
		{
			DataTable table = new DataTable("TestTable");

			DataColumn tT_PKColumn = new DataColumn("TT_PK", typeof(Guid));
			table.Columns.Add(tT_PKColumn);
			table.PrimaryKey = new DataColumn[] { tT_PKColumn };

			table.Columns.AddRange(
				new DataColumn[]
				{
					new DataColumn("TT_RX_98Guid", typeof(Guid)),
					new DataColumn("TT_RX_NK98String", typeof(string)),
			});
			Dictionary<string, string> dbTypes = new Dictionary<string, string>();
			dbTypes.Add("TT_PK", "uniqueidentifier");
			dbTypes.Add("TT_RX_98Guid", "uniqueidentifier");
			dbTypes.Add("TT_RX_NK98String", "varchar");
			var info = new BusinessObjectInfo(
				fileName: "DummyFileName"
				, isInZArchitectureSolution: false
				, isPersistent: true
				, @namespace: "Enterprise"
				, namespaceOfSchema: "Enterprise"
				, baseClassName: "BusinessObject"
				, sqlSchemaName: Db.SqlDbOwnerSchema
				, table: table
				, decimalScaleTable: new DataTable()
				, foreignKeysTable: new DataTable()
				, dateTimeOffsetTable: new DataTable()
				, refDbCountry: ""
				, refDbType: null
				, dbTypes: dbTypes
				, uniqueKeys: new HashSet<string> { { "TT_PK" } }
				, canForceUpdateNaturalKeyCacheColumns: new HashSet<string>()
				, masterFiles: new BuildXmlBizOEntryCollection()
				, pkIndex: null
				, indexes: Array.Empty<string>()
				, masterFileReference: true
				, preventDelete: false
				, literalOnlyColumns: Array.Empty<string>()
				, nonBlankFilteredIndexColumns: Array.Empty<string>()
				, tables: new Dictionary<string, ITableInfo>());
			AutoBusinessObjectSchema schema = new AutoBusinessObjectSchema(info);
			AssertASCIIFileSameAsString(BaseSourcePath + @"Enterprise\Product\Core\Builder\BusinessObjectGenerator.Test\Testing\TestGeneratedAutoBusinessObjectSchemaWithNumericName.cs", schema.SourceCode.Trim());
		}

		public void TestGeneratedSourceCodeForRefDbTableContainsPkIndexInfo()
		{
			var bizObjInfo = AutoPropertyTest.GetRefDbTableBizObjInfo(RefDbTypeEnum.Single, refCountryCode: null);
			var bizObjSchema = new AutoBusinessObjectSchema(bizObjInfo);
			var sourceCode = bizObjSchema.SourceCode;
			var pkInfoMatch = Regex.Match(sourceCode, @"PK\s*=[^;]+(?<indexed>true|false)\);");

			AssertEquals("Generate Source Code should have PK info, but it doesn't.\r\n\r\n" + sourceCode, true, pkInfoMatch.Success);
			AssertEquals("Generated SchemaPKColumn should NOT be indexed as it comes from wrapping version view (Vx)", "false", pkInfoMatch.Groups["indexed"].Value);
		}
	}
}
