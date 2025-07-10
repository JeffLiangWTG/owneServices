using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnsUniqueValidatorTest : TestCaseWithFactory
	{
		public void TestHasError()
		{
			var header = Factory.NewWithValidTestData<CusOutturnHeader>();
			var outturns = new HashSet<DepotCusOutturn>();
			for (var i = 0; i < 1000; i++)
			{
				var outturn = header.Outturns.AddNew();
				outturn.FillWithValidTestData();
				outturns.Add(outturn);
			}

			var validator = new DepotCusOutturnsUniqueValidator(outturns);

			var sw = Stopwatch.StartNew();

			validator.BuildRepeatedElements();

			sw.Stop();
			AssertLessThan($"Should take less than 10 seconds with 1000 data as it's using Parallel in the cached list building part.", sw.Elapsed.TotalSeconds, 10);

			sw.Restart();

			foreach (var outturn in header.Outturns)
			{
				_ = validator.HasError(outturn.PK);
			}

			sw.Stop();
			AssertLessThan($"Should take less than 3 seconds with 1000 data as it's using the cached list.", sw.Elapsed.TotalSeconds, 3);
		}
	}
}
