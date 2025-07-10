using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class TraderDataProviderTest : BaseTransitDataProviderTest<TraderDataProvider, NctsHeaderCommonMessageSendingObject>
{
	protected override string MovementType => NctsMovementType.Codes.Departure;

	public void TestNew() => AssertNull("NctsHeader==null", TraderDataProvider.New(null));

	public void TestProviderDeparture() => CombineAssertions(() =>
	{
		const string customsRegNo = "123";
		const string language = SwissCustomsLanguageList.Codes.French;

		NctsHeader.BH_CommunicationLanguage = ZString.Empty;
		NctsHeader.MovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;

		AssertNull("Initial CommunicationLanguage", DataProvider.CommunicationLanguage);
		AssertEquals("Initial IdentificationNumber", string.Empty, DataProvider.IdentificationNumber);

		ResetDataProvider();

		NctsHeader.BH_CommunicationLanguage = language;
		NctsHeader.MovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;
		NctsHeader.MovementHeader.Representative.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsRegNo);

		AssertEquals("Set CommunicationLanguage lower case", language.ToLowerInvariant(), DataProvider.CommunicationLanguage);
		AssertEquals("Set IdentificationNumber", customsRegNo, DataProvider.IdentificationNumber);
	});

	public void TestProviderArrival() => CombineAssertions(() =>
	{
		const string customsRegNo = "123";
		const string language = SwissCustomsLanguageList.Codes.French;

		NctsHeader.BH_CommunicationLanguage = ZString.Empty;
		NctsHeader.BH_HeaderType = ZString.Empty;
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		NctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;

		AssertNull("Initial CommunicationLanguage", DataProvider.CommunicationLanguage);
		AssertEquals("Initial IdentificationNumber", string.Empty, DataProvider.IdentificationNumber);

		ResetDataProvider();

		NctsHeader.BH_CommunicationLanguage = language;

		NctsHeader.DestinationTrader.OrganisationPK = Factory.New<OrgHeader>().PK;
		NctsHeader.DestinationTrader.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsRegNo);

		AssertEquals("Set CommunicationLanguage", language.ToLowerInvariant(), DataProvider.CommunicationLanguage);
		AssertEquals("Set IdentificationNumber", customsRegNo, DataProvider.IdentificationNumber);
	});

	public void TestContactPerson() => CombineAssertions(() =>
	{
		AssertType<StaffContactPersonDataProvider>("type", DataProvider.ContactPerson);
		AssertEquals("ContactPerson.Name", GlbStaff.CurrentUser.GS_FullName, DataProvider.ContactPerson.Name);
		AssertSame("cached", DataProvider.ContactPerson, DataProvider.ContactPerson);
	});

	protected override TraderDataProvider CreateDataProvider() => TraderDataProvider.New(NctsHeader);
}
