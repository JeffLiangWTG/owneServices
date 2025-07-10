using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.BuildTools;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoAddInfoBusinessObjectTest : AutoAddInfoCodeTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneratedSourceCode()
		{
			var testObject = new AutoAddInfoBusinessObject(CreateInfo(
				CreateTestDataTable(),
				Array.Empty<string>(),
				Array.Empty<string>(),
				Array.Empty<string>(),
				"Enterprise.Customs.BusinessObject",
				"Enterprise.Customs.Business",
				false,
				new BuildXmlBizOEntryCollection
				{
					new BuildXmlBizOEntry("", "", "RefCurrency", "MasterFiles", false, false, false)
				}));
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString("Enterprise.BusinessObjectGenerator.Test.Testing.AddInfo.TestGeneratedAutoAddInfoBusinessObject.cs", System.Text.Encoding.UTF8);
			AssertMultilineASCIIEquals(str, testObject.SourceCode.Trim());
		}

		protected override DataTable CreateTestDataTable()
		{
			var result = new DataTable("ZZDummyBizo");
			result.Columns.AddRange(
				new DataColumn[]
				{
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
					new DataColumn("Z0_RX_NKCurrency", typeof(string)) { MaxLength = 3 },
				});

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
					{ "Z0_RX_NKCurrency", "varchar" },
				};

			return result;
		}
	}
}
