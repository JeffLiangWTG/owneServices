using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportPreviousDocumentAllowedCombinationsFactoryTest : TestCaseWithFactory
{
	public void TestGetAllowedCombinations()
	{
		CombineAssertions(() =>
		{
			var combinationsLoader = new ImportPreviousDocumentAllowedCombinationsFactory();
			var combinations = combinationsLoader.GetAllowedCombinations(Factory);
			AssertEquals("Number of combinations", 37, combinations.Count);
			AssertPreviousDocumentCombinationItem(combinationIndex: 0, combinations.ElementAt(0), "TIEX", "720", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 1, combinations.ElementAt(1), "CIM", "720", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 2, combinations.ElementAt(2), "LC", "270", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 3, combinations.ElementAt(3), "AWB", "740", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 4, combinations.ElementAt(4), "LTA", "740", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 5, combinations.ElementAt(5), "BCTR", "750", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 6, combinations.ElementAt(6), "A44", "785", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 7, combinations.ElementAt(7), "A45", "785", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 8, combinations.ElementAt(8), "MTA", "785", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 9, combinations.ElementAt(9), "MRN", "820", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 10, combinations.ElementAt(10), "MRN", "821", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 11, combinations.ElementAt(11), "MRN", "822", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 12, combinations.ElementAt(12), "T1", "821", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 13, combinations.ElementAt(13), "T1", "821", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 14, combinations.ElementAt(14), "T2", "822", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 15, combinations.ElementAt(15), "T2F", "T2F", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 16, combinations.ElementAt(16), "TIR", "952", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 17, combinations.ElementAt(17), "A6", "955", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 18, combinations.ElementAt(18), "NN", "NNN", "Z", PreviousDocumentCombinationTemplate.ImportNN);
			AssertPreviousDocumentCombinationItem(combinationIndex: 19, combinations.ElementAt(19), "A3", "337", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 20, combinations.ElementAt(20), "PF", "337", "Z", PreviousDocumentCombinationTemplate._5);
			AssertPreviousDocumentCombinationItem(combinationIndex: 21, combinations.ElementAt(21), "2", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 22, combinations.ElementAt(22), "2T", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 23, combinations.ElementAt(23), "2S", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 24, combinations.ElementAt(24), "5", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 25, combinations.ElementAt(25), "5T", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 26, combinations.ElementAt(26), "5S", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 27, combinations.ElementAt(27), "7", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 28, combinations.ElementAt(28), "7T", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 29, combinations.ElementAt(29), "7S", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 30, combinations.ElementAt(30), "NUM", "ZZZ", "Z", PreviousDocumentCombinationTemplate.ImportNUM);
			AssertPreviousDocumentCombinationItem(combinationIndex: 31, combinations.ElementAt(31), "MR1", "MRN", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 32, combinations.ElementAt(32), "TC", "337", "Z", PreviousDocumentCombinationTemplate._2);
			AssertPreviousDocumentCombinationItem(combinationIndex: 33, combinations.ElementAt(33), "DM", "CLE", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 34, combinations.ElementAt(34), "1", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 35, combinations.ElementAt(35), "1T", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 36, combinations.ElementAt(36), "1S", "ZZZ", "Z", PreviousDocumentCombinationTemplate._3);
		});
	}

	void AssertPreviousDocumentCombinationItem(int combinationIndex, PreviousDocumentCombinationItem combinationItem, string expectedProcedure, string expectedCode, string expectedSubType, PreviousDocumentCombinationTemplate expectedTemplate)
	{
		AssertEquals($"Combination at [{combinationIndex}], Procedure", expectedProcedure, combinationItem.Procedure);
		AssertEquals($"Combination at [{combinationIndex}], Code", expectedCode, combinationItem.Code.Code);
		AssertEquals($"Combination at [{combinationIndex}], SubType", expectedSubType, combinationItem.SubType.Code);
		AssertEquals($"Combination at [{combinationIndex}], Template", expectedTemplate, combinationItem.Template);
	}
}
