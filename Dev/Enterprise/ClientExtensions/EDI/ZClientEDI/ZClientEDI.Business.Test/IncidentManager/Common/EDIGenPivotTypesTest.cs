using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class EDIGenPivotTypesTest : TestCase
	{
		public void TestTypesNotDuplicated()
		{
			string[] genericCodes = typeof(Core.Constants.GenPivotTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(null).ToString()).ToArray();
			string[] ediCodes = typeof(EDIGenPivotTypes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(null).ToString()).ToArray();
			List<string> codes = new List<string>();
			foreach (string code in ediCodes)
			{
				AssertCollectionNotContains("Type code " + code + " is duplicated", code, codes);
				AssertCollectionNotContains("Type code " + code + " also exists as a generic type", code, genericCodes);
				codes.Add(code);
			}
		}
	}
}