using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoNAddInfoBusinessObjectTest : AutoAddInfoCodeTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneratedSourceCode()
		{
			var testObject = new AutoAddInfoBusinessObject(CreateInfo(CreateTestDataTable(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "Enterprise.Customs.BusinessObject", "Enterprise.Customs.Business"));
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString("Enterprise.BusinessObjectGenerator.Test.Testing.AddInfo.TestGeneratedAutoNAddInfoBusinessObject.cs", System.Text.Encoding.UTF8);
			AssertMultilineASCIIEquals(str, testObject.SourceCode.Trim());
		}

		protected override DataTable CreateTestDataTable()
		{
			var result = new DataTable("ZZDummyBizo");
			result.Columns.AddRange(
				new DataColumn[]
				{
					new DataColumn("Z0_PK", typeof(Guid)),
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
				});

			return result;
		}

		protected override Dictionary<string, string> CreateTestDbTypes()
		{
			var result = new Dictionary<string, string>
			{
					{ "Z0_PK", "uniqueidentifier" },
					{ "Z0_NAddInfoString35", "nvarchar" },
					{ "Z0_NAddInfoString3", "nvarchar" },
					{ "Z0_NAddInfoDecimal122", "decimal" },
					{ "Z0_NAddInfoDecimal073", "decimal" },
					{ "Z0_NAddInfoInt16", "smallint" },
					{ "Z0_NAddInfoInt32", "int" },
					{ "Z0_NAddInfoBool", "bit" },
					{ "Z0_NAddInfoGuid", "uniqueidentifier" },
					{ "Z0_NAddInfoDateTime", "datetime" },
					{ "Z0_NAddInfoDate", "date" }
				};

			return result;
		}
	}
}
