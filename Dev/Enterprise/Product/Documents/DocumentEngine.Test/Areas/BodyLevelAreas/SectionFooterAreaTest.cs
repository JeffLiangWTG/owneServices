using Enterprise.DocumentEngine.Exceptions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionFooterAreaTest : AreaAbstractTest
	{
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionFooter");
			Assert(createdArea is SectionFooterArea);
		}

		[ExpectException(typeof(CloneAreaException))]
		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionFooter");
			var clone = createdArea.Clone(10);
			AssertNotNull(clone);
		}

		public void TestShowEvenWithNoData()
		{
			var createdArea = (SectionFooterArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionFooter:" + Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
			AssertEquals(true, createdArea.ShowEvenWithNoData);
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionFooter");
	}
}
