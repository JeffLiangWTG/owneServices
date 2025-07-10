using Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStorageBillDetailTabLayout))]
sealed class UCC6TemporaryStorageBillDetailTabLayoutTest : TemporaryStorageBillDetailTabLayoutProviderAbstractTest<UCC6TemporaryStorageBillDetailTabLayout>
{
	protected override bool ExpectedIsSupportingDocumentsTabVisible => false;
	protected override bool ExpectedIsAdditionalInformationTabVisible => false;
}
