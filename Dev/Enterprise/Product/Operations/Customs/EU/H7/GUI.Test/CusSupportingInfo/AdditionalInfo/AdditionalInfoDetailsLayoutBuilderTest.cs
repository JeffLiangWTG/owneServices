using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(AdditionalInfoDetailsLayoutBuilder<AdditionalInfo>))]
	class AdditionalInfoDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalInfoDetailsLayoutBuilder<AdditionalInfo>, AdditionalInfo, AdditionalInfoDetailsControlBag>
	{
		protected override AdditionalInfoDetailsLayoutBuilder<AdditionalInfo> GetColumnLayoutBuilderForTesting() => new AdditionalInfoDetailsLayoutBuilder<AdditionalInfo>();
	}
}
