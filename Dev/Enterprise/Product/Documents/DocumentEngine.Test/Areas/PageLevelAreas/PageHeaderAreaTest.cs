namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class PageHeaderAreaTest : AreaAbstractTest
	{
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader");
			Assert(createdArea is PageHeaderArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader");
			Assert(createdArea.Clone(10) is PageHeaderArea);
		}

		public void TestStartFromSecondPage()
		{
			var createdArea = (PageHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader:StartFromSecondPage");
			AssertEquals(true, createdArea.StartFromSecondPage);
			createdArea = (PageHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader");
			AssertEquals(false, createdArea.StartFromSecondPage);
		}

		public void TestStartFromSecondPageShouldBeFalseInClonedArea()
		{
			var createdArea = (PageHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader:StartFromSecondPage");
			AssertEquals(true, createdArea.StartFromSecondPage);
			var clonedArea = (PageHeaderArea)createdArea.Clone(10);
			AssertEquals(false, clonedArea.StartFromSecondPage);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#PageHeader");
	}
}
