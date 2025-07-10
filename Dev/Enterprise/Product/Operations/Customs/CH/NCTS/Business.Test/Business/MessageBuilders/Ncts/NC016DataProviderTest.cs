using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NC016DataProvider))]
sealed class NC016DataProviderTest : BaseNctsMessageDataProviderTest<NC016DataProvider, NctsHeaderCommonMessageSendingObject>
{
	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override NC016DataProvider CreateDataProvider() => new NC016DataProvider(MessageSendingObject);

	public void TestExportOperationGDRN() => AssertEquals("Not mapped yet", string.Empty, DataProvider.ExportOperationGDRN);

	public void TestTransitOperationMRN()
	{
		NctsHeader.MovementReferenceNumberSetter("23BE14785692745688.2");
		AssertEquals("MRN", "23BE14785692745688", DataProvider.TransitOperationMRN);
	}

	public void TransportOperationJRN() => AssertEquals("Not mapped yet", string.Empty, DataProvider.TransportOperationJRN);
}
