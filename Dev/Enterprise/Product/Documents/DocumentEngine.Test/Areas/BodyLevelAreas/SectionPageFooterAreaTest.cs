namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionPageFooterAreaTest : AreaAbstractTest
	{
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageFooter");
			Assert(createdArea is SectionPageFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageFooter");
			Assert(createdArea.Clone(10) is SectionPageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageFooter");
	}
}
