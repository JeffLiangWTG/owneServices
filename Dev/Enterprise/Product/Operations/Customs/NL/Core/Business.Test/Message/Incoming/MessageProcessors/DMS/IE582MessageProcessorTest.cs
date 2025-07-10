using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE582MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC582C;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Reminder</i></td></tr>");
			sb.Append("<tr><td><b>Expiry Date:</b></td><td><i>20240317</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>Declaration awaiting Exit confirmation</i></td></tr>");
			sb.Append("<tr><td><b>Customs Remark:</b></td><td><i>If no Exit information has been received before Expiry the declaration may be canceled by Customs</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC582C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response></Response></MetaData>";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE582MessageProcessor(logger);

	[TestDate(2023, 10, 19)]
	public new void TestProcessMessage()
	{
		var subStyles = new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF };
		var inputParameters = new List<(ZString status, ZString entryStatus, ZString expectedStatus, ZString expectedEntryStatus)>
		{
			(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.ProvisionalRelease, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.ProvisionalRelease),
			(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Released, NLConstants.Status.Reminder, NLConstants.EntryStatusNew.Released),
			(NLConstants.Status.Reminder, NLConstants.EntryStatusNew.ProvisionalRelease, NLConstants.Status.Reminder, NLConstants.EntryStatusNew.ProvisionalRelease),
			(NLConstants.Status.Reminder, NLConstants.EntryStatusNew.Released, NLConstants.Status.Reminder, NLConstants.EntryStatusNew.Released),
		};

		AssertProcessMessage(string.Empty, string.Empty, string.Empty, NLConstants.Status.Reminder, "810");

		foreach (var subStyle in subStyles)
		{
			foreach (var inputParameter in inputParameters)
			{
				AssertProcessMessage(subStyle, inputParameter.status, inputParameter.entryStatus, inputParameter.expectedStatus, inputParameter.expectedEntryStatus);
			}
		}
	}

	public void AssertProcessMessage(string subStyle, string status, string entryStatus, string expectedStatus, string expectedEntryStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2023, 10, 19);

		if (!subStyle.IsEmpty())
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_SubStyle = subStyle;
		}

		if (!status.IsEmpty())
		{
			entryHeader.CH_Status = status;
		}

		if (!entryStatus.IsEmpty())
		{
			entryHeader.CH_EntryStatus = entryStatus;
		}

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions($"SubStyle: {subStyle}, Status: {status}, EntryStatus: {entryStatus}", () =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Expiry Date", new ZDateTime(2024, 3, 17), mrnEntryNumber.CE_ExpiryDate);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}
}
