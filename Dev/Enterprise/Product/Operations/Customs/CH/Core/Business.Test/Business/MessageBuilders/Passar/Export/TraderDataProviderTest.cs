using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TraderDataProvider))]
sealed class TraderDataProviderTest : BasePassarDataProviderTest<TraderDataProvider>
{
	public void TestNew()
	{
		AssertNull(TraderDataProvider.New(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		var declarantOrg = Factory.New<OrgHeader>();
		declarantOrg.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID123", Core.Constants.CountryCodes.Switzerland);
		Declaration.DeclarantAddress.OA_OH = declarantOrg.PK;

		AssertEquals("IdentificationNumber", "BID123", DataProvider.IdentificationNumber);
	});

	public void TestContact() => CombineAssertions(() =>
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "S01";
		staff.GS_FullName = "The Agent's Name";
		Declaration.JE_GS_NKCusAgent = staff.GS_Code;

		AssertEquals("The Agent's Name", DataProvider.ContactPerson.Name);
		AssertSame("cached", DataProvider.ContactPerson, DataProvider.ContactPerson);
	});

	public void TestCommunicationLanguage() => CombineAssertions(() =>
	{
		var language = SwissCustomsLanguageList.Codes.French;
		Declaration.JE_DeclarationLanguage = ZString.Empty;
		AssertNull("Empty", DataProvider.CommunicationLanguage);
		Declaration.JE_DeclarationLanguage = language;
		AssertNotEquals("Not empty", ZString.Empty, DataProvider.CommunicationLanguage);
		AssertEquals("lowercase", language.ToLowerInvariant(), DataProvider.CommunicationLanguage);
	});

	protected override TraderDataProvider CreateDataProvider() => TraderDataProvider.New(Declaration);
}
