using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class BackPageAreaTest : AreaAbstractTest
	{
		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#BackPage");
			Assert(createdArea is BackPageArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#BackPage");
			createdArea.Clone(10);
			Assert(createdArea is BackPageArea);
		}

		public void TestFirstPageOnly()
		{
			var createdArea = (BackPageArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#BackPage:FirstPageOnly");
			AssertEquals(true, createdArea.FirstPageOnly);
			createdArea = (BackPageArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#BackPage");
			AssertEquals(false, createdArea.FirstPageOnly);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#BackPage");
	}
}
