using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration>))]
	sealed class DeclarationOrganizationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration>, EMCSJobDeclaration, DeclarationOrganizationsControlBag>
	{
		protected override DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration> GetColumnLayoutBuilderForTesting() => new DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration>();

		protected override int ExpectedMaxColumns => 1;
	}
}
