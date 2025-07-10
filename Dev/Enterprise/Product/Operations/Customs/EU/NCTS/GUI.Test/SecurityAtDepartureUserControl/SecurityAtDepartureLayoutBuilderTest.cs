using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SecurityAtDepartureLayoutBuilder<NctsHeader>))]
	class SecurityAtDepartureLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SecurityAtDepartureLayoutBuilder<NctsHeader>, NctsHeader, SecurityAtDepartureControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override SecurityAtDepartureLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new SecurityAtDepartureLayoutBuilder<NctsHeader>();
	}
}
