using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BasePlaceOfLoadingOrUnloadingDataProviderTest<TDataProvider> : BaseDepartureDataProviderTest<TDataProvider, NctsHeaderDepartureMessageSendingObject>
	where TDataProvider : BasePlaceOfLoadingOrUnloadingDataProvider
{
	protected override TDataProvider CreateDataProvider() => CreateDataProvider(NctsHeader.MovementHeader);

	protected abstract TDataProvider CreateDataProvider(NctsDepartureMovementHeader movementHeader);

	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull("MovementHeader==null", CreateDataProvider(null));

		SetPortCode(ZString.Empty);
		SetPortLocation(ZString.Empty);
		AssertNull("Neither code nor location", CreateDataProvider());
	});

	public void TestProvider() => CombineAssertions(() =>
	{
		SetPortLocation("Hamburg");
		SetPortCode("DEHAM");

		AssertEquals("Location", "Hamburg", DataProvider.Location);
		AssertEquals("UNLocode", "DEHAM", DataProvider.UNLocode);
		AssertEquals("Country", "DE", DataProvider.Country);
	});

	public void TestLocation() => CombineAssertions(() =>
	{
		SetPortCode("DEHAM");
		SetPortLocation(ZString.Empty);
		AssertNull("empty", DataProvider.Location);
	});

	public void TestUNLOCODE() => CombineAssertions(() =>
	{
		SetPortCode(ZString.Empty);
		SetPortLocation("Hamburg");
		AssertNull("empty", DataProvider.UNLocode);
		ResetDataProvider();

		SetPortCode(Core.Constants.CountryCodes.Germany);
		SetPortLocation("Hamburg");
		AssertNull("country code", DataProvider.UNLocode);
		ResetDataProvider();
	});

	public void TestCountry() => CombineAssertions(() =>
	{
		SetPortCode(ZString.Empty);
		SetPortLocation("Hamburg");
		AssertNull("empty", DataProvider.Country);
		ResetDataProvider();

		SetPortCode(Core.Constants.CountryCodes.Germany);
		SetPortLocation("Hamburg");
		AssertEquals("Country", "DE", DataProvider.Country);
	});

	protected abstract void SetPortCode(ZString portCode);

	protected abstract void SetPortLocation(ZString portLocation);
}
