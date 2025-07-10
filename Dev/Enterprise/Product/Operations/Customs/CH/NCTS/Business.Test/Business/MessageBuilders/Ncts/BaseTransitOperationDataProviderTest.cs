using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class BaseTransitOperationDataProviderTest : BaseArrivalDataProviderTest<BaseTransitOperationDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	public void TestNew()
	{
		AssertNull(BaseTransitOperationDataProvider.New(null));
	}

	public void TestDataProviderIncludeMRNTrue()
	{
		NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234.5";
		NctsHeader.ArrivalMovementHeader.OtherThingsToReport = "other things";

		CombineAssertions(() =>
		{
			AssertEquals("MRN", "MRN1234", DataProvider.MRN);
			AssertEquals("MRNVersion", 5, DataProvider.MRNVersion);
			AssertEquals("OtherThingsToReport", "other things", DataProvider.OtherThingsToReport);
		});
	}

	public void TestDataProviderIncludeMRNFalse()
	{
		NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234.5";
		var provider = BaseTransitOperationDataProviderForTest.New(NctsHeader);

		CombineAssertions(() =>
		{
			AssertNull("MRN", provider.MRN);
			AssertEquals("MRNVersion", null, provider.MRNVersion);
		});
	}

	public void TestOtherTingsToReport()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		NctsHeader.ArrivalMovementHeader.OtherThingsToReport = ZString.Empty;
		AssertNull("null if empty", DataProvider.OtherThingsToReport);
	}

	protected override BaseTransitOperationDataProvider CreateDataProvider() => BaseTransitOperationDataProvider.New(NctsHeader);
}

public class BaseTransitOperationDataProviderForTest : BaseTransitOperationDataProvider
{
	public static new BaseTransitOperationDataProviderForTest New(NctsHeader nctsHeader) => nctsHeader == null ? null : new BaseTransitOperationDataProviderForTest(nctsHeader);

	protected BaseTransitOperationDataProviderForTest(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override bool IncludeMRN => false;
}
