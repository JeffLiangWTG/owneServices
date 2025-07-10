using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillDetailTabLayout))]
	sealed class UCC6TemporaryStorageBillDetailTabLayoutTest : TemporaryStorageBillDetailTabLayoutProviderAbstractTest<UCC6TemporaryStorageBillDetailTabLayout>
	{
		protected override bool ExpectedIsSupportingDocumentsTabVisible => true;
		protected override bool ExpectedIsAdditionalInformationTabVisible => true;
	}
}
