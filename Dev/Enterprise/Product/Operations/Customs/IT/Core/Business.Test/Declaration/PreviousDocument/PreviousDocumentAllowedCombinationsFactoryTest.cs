using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentAllowedCombinationsFactoryTest : TestCaseWithFactory
{
	public void TestGetAllowedCombinations()
	{
		CombineAssertions(() =>
		{
			var combinationsLoader = new PreviousDocumentAllowedCombinationsFactory();
			var combinations = combinationsLoader.GetAllowedCombinations(Factory);
			AssertEquals("Number of combinations", 58, combinations.Count);
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
			AssertPreviousDocumentCombinationItem(combinationIndex: 17, combinations.ElementAt(17), "A6", "EX", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 18, combinations.ElementAt(18), "NN", "ZZZ", "X", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 19, combinations.ElementAt(19), "NN", "ZZZ", "Z", PreviousDocumentCombinationTemplate._1);
			AssertPreviousDocumentCombinationItem(combinationIndex: 20, combinations.ElementAt(20), "A3", "ZZZ", "Z", PreviousDocumentCombinationTemplate._4);
			AssertPreviousDocumentCombinationItem(combinationIndex: 21, combinations.ElementAt(21), "PF", "ZZZ", "Z", PreviousDocumentCombinationTemplate._5);
			AssertPreviousDocumentCombinationItem(combinationIndex: 22, combinations.ElementAt(22), "2", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 23, combinations.ElementAt(23), "2", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 24, combinations.ElementAt(24), "2", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 25, combinations.ElementAt(25), "2", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 26, combinations.ElementAt(26), "2T", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 27, combinations.ElementAt(27), "2T", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 28, combinations.ElementAt(28), "2T", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 29, combinations.ElementAt(29), "2T", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 30, combinations.ElementAt(30), "2S", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 31, combinations.ElementAt(31), "2S", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 32, combinations.ElementAt(32), "2S", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 33, combinations.ElementAt(33), "2S", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 34, combinations.ElementAt(34), "5", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 35, combinations.ElementAt(35), "5", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 36, combinations.ElementAt(36), "5", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 37, combinations.ElementAt(37), "5", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 38, combinations.ElementAt(38), "5T", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 39, combinations.ElementAt(39), "5T", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 40, combinations.ElementAt(40), "5T", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 41, combinations.ElementAt(41), "5T", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 42, combinations.ElementAt(42), "5S", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 43, combinations.ElementAt(43), "5S", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 44, combinations.ElementAt(44), "5S", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 45, combinations.ElementAt(45), "5S", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 46, combinations.ElementAt(46), "7", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 47, combinations.ElementAt(47), "7", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 48, combinations.ElementAt(48), "7", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 49, combinations.ElementAt(49), "7", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 50, combinations.ElementAt(50), "7T", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 51, combinations.ElementAt(51), "7T", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 52, combinations.ElementAt(52), "7T", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 53, combinations.ElementAt(53), "7T", "EX", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 54, combinations.ElementAt(54), "7S", "IM", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 55, combinations.ElementAt(55), "7S", "EU", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 56, combinations.ElementAt(56), "7S", "CO", "Z", PreviousDocumentCombinationTemplate._3);
			AssertPreviousDocumentCombinationItem(combinationIndex: 57, combinations.ElementAt(57), "7S", "EX", "Z", PreviousDocumentCombinationTemplate._3);
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
