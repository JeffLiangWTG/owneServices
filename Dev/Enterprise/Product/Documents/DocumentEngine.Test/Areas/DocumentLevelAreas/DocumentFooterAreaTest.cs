using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class DocumentFooterAreaTest : AreaAbstractTest
	{
		public void TestIdentifier()
		{
			var footer = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentFooter");
			AssertEquals("#DocumentFooter", footer.Identifier);
		}

		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentFooter");
			Assert(createdArea is DocumentFooterArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentFooter");
			var clone = createdArea.Clone(10);
			Assert(clone is DocumentFooterArea);
		}

		public void TestStickToBottom()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentFooter:StickToBottom");
			AssertEquals("TestReport.ErrorManager.HasErrors", false, TestReport.ErrorManager.HasErrors);
			Assert(createdArea is LastPageFooterArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentFooter");
	}
}
