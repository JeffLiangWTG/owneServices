using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class FirstPageFooterAreaTest : AreaAbstractTest
	{
		public void TestIdentifier()
		{
			var footer = AreaFactory.InstantiateArea(1, 10, TestReport, "#FIRSTPAGEFOOTER");
			AssertEquals("#FirstPageFooter", footer.Identifier);
		}

		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#FirstPageFooter");
			Assert(createdArea is FirstPageFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#FirstPageFooter");
			Assert(createdArea.Clone(10) is FirstPageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#FirstPageFooter");
	}
}
