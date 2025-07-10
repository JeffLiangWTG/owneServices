using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(AdditionalInfoLayoutBuilder<JobDeclaration>))]
	class AdditionalInfoLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalInfoLayoutBuilder<JobDeclaration>, JobDeclaration, AdditionalInfoControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override AdditionalInfoLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new AdditionalInfoLayoutBuilder<JobDeclaration>();
	}
}
