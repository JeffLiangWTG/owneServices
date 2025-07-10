using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(AdditionalInformationFieldsLayoutBuilder<AdditionalInformation>))]
sealed class AdditionalInformationFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalInformationFieldsLayoutBuilder<AdditionalInformation>, AdditionalInformation, AdditionalInformationFieldsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override AdditionalInformationFieldsLayoutBuilder<AdditionalInformation> GetColumnLayoutBuilderForTesting() => new AdditionalInformationFieldsLayoutBuilder<AdditionalInformation>();
}
