using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InAndOutwardProcessingFieldsLayoutBuilder))]
sealed class InAndOutwardProcessingFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InAndOutwardProcessingFieldsLayoutBuilder, JobComInvoiceLine, InAndOutwardProcessingFieldsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override InAndOutwardProcessingFieldsLayoutBuilder GetColumnLayoutBuilderForTesting() => new InAndOutwardProcessingFieldsLayoutBuilder();
}
