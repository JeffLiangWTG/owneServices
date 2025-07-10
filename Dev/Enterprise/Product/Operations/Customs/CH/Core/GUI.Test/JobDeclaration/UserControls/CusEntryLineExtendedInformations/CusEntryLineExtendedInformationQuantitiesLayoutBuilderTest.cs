using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine>))]
class CusEntryLineExtendedInformationQuantitiesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine>, CusEntryLine, CusEntryLineExtendedInformationQuantitiesControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine> GetColumnLayoutBuilderForTesting() => new CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine>();
}
