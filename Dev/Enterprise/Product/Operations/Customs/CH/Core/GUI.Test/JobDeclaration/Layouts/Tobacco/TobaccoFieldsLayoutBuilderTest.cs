using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Test;

[TestedType(typeof(TobaccoFieldsLayoutBuilder<Tobacco>))]
sealed class TobaccoFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TobaccoFieldsLayoutBuilder<Tobacco>, Tobacco, TobaccoFieldsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override TobaccoFieldsLayoutBuilder<Tobacco> GetColumnLayoutBuilderForTesting() => new TobaccoFieldsLayoutBuilder<Tobacco>();
}
