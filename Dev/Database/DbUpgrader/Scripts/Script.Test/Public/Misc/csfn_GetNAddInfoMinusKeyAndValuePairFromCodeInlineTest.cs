using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(csfn_GetNAddInfoMinusKeyAndValuePairFromCodeInline))]
	class csfn_GetNAddInfoMinusKeyAndValuePairFromCodeInlineTest : DbCreateScriptTest
	{
		public void TestGetNAddInfoMinusKeyAndValuePairFromCodeInline()
		{
			var sql = @"SELECT AddInfoValue, Value FROM csfn_GetNAddInfoMinusKeyAndValuePairFromCodeInline(N'GoodsSpecModel=1|3|A|B|X|10KG¤6CAN/BOX|D|E|G1|C1|其他*NameOfGoods=胡萝卜素', 'GoodsSpecModel')";
			using (var command = TestConnection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals("NameOfGoods=胡萝卜素", reader["AddInfoValue"].ToString());
				AssertEquals("1|3|A|B|X|10KG*6CAN/BOX|D|E|G1|C1|其他", reader["Value"].ToString());
				AssertEquals(false, reader.Read());
			}
		}
	}
}
