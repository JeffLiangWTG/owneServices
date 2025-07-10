using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(OrganisationDetailsLayoutBuilder<CusIntrastatHeader>))]
	sealed class OrganisationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganisationDetailsLayoutBuilder<CusIntrastatHeader>, CusIntrastatHeader, OrganisationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override OrganisationDetailsLayoutBuilder<CusIntrastatHeader> GetColumnLayoutBuilderForTesting() => new OrganisationDetailsLayoutBuilder<CusIntrastatHeader>();
	}
}
