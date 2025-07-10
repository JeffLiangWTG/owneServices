using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(AsycudaBillDataContextManager))]
	sealed class AsycudaBillDataContextManagerShipmentTest : ShipmentDataContextManagerTestCase<AsycudaBillDataContextManager, AsycudaBill>
	{
		public void TestGetEventContextValues()
		{
			var bill = GetNewBusinessObjectForTesting();
			var manager = bill.GetUniversalDataContextManager() as IEventDataContextManager;

			AssertNull(manager.EventContextValues);
		}

		protected override AsycudaBill GetNewBusinessObjectForTesting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => AsycudaManifestUniversalMessagingHelperTest.SampleUxml;
	}
}
