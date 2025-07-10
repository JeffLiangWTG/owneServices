using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CodeSyncTest : TestCase
	{
		public void TestLedgerTypesList()
		{
			MatchConstantsHolders(typeof(LedgerTypeCodes), typeof(LedgerTypesList.Codes));
		}

		public void TestDayOfWeekCodes()
		{
			MatchConstantsHolders(typeof(CalendarCodes.Days), typeof(AutoDayOfWeekCodeList.Codes));
		}

		static void MatchConstantsHolders(Type expected, Type actual)
		{
			AssertContainsExactElementsInAnyOrder(ExtractPairs(expected), ExtractPairs(actual));

			IEnumerable<(string, string)> ExtractPairs(Type holderType)
			{
				return
					from field in holderType.GetFields(BindingFlags.Public | BindingFlags.Static)
					where field.IsLiteral && field.FieldType == typeof(string)
					let value = (string)field.GetValue(null)
					orderby field.Name
					select (field.Name, value);
			}
		}
	}
}
