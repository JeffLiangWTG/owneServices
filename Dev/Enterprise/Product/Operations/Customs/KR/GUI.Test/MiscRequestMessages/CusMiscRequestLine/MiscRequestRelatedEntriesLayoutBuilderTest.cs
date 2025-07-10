using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscRequestRelatedEntriesLayoutBuilder))]
	sealed class MiscRequestRelatedEntriesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscRequestRelatedEntriesLayoutBuilder, CusMiscRequestLine, MiscRequestRelatedEntriesControlBag>
	{
		protected override int ExpectedMaxColumns => 1;
		protected override MiscRequestRelatedEntriesLayoutBuilder GetColumnLayoutBuilderForTesting() => new MiscRequestRelatedEntriesLayoutBuilder();
	}
}
