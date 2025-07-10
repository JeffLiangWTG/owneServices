using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class PrimaryKeyFilterTest : TestCase
	{
		public void TestJsonConverter()
		{
			var gui = Guid.NewGuid();
			var pkf = new PrimaryKeyFilter("MyPrimaryKey", gui);

			var result = JsonConverterHelper.Serialize(pkf);
			var deserialisedFilter = JsonConverterHelper.Deserialize<PrimaryKeyFilter>(result);

			var parameterList = deserialisedFilter.SqlParameters();
			AssertEquals(gui.ToString(), parameterList[0].Value.ToString());
			AssertEquals("MyPrimaryKey", deserialisedFilter.GetFieldName());
		}
	}
}
