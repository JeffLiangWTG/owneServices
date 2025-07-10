using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	abstract class NumberFountainTestCase : DbCreateScriptTest
	{
		protected string FountainName = "TestFountainForScripts";
		protected string LongFountainName = string.Join(string.Empty, Enumerable.Range(1, 256).Select(i => i % 10).Select(i => i.ToString(CultureInfo.InvariantCulture)));
		protected Guid FountainOwner = Guid.Empty;

		protected void AssertNumbers(string message, IReadOnlyList<long> expected, IReadOnlyList<long> result)
		{
			AssertEquals(message, expected.Count, result.Count);
			for (var i = 0; i < expected.Count; i++)
			{
				AssertEquals(message, expected[i], result[i]);
			}
		}

		void FountainsCleanup()
		{
			Helper.DeleteFountain(TestConnection, FountainName, FountainOwner);
			Helper.DeleteFountain(TestConnection, LongFountainName, FountainOwner);

			DbCommitTracker.Ignore("/* FOUNTAIN CLEANUP */");
		}

		public override void RunBare()
		{
			FountainsCleanup();

			base.RunBare();
		}

		protected override void FinalTearDown()
		{
			FountainsCleanup();

			base.FinalTearDown();
		}
	}
}

