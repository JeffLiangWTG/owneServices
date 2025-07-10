using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE551MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC551C;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Decision</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Canceled by Customs</i></td></tr>");
			sb.Append("<tr><td><b>Cancellation Date:</b></td><td><i>" + CancellationDateTimeZoneDependent + "</i></td></tr>");
			sb.Append("<tr><td><b>Remark:</b></td><td><i>Canceled due to not following up outstanding requests</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	ZDateTime CancellationDateTimeZoneDependent
	{
		get
		{
			ZDateTime cancellationDate;
			ZDateTime.TryParseExact("20240422091010Z", out cancellationDate, "yyyyMMddHHmmssZ");
			return cancellationDate;
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC551C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference551</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><SequenceNumeric>1</SequenceNumeric><LimitDateTime formatCode='102'>20200612</LimitDateTime></AdditionalInformation><Status><EffectiveDateTime formatCode='304'>20200723143432Z</EffectiveDateTime></Status><Declaration><ID>22NL13215444</ID></Declaration></Response></MetaData>";

	protected override string BGMReference => "TestReference551";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE551MessageProcessor(logger);

	[TestDate(2024, 04, 22, 09, 10, 10)]
	public new void TestProcessMessage()
	{
		AssertProcessMessage(string.Empty, string.Empty, string.Empty, NLConstants.Status.Cancelled, NLConstants.EntryStatus.CancellationReply, string.Empty);

		var statusses = new string[] { NLConstants.StatusNew.SentToCustoms, NLConstants.StatusNew.Error, NLConstants.StatusNew.Invalid, NLConstants.StatusNew.Accepted };
		var entryStatusses = new string[] { NLConstants.EntryStatusNew.RequestForInformation, NLConstants.EntryStatusNew.PreLodged, NLConstants.EntryStatusNew.MRNAllocated, NLConstants.EntryStatusNew.Released, NLConstants.EntryStatusNew.DocumentsControl, NLConstants.EntryStatusNew.PhysicalInspection, NLConstants.EntryStatusNew.ProvisionalRelease, NLConstants.EntryStatusNew.Received };
		var phaseStatusses = new string[] { CustomsEntryPhaseStatusList.Codes.CRE, CustomsEntryPhaseStatusList.Codes._513, CustomsEntryPhaseStatusList.Codes._514, CustomsEntryPhaseStatusList.Codes._515, CustomsEntryPhaseStatusList.Codes.SUP };
		foreach (var phaseStatus in phaseStatusses)
		{
			foreach (var status in statusses)
			{
				foreach (var entryStatus in entryStatusses)
				{
					AssertProcessMessage(status, entryStatus, phaseStatus, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.NoRelease, CustomsEntryPhaseStatusList.Codes._515);
				}
			}
		}
	}

	void AssertProcessMessage(string status, string entryStatus, string phaseStatus, string expectedStatus, string expectedEntryStatus, string expectedPhaseStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;

		if (!status.IsEmpty())
		{
			entryHeader.CH_Status = status;
		}

		if (!entryStatus.IsEmpty())
		{
			entryHeader.CH_EntryStatus = entryStatus;
		}

		if (!phaseStatus.IsEmpty())
		{
			entryHeader.CH_PhaseStatus = phaseStatus;
		}

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions($"Status: {status}, EntryStatus: {entryStatus}, PhaseStatus: {phaseStatus}",() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			if (!(phaseStatus.IsEmpty() && expectedPhaseStatus.IsEmpty()))
			{
				AssertEquals("Entry Header - Phase Status", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
			}
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	[TestDate(2024, 04, 22, 09, 10, 10)]
	public void TestProcessMessage_PhaseStatus513()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", NLConstants.StatusNew.Accepted, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", NLConstants.EntryStatusNew.NoRelease, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", CustomsEntryPhaseStatusList.Codes._515, entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	public void TestWorkFlowSubStyleE_F() => CombineAssertions(() =>
	{
		var testCases = new List<(string statusBefore, string statusAfter, string entryStatusBefore, string entryStatusAfter, string phaseStatusBefore, string phaseStatusAfter)>
		{
			("ACC", "ACC", "CTL", "NRL", "515", "515"),
			("ACC", "ACC", "DOC", "NRL", "515", "515"),
			("ACC", "ACC", "RFI", "NRL", "515", "515"),
			("REM", "ACC", "RFI", "NRL", "515", "515"),
		};

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_EntryStyle = "CO";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.EntryInstruction.CEI_SubStyle = "E";
		var message = CreateNewTestMessage();
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		foreach (var testCase in testCases)
		{
			entryHeader.CH_Status = testCase.statusBefore;
			entryHeader.CH_EntryStatus = testCase.entryStatusBefore;
			entryHeader.CH_PhaseStatus = testCase.phaseStatusBefore;
			message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
			Factory.Save();

			MessageProcessor.ProcessMessage(message);

			AssertEquals($"Status '{testCase.statusBefore}', EntryStatus '{testCase.entryStatusBefore}', PhaseStatus '{testCase.phaseStatusBefore}' (Status)", testCase.statusAfter, entryHeader.CH_Status);
			AssertEquals($"Status '{testCase.statusBefore}', EntryStatus '{testCase.entryStatusBefore}', PhaseStatus '{testCase.phaseStatusBefore}' (EntryStatus)", testCase.entryStatusAfter, entryHeader.CH_EntryStatus);
			AssertEquals($"Status '{testCase.statusBefore}', EntryStatus '{testCase.entryStatusBefore}', PhaseStatus '{testCase.phaseStatusBefore}' (PhaseStatus)", testCase.phaseStatusAfter, entryHeader.CH_PhaseStatus);
		}
	});
}
