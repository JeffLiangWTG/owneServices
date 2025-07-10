using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public abstract class DeltaIEBaseProcessorTest<T, TProcessor> : TestCaseWithFactory where TProcessor : DeltaIEBaseProcessor<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		public void TestSetEvent()
		{
			var entryHeader = GetEntryHeader();
			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();
			processor.ProcessMessage(message);

			var reference = GetEventReference();
			if (!reference.IsEmpty)
			{
				AssertEquals(1, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsUpdate).Count(x => x.SL_Reference == reference));
			}
			else
			{
				AssertEquals(0, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsUpdate).Count());
			}
		}

		public void TestFeesUpdated()
		{
			var entryHeader = GetEntryHeader();

			var entryLine1 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.ConfirmedFees.AddNew();

			var entryLine2 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine1.ConfirmedFees.AddNew();

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageTextForFees());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("Existing confirmed fees should have been replaced by fees from message, or left as is if message has no taxes.", GetExpectedFeesCountAfterProcessingMessageWithTaxes(), entryHeader.AllEntryLines.Select(x => x.ConfirmedFees.Count).Sum());

			if (GetMessageTextForFees() != ZString.Empty)
			{
				AssertConfirmedEntryLineFee("entryLine1_Fee1", entryLine1.ConfirmedFees[0], "B00", "A445", 8394m, 20.0m, 1678.8m, "R", "TNE1");
				AssertConfirmedEntryLineFee("entryLine1_Fee2", entryLine1.ConfirmedFees[1], "1I1", "Q416", 8007m, 0.09m, 7m, "R", "TNE2");
				AssertConfirmedEntryLineFee("entryLine2_Fee1", entryLine2.ConfirmedFees[0], "B00", "A445", 4006m, 20.0m, 801m, "R", "TNE3");

				AssertEquals("There should be 2 confirmed charges in EntryHeader.", 2, entryHeader.ConfirmedCharges.Count);
				AssertConfirmedEntryHeaderCharge("EntryHeader_Charge1", entryHeader.ConfirmedCharges[0], "A00", 200m, "U165", "1");
				AssertConfirmedEntryHeaderCharge("EntryHeader_Charge2", entryHeader.ConfirmedCharges[1], "A30", 300m, "U235", "1");

				AssertEquals("EntryHeader Confirmed Guarantee Amount should be equal to 200.", 200m, entryHeader.CH_ConfirmedGuaranteeAmount);
			}
		}

		void AssertConfirmedEntryLineFee(ZString message, CusEntryLineFee confirmedFee, ZString typeCode, ZString nationalTypeCode, ZDecimal baseValue, ZDecimal rate, ZDecimal chargeAmount, ZString methodOfPayment, ZString methodOfCalculation)
		{
			AssertEquals(message + ":Charge type should reflect taxType.", typeCode, confirmedFee.CF_ChargeType);
			AssertEquals(message + ":Method Of Calculation should reflect methodOfCalculation.", methodOfCalculation, confirmedFee.CF_MethodOfCalculation);
			AssertEquals(message + ":Base value should reflect TaxBase.amount.", baseValue, confirmedFee.CF_BaseValue);
			AssertEquals(message + ":Rate should reflect TaxBase.taxrate.", rate, confirmedFee.CF_Rate);
			AssertEquals(message + ":Charge amount should reflect payableTaxAmount.", new Customs.Business.IntegerFeeRounder().Round(chargeAmount), confirmedFee.CF_ChargeAmount);
			AssertEquals(message + ":Method of Payment should reflect methodOfPayment.", methodOfPayment, confirmedFee.CF_MethodOfPayment);
			AssertEquals(message + ":NationalFeeTypeCode should reflect nationaltaxType.", nationalTypeCode, confirmedFee.NationalFeeTypeCode);
			AssertEquals(message + ":Source should always be 'CUS'.", Enterprise.Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CUS, confirmedFee.CF_Source);
			AssertEquals(message + ":Action should be empty.", ZString.Empty, confirmedFee.G4_RateOverride);
		}

		void AssertConfirmedEntryHeaderCharge(string message, CusEntryHeaderCharges confirmedCharge, string chargeType, decimal chargeAmount, string nationalFeeTypeCode, string taxStatus)
		{
			AssertEquals($"{message}: Charge type should be equal to {chargeType}.", chargeType, confirmedCharge.C1_ChargeType);
			AssertEquals($"{message}: Charge amount should be equal to {chargeAmount}.", chargeAmount, confirmedCharge.C1_ChargeAmount);
			AssertEquals($"{message}: National fee type code should be equal to {nationalFeeTypeCode}.", nationalFeeTypeCode, confirmedCharge.NationalFeeTypeCode);
			AssertEquals($"{message}: Tax status should be equal to {taxStatus}.", taxStatus, confirmedCharge.TaxStatus);
		}

		[TestDate(2024, 01, 08)]
		public void TestCorrectMessage_AdditionalRefsAreNotEmptyAtOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CRN = "ABC";
				entryHeader.MRN = "DEF";
				entryHeader.CorrelationID = GetExpectedLRN();
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.CRN = "ABC";
				entryHeader.CorrelationID = "DEF";
				entryHeader.MRN = GetExpectedMRN();
			}
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("MessageInterpretation should be assigned.", GetExpectedMessageInterpretation(), message.EM_MessageInterpretation);
			AssertEquals("MessageStatus should be assigned.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("CusEntryHeader should be linked to the message.", entryHeader.PK, message.EM_LinkedObject.PK);
			AssertEquals("CRN should not have been updated.", "ABC", entryHeader.CRN);

			TestDateAttribute.AddDays(3);
			AssertEquals("MessageInterpretation should be assigned.", GetExpectedMessageInterpretation(), message.EM_MessageInterpretation);

			if (EntryHeaderLocatingReferenceType() == "LRN")
			{
				AssertEquals("MRN should not have been updated.", "DEF", entryHeader.MRN);
			}
			else if (EntryHeaderLocatingReferenceType() == "MRN")
			{
				AssertEquals("LRN should not have been updated.", "DEF", entryHeader.CorrelationID);
			}
			AssertEquals("Message Status of Entry Header should have been updated.", GetExpectedEntryHeaderMessageStatus(), entryHeader.CH_Status);
			AssertEntryStatusUpdatedWithCESEvent(entryHeader);
		}

		[TestDate(2024, 01, 08)]
		protected virtual void TestCorrectMessage_AdditionalRefsAreEmptyAtOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();

			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CorrelationID = GetExpectedLRN();
				entryHeader.CRN = ZString.Empty;
				entryHeader.MRN = ZString.Empty;
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.MRN = GetExpectedMRN();
				entryHeader.CRN = ZString.Empty;
				entryHeader.CorrelationID = ZString.Empty;
			}

			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("MessageInterpretation should be assigned.", GetExpectedMessageInterpretation(), message.EM_MessageInterpretation);
			AssertEquals("MessageStatus should be assigned.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("CusEntryHeader should be linked to the message.", entryHeader.PK, message.EM_LinkedObject.PK);
			AssertEquals("CRN should have been updated.", GetExpectedCRN(), entryHeader.CRN);
			if (EntryHeaderLocatingReferenceType() == "LRN")
			{
				AssertEquals("MRN should have been updated.", GetExpectedMRN(), entryHeader.MRN);
			}
			else if (EntryHeaderLocatingReferenceType() == "MRN")
			{
				AssertEquals("LRN should have been updated.", GetExpectedLRN(), entryHeader.CorrelationID);
			}
			AssertEquals("Message Status of Entry Header should have been updated.", GetExpectedEntryHeaderMessageStatus(), entryHeader.CH_Status);
			AssertEntryStatusUpdatedWithCESEvent(entryHeader);
		}

		public void TestProcessor()
		{
			var processor = GetDeltaIEBaseProcessor();

			AssertContainsExactElementsInAnyOrder("MessageTypesToInclude of DeltaIEBaseProcessor should contains DEC.", new string[] { MessageTypeList.Codes.DEC }, processor.MessageTypesToInclude);
			AssertEquals("ApplicationCode of DeltaIEBaseProcessor should be FRC.", EDIMessage.ApplicationCodes.FRCustomsMessage, processor.ApplicationCode);
			AssertEquals("MessageFriendlyName of DeltaIEBaseProcessor should be FR DeltaIE Base Processor.", "FR DeltaIE Base Processor", processor.MessageFriendlyName);
		}

		public void TestBadlyFormattedMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();

			processor.ProcessMessage(message);
			AssertEquals("MessageStatus should be discard", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertEquals("Error note should be added to the message.", EntryHeaderErrorMessage(), (message.Notes.GetAllNotes().Last() as StmNote).ST_NoteText);
			if (EntryHeaderLocatingReferenceType() == "LRN")
			{
				entryHeader.CorrelationID = "ABC";
			}
			else if (EntryHeaderLocatingReferenceType() == "MRN")
			{
				entryHeader.MRN = "ABC";
			}

			Factory.Save();
			processor.ProcessMessage(message);
			AssertEquals("MessageStatus should be discard", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertEquals("Error note should be added to the message.", EntryHeaderErrorMessage(), (message.Notes.GetAllNotes().Last() as StmNote).ST_NoteText);
		}

		public void TestMissingFieldsNotPreventingFromMessageProcessing()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CorrelationID = GetExpectedLRN();
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.MRN = GetExpectedMRN();
			}
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();

			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = GetMessageTextForMissingMandatoryFields();
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("Should be able to process message with missing mandatory fields.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestMessageWithoutImportOperationOrEntryHeaderLocatingRef()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();

			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CorrelationID = GetExpectedLRN();
				entryHeader.CRN = "ABC";
				entryHeader.MRN = "DEF";
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.MRN = GetExpectedMRN();
				entryHeader.CRN = "ABC";
				entryHeader.CorrelationID = "DEF";
			}

			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageTextWithoutImportOperation());
			Factory.Save();

			processor.ProcessMessage(message);
			AssertEquals("MessageInterpretation should be assigned.", GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef(), message.EM_MessageInterpretation);
			AssertEquals("MessageStatus should be discard", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertEquals("Error note should be added to the message.", EntryHeaderErrorMessage(), (message.Notes.GetAllNotes().Last() as StmNote).ST_NoteText);
		}

		[TestDate(2024, 01, 08)]
		public void TestMessageWithoutAdditionalRefs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();

			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CorrelationID = GetExpectedLRN();
				entryHeader.CRN = "ABC";
				entryHeader.MRN = "DEF";
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.MRN = GetExpectedMRN();
				entryHeader.CRN = "ABC";
				entryHeader.CorrelationID = "DEF";
			}
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageTextWithoutAdditionalRefs());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("MessageStatus should be assigned.", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("CusEntryHeader should be linked to the message.", entryHeader.PK, message.EM_LinkedObject.PK);
			AssertEquals("CRN should not be updated if provided CRN is empty.", "ABC", entryHeader.CRN);

			if (EntryHeaderLocatingReferenceType() == "LRN")
			{
				AssertEquals("MRN should not be updated if provided MRN is empty.", "DEF", entryHeader.MRN);
			}
			else if (EntryHeaderLocatingReferenceType() == "MRN")
			{
				AssertEquals("LRN should not be updated if provided MRN is empty.", "DEF", entryHeader.CorrelationID);
			}
			AssertEquals("Message Status of Entry Header should have been updated.", GetExpectedEntryHeaderMessageStatus(), entryHeader.CH_Status);
			AssertEntryStatusUpdatedWithCESEvent(entryHeader);
		}

		[TestDate(2024, 01, 08)]
		public void TestCE_IssueDateIsUpdated()
		{
			var entryHeader = GetEntryHeader();
			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			if (message.EM_MessageSubType == DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification)
			{
				entryHeader.EntryNumber = "12345678";
			}
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("Message Status of Entry Header should have been updated.", GetExpectedEntryHeaderMessageStatus(), entryHeader.CH_Status);
			AssertEntryStatusUpdatedWithCESEvent(entryHeader);
			AssertEquals("CE_IssueDate of MRNEntryNumber should have been updated.", GetMRNExpectedIssueDate(), entryHeader.MRNEntryNumber?.CE_IssueDate ?? ZDateTime.Empty);
			AssertEquals("CE_IssueDate of CRNEntryNumber should have been updated.", GetCRNExpectedIssueDate(), entryHeader.CRNEntryNumber?.CE_IssueDate ?? ZDateTime.Empty);
			AssertEquals("CE_IssueDate of CusEntryNumber should have been updated.", GetExpectedCustomsEntryIssueDate(), entryHeader.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty);
		}

		public void TestCE_ExpiryDateIsUpdated()
		{
			var entryHeader = GetEntryHeader();
			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("CE_ExpiryDate of CRNEntryNumber should have been updated.", GetCRNExpectedExpiryDate(), entryHeader.CRNEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty);
		}

		void AssertEntryStatusUpdatedWithCESEvent(CusEntryHeader entryHeader)
		{
			AssertEquals("Entry Status of Entry Header should have been updated.", GetExpectedEntryHeaderEntryStatus(), entryHeader.CH_EntryStatus);
			if (!GetExpectedEntryHeaderEntryStatus().IsEmpty)
			{
				AssertEquals(GetExpectedCESLogInfo(), string.Join(",", entryHeader.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code).Select(l => $"{l.SL_SE_NKEvent} {l.SL_EventTime.ToString("yyyy-MM-dd HH:mm:ss")}")));
			}
		}

		public void TestEntryNumberIsUpdated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CorrelationID = GetExpectedLRN();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();

			AssertEquals("Entry Number should not have been updated yet.", ZString.Empty, entryHeader.EntryNumber);
			processor.ProcessMessage(message);
			AssertEquals("Entry Number should have been updated.", GetExpectedEntryNumber(), entryHeader.EntryNumber);
		}

		public void TestCH_EntryReleaseDateIsUpdated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CorrelationID = GetExpectedLRN();
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();

			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = GetMessageText();
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("CH_EntryReleaseDate of entryHeader should have been updated.", GetExpectedReleaseDate(), entryHeader.CH_EntryReleaseDate);
		}

		public CusEntryHeader GetEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				entryHeader.CorrelationID = GetExpectedLRN();
			}
			else if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
			{
				entryHeader.MRN = GetExpectedMRN();
			}
			declaration.CustomsEntryHeaders.Add(entryHeader);
			return entryHeader;
		}

		public DeltaIEFREDIMessage GetDeltaIEFREDIMessageWithMessageText(string messageText)
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			return message;
		}

		public GlbGroup SetUpStaffAndGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();
			return group;
		}

		protected abstract ZString GetExpectedMessageInterpretation();

		protected abstract ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef();

		protected ApplicationTypeMessageProcessor GetDeltaIEBaseProcessor() => (TProcessor)Activator.CreateInstance(typeof(TProcessor), new BatchProcessor.LoggingInformation());

		protected abstract ZString GetMessageSubType();

		protected abstract ZString GetMessageText();

		protected abstract ZString GetMessageTextWithoutAdditionalRefs();

		protected abstract ZString GetMessageTextWithoutImportOperation();

		protected abstract ZString GetMessageTextForFees();

		protected abstract ZString GetExpectedLRN();

		protected abstract ZString GetExpectedCRN();

		protected abstract ZString GetExpectedMRN();

		protected virtual ZDateTime GetExpectedIssueDate() => ZDateTime.Empty;

		protected virtual ZDateTime GetCRNExpectedIssueDate() => GetExpectedIssueDate();

		protected virtual ZDateTime GetMRNExpectedIssueDate() => GetExpectedIssueDate();

		protected virtual ZDateTime GetCRNExpectedExpiryDate() => ZDateTime.Empty;

		protected abstract ZString GetExpectedEntryHeaderMessageStatus();

		protected abstract ZString GetExpectedEntryHeaderEntryStatus();

		protected virtual ZString GetExpectedCESLogInfo() => ZString.Empty;

		protected abstract ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes();

		protected abstract ZString GetEventReference();

		protected abstract ZString GetMessageTextForMissingMandatoryFields();

		protected virtual ZString GetExpectedEntryNumber() => ZString.Empty;

		protected virtual ZDateTime GetExpectedCustomsEntryIssueDate() => ZDateTime.Empty;

		protected virtual ZDateTime GetExpectedReleaseDate() => ZDateTime.Empty;

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected virtual ZString EntryHeaderLocatingReferenceType() => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		protected ZString EntryHeaderErrorMessage()
		{
			return $"Couldn't locate Job using provided {EntryHeaderLocatingReferenceType()}#";
		}
	}
}
