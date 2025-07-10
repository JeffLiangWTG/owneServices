using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class PageFooterAreaTest : AreaAbstractTest
	{
		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#PageFooter");
			Assert(createdArea is PageFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#PageFooter");
			Assert(createdArea.Clone(10) is PageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#PageFooter");
	}
}
