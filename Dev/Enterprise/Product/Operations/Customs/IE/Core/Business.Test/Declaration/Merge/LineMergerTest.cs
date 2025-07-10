using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class LineMergerTest : TestCaseWithFactory
	{
		public void TestGetNewDutyCalculatorStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lineMerger = new LineMerger(declaration);

			var dutyCalculatorStrategy = typeof(LineMerger).GetMethod("GetNewDutyCalculatorStrategy", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(lineMerger, Array.Empty<object>());
			AssertType<DutyCalculatorStrategy>("GetNewDutyCalculatorStrategy should return IE DutyCalculatorStrategy", dutyCalculatorStrategy);
		}
	}
}
