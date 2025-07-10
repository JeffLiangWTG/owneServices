using System;
using System.Data;
using CargoWise.IO;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoAddInfoBusinessObjectSchemaTest : AutoAddInfoCodeTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneratedSourceCode()
		{
			string[] indexes = { "NR_UX__AB_Code", "NR_UX__RZ_Code" };
			string[] literalOnlyColumns = { "TT_Bit", "TT_Guid", "TT_SmallDateTime" };
			string[] nonBlankFilteredIndexColumns = { "TT_String" };
			var info = CreateInfo(CreateTestDataTableForView(), indexes, literalOnlyColumns, nonBlankFilteredIndexColumns);
			var schema = new AutoAddInfoBusinessObjectSchema(info);
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var str = resourceRetriever.GetString("Enterprise.BusinessObjectGenerator.Test.Testing.AddInfo.TestGeneratedAutoAddInfoBusinessObjectSchema.cs", System.Text.Encoding.UTF8);
			AssertMultilineASCIIEquals(str, schema.SourceCode.Trim());
		}

		DataTable CreateTestDataTableForView()
		{
			var result = new DataTable("TestTable");
			result.Columns.AddRange(
				new DataColumn[]
				{
					new DataColumn("TT_PK", typeof(Guid)),
					new DataColumn("TT_Char", typeof(string)) { MaxLength = 3 },
					new DataColumn("TT_DateTime", typeof(DateTime)),
					new DataColumn("TT_DateTime2", typeof(DateTime)),
					new DataColumn("TT_DateTimeOffset", typeof(DateTimeOffset)),
					new DataColumn("TT_Date", typeof(DateTime)),
					new DataColumn("TT_Decimal", typeof(decimal)),
					new DataColumn("TT_Int32", typeof(int)),
					new DataColumn("TT_Money", typeof(decimal)),
					new DataColumn("TT_UnicodeString", typeof(string)) { MaxLength = 10 },
					new DataColumn("TT_SmallDateTime", typeof(DateTime)),
					new DataColumn("TT_Int16", typeof(short)),
					new DataColumn("TT_Byte", typeof(byte)),
					new DataColumn("TT_Guid", typeof(Guid)),
					new DataColumn("TT_String", typeof(string)),
					new DataColumn("TT_Xml", typeof(string)),
					new DataColumn("TT_Binary", typeof(byte[])),
					new DataColumn("TT_FixedBinary", typeof(byte[])),
					new DataColumn("TT_Bit", typeof(bool)),
					new DataColumn("TT_BigInt", typeof(long)),
					new DataColumn("TT_Geography", typeof(SqlGeography)),
				});

			return result;
		}
	}
}
