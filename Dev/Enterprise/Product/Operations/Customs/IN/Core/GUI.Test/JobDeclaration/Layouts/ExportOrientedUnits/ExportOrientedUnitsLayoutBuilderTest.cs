using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportOrientedUnitsLayoutBuilder))]
class ExportOrientedUnitsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportOrientedUnitsLayoutBuilder, JobDeclaration, ExportOrientedUnitsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ExportOrientedUnitsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ExportOrientedUnitsLayoutBuilder();
}
