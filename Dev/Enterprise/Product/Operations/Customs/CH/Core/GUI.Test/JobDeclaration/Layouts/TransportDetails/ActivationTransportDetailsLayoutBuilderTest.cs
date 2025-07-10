using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ActivationTransportDetailsLayoutBuilder))]
sealed class ActivationTransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ActivationTransportDetailsLayoutBuilder, MessageSendingDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override ActivationTransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ActivationTransportDetailsLayoutBuilder();
}
