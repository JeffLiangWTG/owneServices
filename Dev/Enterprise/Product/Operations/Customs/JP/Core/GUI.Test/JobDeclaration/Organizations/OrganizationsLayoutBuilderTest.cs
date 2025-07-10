using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(OrganizationsLayoutBuilder<JobDeclaration>))]
	sealed class OrganizationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganizationsLayoutBuilder<JobDeclaration>, JobDeclaration, CommonOrganisationsControlBag>
	{
		protected override OrganizationsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting()
		{
			return new OrganizationsLayoutBuilder<JobDeclaration>();
		}
	}
}
