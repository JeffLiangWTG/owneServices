using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AdjustmentsDocPage))]
	abstract class AdjustmentsDocPageTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestSubHeaderWithNoLines();

		public abstract void TestGetPages();

		public abstract void TestAdjustmentsDocFirstPage();

		public abstract void TestSortLinesNumerically();

		public abstract void TestSplitLineShouldBeAtTheBackOfEachGroup();
	}
}
