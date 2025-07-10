using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(IncidentDetailsLayoutBuilder<EnRouteIncident>))]
	sealed class IncidentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<IncidentDetailsLayoutBuilder<EnRouteIncident>, EnRouteIncident, IncidentDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override IncidentDetailsLayoutBuilder<EnRouteIncident> GetColumnLayoutBuilderForTesting() => new IncidentDetailsLayoutBuilder<EnRouteIncident>();
	}
}
