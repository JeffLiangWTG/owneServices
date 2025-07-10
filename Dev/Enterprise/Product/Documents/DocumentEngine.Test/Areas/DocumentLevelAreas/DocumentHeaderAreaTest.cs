using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class DocumentHeaderAreaTest : AreaAbstractTest
	{
		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentHeader");
			Assert(createdArea is DocumentHeaderArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentHeader");
			var clone = createdArea.Clone(10);
			Assert(clone is DocumentHeaderArea);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#DocumentHeader");
	}
}
