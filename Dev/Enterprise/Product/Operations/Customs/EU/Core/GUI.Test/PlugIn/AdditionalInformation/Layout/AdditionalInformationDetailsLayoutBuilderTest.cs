using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(AdditionalInformationDetailsLayoutBuilder))]
	class AdditionalInformationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalInformationDetailsLayoutBuilder, AdditionalInfo, AdditionalInformationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override AdditionalInformationDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new AdditionalInformationDetailsLayoutBuilder();
	}
}
