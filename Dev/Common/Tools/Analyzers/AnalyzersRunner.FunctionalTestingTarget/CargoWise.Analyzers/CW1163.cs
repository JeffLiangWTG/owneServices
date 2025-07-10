using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Used for testing CW1163")]
	class LazyClass
	{
		public static LazyClass ExpensiveMethod()
		{
			return new LazyClass();
		}
	}

	class MyBadClass
	{
		// CW1163 Do not initialize Lazy with method invocation
		readonly Lazy<LazyClass> lazy = new Lazy<LazyClass>(() => LazyClass.ExpensiveMethod());

		// CW1163 Do not initialize Lazy with method invocation
		readonly Lazy<LazyClass> lazyImp = new (() => LazyClass.ExpensiveMethod());

		public Lazy<LazyClass> Lazy => lazy;

		public Lazy<LazyClass> LazyImp => lazyImp;
	}
}
