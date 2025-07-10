using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSendingObjectLookups))]
sealed class ImportDeclarationMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCorrectionReasonList()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);

		var list = SendingObject.Lookups.CorrectionReasonList;

		AssertEquals("CorrectionReasonList", "2, 99", list.CodesAsString);
	}

	public void TestMessageTypeList() => CombineAssertions(() =>
	{
		var entryHeader = SendingObject.Header;

		AssertMessageTypeList(ZString.Empty, ZString.Empty, PassarMessageTypeList.Codes.NI015);

		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI016);

		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI013, PassarMessageTypeList.Codes.NI014, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, ZString.Empty);

		AssertMessageTypeList(CHLogicalStatusList.Codes.Acknowledged, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Acknowledged, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Acknowledged, ZString.Empty);

		AssertMessageTypeList(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI013, PassarMessageTypeList.Codes.NI014);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI014, PassarMessageTypeList.Codes.NI013);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI015);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Invalid, ZString.Empty);

		AssertMessageTypeList(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI013, PassarMessageTypeList.Codes.NI014);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI014, PassarMessageTypeList.Codes.NI013);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI015);
		AssertMessageTypeList(CHLogicalStatusList.Codes.Failed, ZString.Empty);

		void AssertMessageTypeList(ZString status, ZString phaseStatus, params string[] expectedMessageTypes)
		{
			entryHeader.CH_Status = status;
			entryHeader.CH_PhaseStatus = phaseStatus;

			AssertContainsExactElementsInAnyOrder($"{status}/{phaseStatus}: expected types {string.Join(", ", expectedMessageTypes)}", expectedMessageTypes, SendingObject.Lookups.MessageTypeList.GetAllCodes());
		}
	});

	ImportDeclarationMessageSendingObject SendingObject => sendingObject ??= CreateMessageSendingObject();
	ImportDeclarationMessageSendingObject sendingObject;

	ImportDeclarationMessageSendingObject CreateMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new ImportDeclarationMessageSendingObject(entryHeader);
	}
}
