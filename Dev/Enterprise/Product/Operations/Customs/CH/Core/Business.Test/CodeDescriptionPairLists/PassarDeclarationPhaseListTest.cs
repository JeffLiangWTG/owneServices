using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarDeclarationPhaseList))]
sealed class PassarDeclarationPhaseListTest : TestCase
{
	public void TestGetAppropriateEntryHeaderPhaseStatusCode() => CombineAssertions(() =>
	{
		AssertEquals("null", ZString.Empty, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(null));
		AssertEquals("Empty", ZString.Empty, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(ZString.Empty));
		AssertEquals("NE013", PassarDeclarationPhaseList.Codes.Amendment, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NE013));
		AssertEquals("NE014", PassarDeclarationPhaseList.Codes.Cancellation, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NE014));
		AssertEquals("NE015", PassarDeclarationPhaseList.Codes.Declaration, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NE015));
		AssertEquals("NE013", PassarDeclarationPhaseList.Codes.EDecToPassarDataTransfer, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NE130));
		AssertEquals("NC016", PassarDeclarationPhaseList.Codes.RequestDataJourney, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NC016));
		AssertEquals("NI013", PassarDeclarationPhaseList.Codes.Amendment, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NI013));
		AssertEquals("NI014", PassarDeclarationPhaseList.Codes.Cancellation, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NI014));
		AssertEquals("NI015	", PassarDeclarationPhaseList.Codes.Declaration, PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(PassarMessageTypeList.Codes.NI015));
	});
}
