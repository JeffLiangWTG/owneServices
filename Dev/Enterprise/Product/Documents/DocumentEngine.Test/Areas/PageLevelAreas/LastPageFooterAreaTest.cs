using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class LastPageFooterAreaTest : AreaAbstractTest
	{
		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#LastPageFooter");
			Assert(createdArea is LastPageFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#LastPageFooter");
			createdArea.Clone(10);
			Assert(createdArea is LastPageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#LastPageFooter");
	}
}
