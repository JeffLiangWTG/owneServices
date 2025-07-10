using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.IO;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoAddInfoBusinessObjectWithChildTableTest : AutoAddInfoCodeTestCase
	{
		public void TestGeneratedSourceCode()
		{
			var childInfo = CreateInfo(CreateChildDataTable(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
				"Enterprise.Customs.BusinessObject", "Enterprise.Customs.Business");
			var testObject = new AutoAddInfoBusinessObject(CreateInfo(CreateTestDataTable(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
				"Enterprise.Customs.BusinessObject", "Enterprise.Customs.Business",
				childTableName: "ChildBusinessObject", childTableProperties: new AutoPropertyList(childInfo).Properties.ToArray()));
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString("Enterprise.BusinessObjectGenerator.Test.Testing.AddInfo.TestGeneratedAutoAddInfoBusinessObjectWithChildTable.cs", System.Text.Encoding.UTF8);
			AssertMultilineASCIIEquals(str, testObject.SourceCode.Trim());
		}

		DataTable CreateChildDataTable()
		{
			var result = new DataTable("ChildBusinessObject");
			result.Columns.AddRange(
				[
					new DataColumn("ZZ_PK", typeof(Guid)),
					new DataColumn("ZZ_AddInfoString35", typeof(string)) { MaxLength = 35 },
					new DataColumn("ZZ_AddInfoString3", typeof(string)) { MaxLength = 3 },
					new DataColumn("ZZ_AddInfoDecimal122", typeof(decimal)),
					new DataColumn("ZZ_AddInfoDecimal073", typeof(decimal)),
					new DataColumn("ZZ_AddInfoInt16", typeof(short)),
					new DataColumn("ZZ_AddInfoInt32", typeof(int)),
					new DataColumn("ZZ_AddInfoBool", typeof(bool)),
					new DataColumn("ZZ_AddInfoGuid", typeof(Guid)),
					new DataColumn("ZZ_AddInfoDateTime", typeof(DateTime)),
					new DataColumn("ZZ_AddInfoDate", typeof(DateTime)),
					new DataColumn("ZZ_NAddInfoString35", typeof(string)) { MaxLength = 35 },
					new DataColumn("ZZ_NAddInfoString3", typeof(string)) { MaxLength = 3 },
					new DataColumn("ZZ_NAddInfoDecimal122", typeof(decimal)),
					new DataColumn("ZZ_NAddInfoDecimal073", typeof(decimal)),
					new DataColumn("ZZ_NAddInfoInt16", typeof(short)),
					new DataColumn("ZZ_NAddInfoInt32", typeof(int)),
					new DataColumn("ZZ_NAddInfoBool", typeof(bool)),
					new DataColumn("ZZ_NAddInfoGuid", typeof(Guid)),
					new DataColumn("ZZ_NAddInfoDateTime", typeof(DateTime)),
					new DataColumn("ZZ_NAddInfoDate", typeof(DateTime)),
				]);

			return result;
		}

		protected override DataTable CreateTestDataTable()
		{
			var result = new DataTable("ZZDummyBizo");
			result.Columns.AddRange(
				[
					new DataColumn("Z0_PK", typeof(Guid)),
					new DataColumn("Z0_AddInfoString35", typeof(string)) { MaxLength = 35 },
					new DataColumn("Z0_AddInfoString3", typeof(string)) { MaxLength = 3 },
					new DataColumn("Z0_AddInfoDecimal122", typeof(decimal)),
					new DataColumn("Z0_AddInfoDecimal073", typeof(decimal)),
					new DataColumn("Z0_AddInfoInt16", typeof(short)),
					new DataColumn("Z0_AddInfoInt32", typeof(int)),
					new DataColumn("Z0_AddInfoBool", typeof(bool)),
					new DataColumn("Z0_AddInfoGuid", typeof(Guid)),
					new DataColumn("Z0_AddInfoDateTime", typeof(DateTime)),
					new DataColumn("Z0_AddInfoDate", typeof(DateTime)),
					new DataColumn("Z0_NAddInfoString35", typeof(string)) { MaxLength = 35 },
					new DataColumn("Z0_NAddInfoString3", typeof(string)) { MaxLength = 3 },
					new DataColumn("Z0_NAddInfoDecimal122", typeof(decimal)),
					new DataColumn("Z0_NAddInfoDecimal073", typeof(decimal)),
					new DataColumn("Z0_NAddInfoInt16", typeof(short)),
					new DataColumn("Z0_NAddInfoInt32", typeof(int)),
					new DataColumn("Z0_NAddInfoBool", typeof(bool)),
					new DataColumn("Z0_NAddInfoGuid", typeof(Guid)),
					new DataColumn("Z0_NAddInfoDateTime", typeof(DateTime)),
					new DataColumn("Z0_NAddInfoDate", typeof(DateTime)),
				]);

			return result;
		}

		protected override Dictionary<string, string> CreateTestDbTypes()
		{
			var result = new Dictionary<string, string>
			{
					{ "Z0_PK", "uniqueidentifier" },
					{ "Z0_AddInfoString35", "varchar" },
					{ "Z0_AddInfoString3", "varchar" },
					{ "Z0_AddInfoDecimal122", "decimal" },
					{ "Z0_AddInfoDecimal073", "decimal" },
					{ "Z0_AddInfoInt16", "smallint" },
					{ "Z0_AddInfoInt32", "int" },
					{ "Z0_AddInfoBool", "bit" },
					{ "Z0_AddInfoGuid", "uniqueidentifier" },
					{ "Z0_AddInfoDateTime", "datetime" },
					{ "Z0_AddInfoDate", "date" },
					{ "Z0_NAddInfoString35", "nvarchar" },
					{ "Z0_NAddInfoString3", "nvarchar" },
					{ "Z0_NAddInfoDecimal122", "decimal" },
					{ "Z0_NAddInfoDecimal073", "decimal" },
					{ "Z0_NAddInfoInt16", "smallint" },
					{ "Z0_NAddInfoInt32", "int" },
					{ "Z0_NAddInfoBool", "bit" },
					{ "Z0_NAddInfoGuid", "uniqueidentifier" },
					{ "Z0_NAddInfoDateTime", "datetime" },
					{ "Z0_NAddInfoDate", "date" },
					{ "ZZ_PK", "uniqueidentifier" },
					{ "ZZ_AddInfoString35", "varchar" },
					{ "ZZ_AddInfoString3", "varchar" },
					{ "ZZ_AddInfoDecimal122", "decimal" },
					{ "ZZ_AddInfoDecimal073", "decimal" },
					{ "ZZ_AddInfoInt16", "smallint" },
					{ "ZZ_AddInfoInt32", "int" },
					{ "ZZ_AddInfoBool", "bit" },
					{ "ZZ_AddInfoGuid", "uniqueidentifier" },
					{ "ZZ_AddInfoDateTime", "datetime" },
					{ "ZZ_AddInfoDate", "date" },
					{ "ZZ_NAddInfoString35", "nvarchar" },
					{ "ZZ_NAddInfoString3", "nvarchar" },
					{ "ZZ_NAddInfoDecimal122", "decimal" },
					{ "ZZ_NAddInfoDecimal073", "decimal" },
					{ "ZZ_NAddInfoInt16", "smallint" },
					{ "ZZ_NAddInfoInt32", "int" },
					{ "ZZ_NAddInfoBool", "bit" },
					{ "ZZ_NAddInfoGuid", "uniqueidentifier" },
					{ "ZZ_NAddInfoDateTime", "datetime" },
					{ "ZZ_NAddInfoDate", "date" }
				};

			return result;
		}
	}
}
