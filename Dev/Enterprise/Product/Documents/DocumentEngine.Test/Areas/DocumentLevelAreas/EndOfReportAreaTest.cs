using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class EndOfReportAreaTest : AreaAbstractTest
	{
		public override void TestVisualisationManagerHasAppropriateVisualisationRenderer()
		{
			AssertNull(AreaVisualisationManagerFactory.New(GetNewAreaToTest()));
		}

		[ExpectNoExceptions]
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#EndOfReport");
			Assert(createdArea is EndOfReportArea);
		}

		[ExpectException(typeof(CloneAreaException))]
		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#EndOfReport");
			_ = createdArea.Clone(10);
		}

		public void TestEndingRow()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#EndOfReport");
			AssertEquals(2, createdArea.End);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#EndOfReport");
	}
}
