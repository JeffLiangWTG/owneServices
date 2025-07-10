using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPTemporaryLandingLayoutBuilder<AsycudaBill>))]
	sealed class JPTemporaryLandingLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<JPTemporaryLandingLayoutBuilder<AsycudaBill>, AsycudaBill, JPTemporaryLandingControlBag>
	{
		protected override JPTemporaryLandingLayoutBuilder<AsycudaBill> GetColumnLayoutBuilderForTesting() => new JPTemporaryLandingLayoutBuilder<AsycudaBill>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
