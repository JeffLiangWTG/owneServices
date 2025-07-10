using NUnit.Framework;

namespace CargoWise.Bi.Registration.PowerBi.Testing
{
	class BiReportCategoriesTest : TestCase
	{
		public void TestGetBiReportCategory()
		{
			AssertEquals(BiReportCategory.All, BiReportCategories.GetBiReportCategory("ALL"));
			AssertEquals(BiReportCategory.All, BiReportCategories.GetBiReportCategory("DeFaUlT"));
		}
	}
}
