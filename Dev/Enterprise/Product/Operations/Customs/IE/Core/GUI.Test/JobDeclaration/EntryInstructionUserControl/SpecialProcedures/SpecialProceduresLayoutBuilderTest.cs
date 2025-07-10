using System.Reflection;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(SpecialProceduresLayoutBuilder))]
	sealed class SpecialProceduresLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SpecialProceduresLayoutBuilder, JobDeclaration, EU.GUI.SpecialProceduresControlBag>
	{
		public void TestIEBag()
		{
			var ieBag = ColumnLayoutBuilderForTesting.IEBag;
			var expectedType = typeof(SpecialProceduresControlBag);
			AssertionWithHtml.CombineAssertions(delegate
			{
				Assertion.AssertType("Correct Type", expectedType, ieBag);
				Assertion.AssertSame(expectedType.Name + ".Instance", expectedType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null), ieBag);
			});
		}

		protected override SpecialProceduresLayoutBuilder GetColumnLayoutBuilderForTesting() => new SpecialProceduresLayoutBuilder();

		protected override int ExpectedMaxColumns => 3;
	}
}
