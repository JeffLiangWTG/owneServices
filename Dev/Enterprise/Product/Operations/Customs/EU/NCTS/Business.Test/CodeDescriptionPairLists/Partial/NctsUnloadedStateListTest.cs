using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUnloadedStateList))]
	sealed class NctsUnloadedStateListTest : TestCase
	{
		public void TestCreateConfigurableList()
		{
			var testCases = new[]
			{
				new { ExclusionKeys = "DAM", ExpectedDAM = false, ExpectedDIF = true },
				new { ExclusionKeys = "DIF", ExpectedDAM = true, ExpectedDIF = false },
				new { ExclusionKeys = "DAM,DIF", ExpectedDAM = false, ExpectedDIF = false },
				new { ExclusionKeys = "", ExpectedDAM = true, ExpectedDIF = true }
			};

			foreach (var testCase in testCases)
			{
				var exclusions = string.IsNullOrEmpty(testCase.ExclusionKeys) ? new List<string>() : new List<string>(testCase.ExclusionKeys.Split(','));
				var list = NctsUnloadedStateList.CreateConfigurableList(exclusions);

				AssertEquals($"Exclusions: {testCase.ExclusionKeys} — Expected DAM: {testCase.ExpectedDAM}", testCase.ExpectedDAM, list.ContainsCode(NctsUnloadedStateList.Codes.DAM));
				AssertEquals($"Exclusions: {testCase.ExclusionKeys} — Expected DIF: {testCase.ExpectedDIF}", testCase.ExpectedDIF, list.ContainsCode(NctsUnloadedStateList.Codes.DIF));
			}
		}
	}
}
