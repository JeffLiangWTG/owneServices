using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Testing
{
	[TestedType(typeof(GetNextDateAsSmallDateTime))]
	class GetNextDateAsSmallDateTimeTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			CombineAssertions(() =>
			{
				AssertNextDateResult(new SqlDateTime(2020, 11, 19));
				AssertNextDateResult(new SqlDateTime(2020, 11, 19, 10, 11, 12));
				AssertNextDateResult(new SqlDateTime(2020, 11, 19, 23, 59, 0));
				AssertNextDateResult(new SqlDateTime(DateTime.UtcNow));
				GetNextDateTestHelper.AssertReturnsNullOnNullInput(TestConnection, ScriptToTest);
			});
		}

		void AssertNextDateResult(SqlDateTime inputDateTime) => GetNextDateTestHelper.AssertNextDateResult(TestConnection, ScriptToTest, inputDateTime);

		public void TestReturnTypeIsSmallDateTime()
		{
			GetNextDateTestHelper.AssertReturnType(TestConnection, ScriptToTest, "smalldatetime");
		}
	}
}

