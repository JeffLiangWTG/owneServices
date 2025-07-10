using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class OnlyOnePageFooterAreaTest : AreaAbstractTest
	{
		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#OnlyOnePageFooter");
			Assert(createdArea is OnlyOnePageFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#OnlyOnePageFooter");
			createdArea.Clone(10);
			Assert(createdArea is OnlyOnePageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#OnlyOnePageFooter");
	}
}
