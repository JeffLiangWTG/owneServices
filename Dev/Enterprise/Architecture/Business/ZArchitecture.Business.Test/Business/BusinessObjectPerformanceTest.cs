using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing.Business
{
	sealed class BusinessObjectPerformanceTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestPerformance()
		{
			var bizOList = new List<BusinessObject>(50000);
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < 50000; i++)
			{
				bizOList.Add(Factory.New<DummyBusinessObject>());
			}

			var creation = sw.Elapsed;

			sw.Restart();
			foreach (var bizO in bizOList)
			{
				bizO.RunPreSaveValidation();
			}
			var validation = sw.Elapsed;

			sw.Restart();
			Factory.Save();
			var save = sw.Elapsed;

			sw.Restart();
			var factory2 = new BusinessObjectFactory();
			var items = factory2.Load<DummyBusinessObject>(new ZQuery());
			var load = sw.Elapsed;

			sw.Restart();
			foreach (var bizO in bizOList)
			{
				bizO.RunPreSaveValidation();
			}
			var validation2 = sw.Elapsed;
			Fail($"Verify you are 'happy' with these results.\r\nCreation:{creation},Validation:{validation},Save:{save},Load:{load},Validation2:{validation2}");
		}
	}
}
