using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarMessageTypeList))]
sealed class PassarMessageTypeListTest : TestCase
{
	public void TestGetMessageSubType() => CombineAssertions(() =>
	{
		AssertCode(MessageSubTypeCodeList.Codes.ImportAmendment, MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI013);
		AssertCode(MessageSubTypeCodeList.Codes.ImportCancellation, MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI014);
		AssertCode(MessageSubTypeCodeList.Codes.ImportDeclaration, MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI015);
		AssertCode(MessageSubTypeCodeList.Codes.ImportLastResponse, MessageTypeCodeList.Codes.Import, PassarMessageTypeList.Codes.NI016);

		AssertCode(MessageSubTypeCodeList.Codes.PassarRequestDataJourney, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NC016);
		AssertCode(MessageSubTypeCodeList.Codes.NctsActivationAtDomicile, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NC123);
		AssertCode(MessageSubTypeCodeList.Codes.PassarAmendment, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE013);
		AssertCode(MessageSubTypeCodeList.Codes.PassarWithdrawal, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE014);
		AssertCode(MessageSubTypeCodeList.Codes.PassarDeclaration, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE015);
		AssertCode(MessageSubTypeCodeList.Codes.PassarEDecToPassarDataTransfer, MessageTypeCodeList.Codes.Export, PassarMessageTypeList.Codes.NE130);

		AssertCode(MessageSubTypeCodeList.Codes.PassarRequestDataJourney, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NC016);
		AssertCode(MessageSubTypeCodeList.Codes.NctsActivationAtDomicile, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NC123);
		AssertCode(MessageSubTypeCodeList.Codes.PassarArrivalNotification, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT007);
		AssertCode(MessageSubTypeCodeList.Codes.PassarAmendment, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT013);
		AssertCode(MessageSubTypeCodeList.Codes.PassarWithdrawal, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT014);
		AssertCode(MessageSubTypeCodeList.Codes.PassarDeclaration, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT015);
		AssertCode(MessageSubTypeCodeList.Codes.PassarInventoryResult, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT044);
		AssertCode(MessageSubTypeCodeList.Codes.NctsNotArrivedTransitMovement, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT141);
		AssertCode(MessageSubTypeCodeList.Codes.NctsNationalDepartureAmendment, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT513);
		AssertCode(MessageSubTypeCodeList.Codes.NctsNationalDeparture, MessageTypeCodeList.Codes.PassarNcts, PassarMessageTypeList.Codes.NT515);

		AssertCode(ZString.Empty, MessageTypeCodeList.Codes.Export, "XXX");
		AssertCode(ZString.Empty, MessageTypeCodeList.Codes.PassarNcts, ZString.Empty);
		AssertCode(ZString.Empty, "XXX", PassarMessageTypeList.Codes.NE015);
		AssertCode(ZString.Empty, ZString.Empty, PassarMessageTypeList.Codes.NE015);

		void AssertCode(ZString expectedMessageSubType, ZString messageType, ZString passarMessageType)
		{
			AssertEquals($"{messageType} {passarMessageType}", expectedMessageSubType, PassarMessageTypeList.GetMessageSubType(messageType, passarMessageType));
		}
	});

	public void TestGetPassarMessageType() => CombineAssertions(() =>
	{
		AssertCode(PassarMessageTypeList.Codes.NI013, MessageTypeCodeList.Codes.Import, MessageSubTypeCodeList.Codes.ImportAmendment);
		AssertCode(PassarMessageTypeList.Codes.NI014, MessageTypeCodeList.Codes.Import, MessageSubTypeCodeList.Codes.ImportCancellation);
		AssertCode(PassarMessageTypeList.Codes.NI015, MessageTypeCodeList.Codes.Import, MessageSubTypeCodeList.Codes.ImportDeclaration);
		AssertCode(PassarMessageTypeList.Codes.NI016, MessageTypeCodeList.Codes.Import, MessageSubTypeCodeList.Codes.ImportLastResponse);

		AssertCode(PassarMessageTypeList.Codes.NC016, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarRequestDataJourney);
		AssertCode(PassarMessageTypeList.Codes.NC123, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.NctsActivationAtDomicile);
		AssertCode(PassarMessageTypeList.Codes.NE013, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarAmendment);
		AssertCode(PassarMessageTypeList.Codes.NE014, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarWithdrawal);
		AssertCode(PassarMessageTypeList.Codes.NE015, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarDeclaration);
		AssertCode(PassarMessageTypeList.Codes.NE130, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarEDecToPassarDataTransfer);

		AssertCode(PassarMessageTypeList.Codes.NC016, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarRequestDataJourney);
		AssertCode(PassarMessageTypeList.Codes.NC123, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.NctsActivationAtDomicile);
		AssertCode(PassarMessageTypeList.Codes.NT007, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarArrivalNotification);
		AssertCode(PassarMessageTypeList.Codes.NT013, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarAmendment);
		AssertCode(PassarMessageTypeList.Codes.NT014, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarWithdrawal);
		AssertCode(PassarMessageTypeList.Codes.NT015, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarDeclaration);
		AssertCode(PassarMessageTypeList.Codes.NT044, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.PassarInventoryResult);
		AssertCode(PassarMessageTypeList.Codes.NT141, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.NctsNotArrivedTransitMovement);
		AssertCode(PassarMessageTypeList.Codes.NT513, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.NctsNationalDepartureAmendment);
		AssertCode(PassarMessageTypeList.Codes.NT515, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.NctsNationalDeparture);

		AssertCode(ZString.Empty, MessageTypeCodeList.Codes.Export, "XXX");
		AssertCode(ZString.Empty, MessageTypeCodeList.Codes.Export, ZString.Empty);
		AssertCode(ZString.Empty, "XXX", MessageSubTypeCodeList.Codes.PassarDeclaration);
		AssertCode(ZString.Empty, ZString.Empty, MessageSubTypeCodeList.Codes.PassarDeclaration);

		void AssertCode(ZString expectedPassarMessageType, ZString messageType, ZString messageSubType)
		{
			AssertEquals($"{messageType} {messageSubType}", expectedPassarMessageType, PassarMessageTypeList.GetPassarMessageType(messageType, messageSubType));
		}
	});

	public void TestIsCancellationMessage() => CombineAssertions(() =>
	{
		AssertEquals("null", expected: false, PassarMessageTypeList.IsCancellationMessage(null));
		AssertEquals("string.Empty", expected: false, PassarMessageTypeList.IsCancellationMessage(string.Empty));
		AssertEquals("NE013", expected: false, PassarMessageTypeList.IsCancellationMessage(PassarMessageTypeList.Codes.NE013));
		AssertEquals("NE014", expected: true, PassarMessageTypeList.IsCancellationMessage(PassarMessageTypeList.Codes.NE014));
		AssertEquals("NI014", expected: true, PassarMessageTypeList.IsCancellationMessage(PassarMessageTypeList.Codes.NI014));
	});

	public void TestIsDataRequestMessage() => CombineAssertions(() =>
	{
		AssertEquals("null", expected: false, PassarMessageTypeList.IsDataRequestMessage(null));
		AssertEquals("string.Empty", expected: false, PassarMessageTypeList.IsDataRequestMessage(string.Empty));
		AssertEquals("NE013", expected: false, PassarMessageTypeList.IsDataRequestMessage(PassarMessageTypeList.Codes.NE013));
		AssertEquals("NC016", expected: true, PassarMessageTypeList.IsDataRequestMessage(PassarMessageTypeList.Codes.NC016));
		AssertEquals("NI016", expected: true, PassarMessageTypeList.IsDataRequestMessage(PassarMessageTypeList.Codes.NI016));
	});
}
