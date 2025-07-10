using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class GoodsDeclarationDataProviderTest : BaseArrivalDataProviderTest<GoodsDeclarationDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override GoodsDeclarationDataProvider CreateDataProvider() => GoodsDeclarationDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers).First();

	public void TestNewCollection()
	{
		AssertNull("null argument", GoodsDeclarationDataProvider.NewCollection(null));

		ResetDataProvider();
		NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
		AssertEquals("Count", 2, GoodsDeclarationDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.MovementReferenceNumbers).Count());
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		MovementReferenceNumber.CSI_ReferenceNumber = "MRN123";
		MovementReferenceNumber.CSI_Description = "add.info";

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("MRN", "MRN123", DataProvider.MRN);
		AssertEquals("AdditionalInformation", "add.info", DataProvider.AdditionalInformation);
	});

	public void TestStateOfseal() => CombineAssertions(() =>
	{
		MovementReferenceNumber.CSI_Status = YesNoList.Codes.Yes;
		AssertEquals($"CSI_Status={MovementReferenceNumber.CSI_Status}", true, DataProvider.SealIsValid);
		MovementReferenceNumber.CSI_Status = YesNoList.Codes.No;
		AssertEquals($"CSI_Status={MovementReferenceNumber.CSI_Status}", false, DataProvider.SealIsValid);
		MovementReferenceNumber.CSI_Status = ZString.Empty;
		AssertNull($"CSI_Status={MovementReferenceNumber.CSI_Status}", DataProvider.SealIsValid);
	});

	public void TestOppositeInformation() => CombineAssertions(() =>
	{
		_ = MovementReferenceNumber;
		AssertNotNull(DataProvider.OppositeInformation);
		AssertSame("cached", DataProvider.OppositeInformation, DataProvider.OppositeInformation);
	});
}
