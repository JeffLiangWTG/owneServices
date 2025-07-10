using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class AdditionalTransitOperationDataProviderTest : BaseArrivalDataProviderTest<AdditionalTransitOperationDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override AdditionalTransitOperationDataProvider CreateDataProvider() => AdditionalTransitOperationDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations).First();

	public void TestNewCollection() => CombineAssertions(() =>
	{
		AssertNull("null argument", AdditionalTransitOperationDataProvider.NewCollection(null));

		ResetDataProvider();
		NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
		NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
		AssertEquals("Count", 2, AdditionalTransitOperationDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.AdditionalTransitOperations).Count());
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		AdditionalTransitOperation.CSI_IssuerType = "08";
		AdditionalTransitOperation.CSI_ReferenceNumber = "reference";
		AdditionalTransitOperation.CSI_Description = "description";
		AdditionalTransitOperation.CSI_Quantity = 12.5;
		AdditionalTransitOperation.CSI_PackQty = 20;

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("Type", "08", DataProvider.Type);
		AssertEquals("ReferenceNumber", "reference", DataProvider.ReferenceNumber);
		AssertEquals("DescriptionOfGoods", "description", DataProvider.DescriptionOfGoods);
		AssertEquals("GrossMass", 12.5m, DataProvider.GrossMass);
		AssertEquals("NumberOfPackages", 20, DataProvider.NumberOfPackages);
	});

	public void TestStateOfSeal() => CombineAssertions(() =>
	{
		AdditionalTransitOperation.CSI_Status = YesNoList.Codes.Yes;
		AssertEquals($"CSI_Status={MovementReferenceNumber.CSI_Status}", true, DataProvider.SealIsValid);
		AdditionalTransitOperation.CSI_Status = YesNoList.Codes.No;
		AssertEquals($"CSI_Status={MovementReferenceNumber.CSI_Status}", false, DataProvider.SealIsValid);
		AdditionalTransitOperation.CSI_Status = ZString.Empty;
		AssertNull($"CSI_Status={MovementReferenceNumber.CSI_Status}", DataProvider.SealIsValid);
	});
}
