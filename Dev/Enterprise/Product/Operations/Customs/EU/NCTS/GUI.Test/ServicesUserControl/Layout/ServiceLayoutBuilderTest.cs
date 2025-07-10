using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ServiceLayoutBuilder<NctsHeader>))]
	class ServiceLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ServiceLayoutBuilder<NctsHeader>, NctsHeader, ServiceControlBag>
	{
		protected override ServiceLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new ServiceLayoutBuilder<NctsHeader>();

		protected override int ExpectedMaxColumns => 2;
	}
}
