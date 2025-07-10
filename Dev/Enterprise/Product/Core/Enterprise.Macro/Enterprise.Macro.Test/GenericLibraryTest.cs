using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Macro.Test
{
	sealed class GenericLibraryTest : TestCaseWithMacros
	{
		[TestDate(2015, 2, 19, 13, 11, 12)]
		[TestUtcOffset(11, 0, 0)]
		public void TestNowMacro()
		{
			const string macro = "Now";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = new DateTime(2015, 2, 20, 0, 11, 12)
			};

			AssertMacroRun(macro, macroRun);
		}

		[TestDate(2015, 2, 19, 13, 11, 12)]
		[TestUtcOffset(11, 0, 0)]
		public void TestTodayMacro()
		{
			const string macro = "Today";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = new DateTime(2015, 2, 20)
			};

			AssertMacroRun(macro, macroRun);
		}

		[TestDate(2015, 2, 19, 13, 11, 12)]
		[TestUtcOffset(11, 0, 0)]
		public void TestUtcNowMacro()
		{
			const string macro = "UtcNow";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = new DateTime(2015, 2, 19, 13, 11, 12)
			};

			AssertMacroRun(macro, macroRun);
		}

		[TestDate(2015, 2, 19, 13, 11, 12)]
		[TestUtcOffset(11, 0, 0)]
		public void TestNowDateTimeOffsetMacro()
		{
			const string macro = "NowDateTimeOffset";

			var macroRun = new MacroRun
			{
				Data = new object(),
				ExpectedResult = new ZDateTimeOffset(2015, 2, 20, 0, 11, 12, new TimeSpan(11, 0, 0))
			};

			AssertMacroRun(macro, macroRun);
		}

		[TestUtcOffset(11, 0, 0)]
		public void TestToLocal()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Date = new ZDateTime(2015, 2, 19, 13, 11, 12);

			const string macro = "Z0_Date.ToLocal()";

			var macroRun = new MacroRun
			{
				Data = dummy,
				ExpectedResult = new ZDateTime(2015, 2, 20, 0, 11, 12)
			};

			AssertMacroRun(macro, macroRun);
		}

		[TestUtcOffset(11, 0, 0)]
		public void TestToUtc()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Date = new ZDateTime(2015, 2, 19, 13, 11, 12);

			const string macro = "Z0_Date.ToUtc()";

			var macroRun = new MacroRun
			{
				Data = dummy,
				ExpectedResult = new ZDateTime(2015, 2, 19, 2, 11, 12)
			};

			AssertMacroRun(macro, macroRun);
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new CargoWiseOneStandardLibrary(); }
		}
	}
}
