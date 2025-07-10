using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class StatisticStatusCodeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var statisticsCodeList = new StatisticStatusCodeList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(statisticsCodeList.CodesAsString, Is.EqualTo("01, 04"), "All codes");
				NUnit.Framework.Assert.That(StatisticStatusCodeList.Descriptions.C01, Is.EqualTo("Goods exempt from declaration for foreign trade statistics according to Annex V, Chapter I, Section 3 No. 5 CCIP (EU) 2020/1197 in conjunction with the Annex and §6(2) and §32(5) of AHStatDV in conjunction with Annex 4").Using(CustomComparers.TypeComparison), "Description for Code '01'");
				NUnit.Framework.Assert.That(StatisticStatusCodeList.Descriptions.C04, Is.EqualTo("Goods are to be registered statistically unless they are exempt from registration for foreign trade statistics (code 01)").Using(CustomComparers.TypeComparison), "Description for Code '04'");
			});
		}
	}
}
