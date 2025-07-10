using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(ExtendedHoursRequestNewLayoutsBuilder))]
	sealed class ExtendedHoursRequestNewLayoutsBuilderTest : ColumnLayoutBuilderAbstractTest<ExtendedHoursRequestNewLayoutsBuilder, ExtendedHoursRequestHeader, ExtendedHoursRequestNewControlBag>
	{
		protected override int ExpectedMaxColumns => 2;
		protected override ExtendedHoursRequestNewLayoutsBuilder GetColumnLayoutBuilderForTesting() => new ExtendedHoursRequestNewLayoutsBuilder();
	}
}
