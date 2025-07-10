using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DMSResponseMessageHelperTest : TestCaseWithFactory
{
	public void TestCreateDMSIncomingDataProvider()
	{
		AssertEquals("CC431A", DMSResponseMessageHelper.CreateDMSIncomingDataProvider(Correct431Message).WCOTypeCode);
	}

	public void TestCreateDMSIncomingDataProvider_Invalid()
	{
		DMSResponseMessageHelper.CreateDMSIncomingDataProvider("XXXXX");
		AssertContains("Failed to deserialize incoming NL DMS xml:", ErrorReporter.LastMessageReported);
		ErrorReporter.Clear();
	}

	public void TestCreateControlIncomingDataProvider()
	{
		AssertEquals("2025042300001", DMSResponseMessageHelper.CreateControlIncomingDataProvider(CorrectControlMessage).Response?.FunctionalReferenceId);
	}

	public void TestCreateControlIncomingDataProvider_Invalid()
	{
		DMSResponseMessageHelper.CreateControlIncomingDataProvider("XXXXX");
		AssertContains("Failed to deserialize incoming NL Control xml:", ErrorReporter.LastMessageReported);
		ErrorReporter.Clear();
	}

	public void TestGetMessageEntryStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		CombineAssertions(() =>
		{
			AssertGetMessageEntryStatus(WCoTypeCodes.AmendmentAccepted, EntryStatus.AmendmentAccepted);
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportAmendmentAccepted, EntryStatus.AmendmentAccepted);
			AssertGetMessageEntryStatus(WCoTypeCodes.InvalidationConfirmation, EntryStatus.Cancelled);
			AssertGetMessageEntryStatus(WCoTypeCodes.DeclarationAcceptance, EntryStatus.AdvanceDeclarationReceived);
			AssertGetMessageEntryStatus(WCoTypeCodes.Acceptance, EntryStatus.Accepted);
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportAcceptance, EntryStatusNew.MRNAllocated);

			var status = DMSResponseMessageTestHelper.MockResponseStatus(nameCode: StatusNameCodes.Released);
			dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status.Object });
			AssertGetMessageEntryStatus(WCoTypeCodes.Release, EntryStatus.ReleasedAndTaxed);
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportRelease, EntryStatusNew.Released);

			AssertGetMessageEntryStatus(WCoTypeCodes.SupplementReminder, EntryStatus.SupplementReminder);
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportSupplementReminder, EntryStatus.SupplementReminder);

			AssertGetMessageEntryStatus(WCoTypeCodes.ExitReminder, EntryStatus.NoExitInformationRecievedYet);

			entryHeader.EntryInstruction.CEI_SubStyle = "B";
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportSupplementReminder, EntryStatusNew.ProvisionalRelease);

			entryHeader.EntryInstruction.CEI_SubStyle = "C";
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportSupplementReminder, EntryStatusNew.ProvisionalRelease);

			var subStyles = new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF };
			foreach (var subStyle in subStyles)
			{
				entryHeader.EntryInstruction.CEI_SubStyle = subStyle;
				AssertGetMessageEntryStatus(WCoTypeCodes.ExportSupplementReminder, EntryStatusNew.ProvisionalRelease);
				entryHeader.CH_EntryStatus = EntryStatusNew.ProvisionalRelease;
				AssertGetMessageEntryStatus(WCoTypeCodes.ExitReminder, EntryStatusNew.ProvisionalRelease);
				entryHeader.CH_EntryStatus = EntryStatusNew.Released;
				AssertGetMessageEntryStatus(WCoTypeCodes.ExitReminder, EntryStatusNew.Released);
			}

			AssertGetMessageEntryStatus(WCoTypeCodes.ReminderForInformation, string.Empty);
			AssertGetMessageEntryStatus(WCoTypeCodes.NoRelease, EntryStatus.NoRelease_NRE);

			dataProviderMock.Setup(x => x.BusinessRejectionTypeCode).Returns(RejectionStatus.Rejection_413);
			AssertGetMessageEntryStatus(WCoTypeCodes.Rejection, EntryStatus.Rejection_413);
			dataProviderMock.Setup(x => x.BusinessRejectionTypeCode).Returns(RejectionStatus.Rejection_513);
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportRejection, EntryStatus.ExportRejection_513);

			var control = DMSResponseMessageTestHelper.MockResponseControl(typeCode: ControlTypes.DocumentsControl);
			dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control.Object });
			AssertGetMessageEntryStatus(WCoTypeCodes.ControlNotification, "310");
			AssertGetMessageEntryStatus(WCoTypeCodes.ExportControlNotification, "310");

			AssertGetMessageEntryStatus(WCoTypeCodes.Cancellation, EntryStatus.ExportCancellation);
			AssertGetMessageEntryStatus(WCoTypeCodes.CancellationReply, EntryStatus.CancellationReply);
			AssertGetMessageEntryStatus(WCoTypeCodes.Ext, string.Empty);

			entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationSent;
			AssertGetMessageEntryStatus(WCoTypeCodes.ReceiveMessage, EntryStatus.AdvanceDeclarationReceived);

			AssertGetMessageEntryStatus(WCoTypeCodes.PreliminaryDeclarationAccepted, EntryStatus.AdvanceDeclarationReceived);

			var controlResult = DMSResponseMessageTestHelper.MockResponseControlResult();
			dataProviderMock.Setup(x => x.ControlResults).Returns(new IDMSControlResult[] { controlResult.Object });
			var controlResultControl = DMSResponseMessageTestHelper.MockResponseControlResultControl(typeCode: ControlTypes.PhysicalInspection);
			controlResult.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControl.Object });
			AssertGetMessageEntryStatus(WCoTypeCodes.RequestForInformation, "340");

			AssertGetMessageEntryStatus(WCoTypeCodes.RequestForInformationReminder, EntryStatus.ExportReminder_NoExitInformationReceived);
			entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;
			AssertGetMessageEntryStatus(WCoTypeCodes.RequestForInformationReminder, string.Empty);
			AssertGetMessageEntryStatus(string.Empty, string.Empty);
		});

		void AssertGetMessageEntryStatus(string typeCode, string expectedEntryStatus)
		{
			dataProviderMock.Setup(x => x.WCOTypeCode).Returns(typeCode);
			AssertEquals($"WCOTypeCode {typeCode}", expectedEntryStatus, DMSResponseMessageHelper.GetMessageEntryStatus(entryHeader, dataProviderMock.Object));
		}
	}

	public void TestGetMessageEntryStatus_Rejection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.Rejection);
		CombineAssertions(() =>
		{
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_413, string.Empty, EntryStatus.Rejection_413);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_414, string.Empty, EntryStatus.Rejection_414);

			AssertGetMessageEntryStatus(RejectionStatus.Rejection_415, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntryStatus.Rejection_415_for_SubStyle_XY);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_415, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntryStatus.Rejection_415_for_SubStyle_XY);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_415, "XX", EntryStatus.Rejection_415_not_SubStyle_XY);

			AssertGetMessageEntryStatus(RejectionStatus.Rejection_432, string.Empty, EntryStatus.Rejection_432);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_CRI, string.Empty, EntryStatus.Rejection_CRI);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_513, string.Empty, EntryStatus.ExportRejection_513);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_514, string.Empty, EntryStatus.ExportRejection_514);

			AssertGetMessageEntryStatus(RejectionStatus.Rejection_515, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntryStatus.ExportRejection_515_for_SubStyle_XY);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_515, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntryStatus.ExportRejection_515_for_SubStyle_XY);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_515, "XX", EntryStatus.Rejection_415_not_SubStyle_XY);

			AssertGetMessageEntryStatus(RejectionStatus.Rejection_511, string.Empty, EntryStatus.ExportRejection_511);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_CRE, string.Empty, EntryStatus.ExportRejection_CRE);
			AssertGetMessageEntryStatus(RejectionStatus.Rejection_583, string.Empty, EntryStatus.ExportRejection_583);
			AssertGetMessageEntryStatus("Invalid", string.Empty, string.Empty);
		});

		void AssertGetMessageEntryStatus(string businessRejectionTypeCode, string subStyle, string expectedEntryStatus)
		{
			entryInstruction.CEI_SubStyle = subStyle;
			dataProviderMock.Setup(x => x.BusinessRejectionTypeCode).Returns(businessRejectionTypeCode);
			AssertEquals($"BusinessRejectionTypeCode - {businessRejectionTypeCode}, SubStyle - {subStyle}", expectedEntryStatus, DMSResponseMessageHelper.GetMessageEntryStatus(entryHeader, dataProviderMock.Object));
		}
	}

	public void TestGetMessageEntryStatus_Receive()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.ReceiveMessage);
		CombineAssertions(() =>
		{
			AssertGetMessageEntryStatus(EntryStatus.AdvanceDeclarationSent, EntryStatus.AdvanceDeclarationReceived);
			AssertGetMessageEntryStatus(EntryStatus.InformationSentToCustoms, EntryStatus.InformationReceivedByCustoms);
			AssertGetMessageEntryStatus(EntryStatus.SupplementSent, EntryStatus.SupplementReceivedByCustoms);
			AssertGetMessageEntryStatus(EntryStatus.InvalidationRequestSent, EntryStatus.InvalidationRequestReceivedByCustoms);
			AssertGetMessageEntryStatus(EntryStatus.AmendmentRequestSent, EntryStatus.AmendmentRequestReceivedByCustoms);
			AssertGetMessageEntryStatus(EntryStatus.ExitInformationDetailsSent, EntryStatus.ExitInformationDetailsReceived);
			AssertGetMessageEntryStatus(string.Empty, string.Empty);
		});

		void AssertGetMessageEntryStatus(string previousEntryStatus, string expectedEntryStatus)
		{
			entryHeader.CH_EntryStatus = previousEntryStatus;
			AssertEquals($"Previous CH_EntryStatus - {previousEntryStatus}", expectedEntryStatus, DMSResponseMessageHelper.GetMessageEntryStatus(entryHeader, dataProviderMock.Object));
		}
	}

	public void TestGetReleaseMessageStatuses()
	{
		CombineAssertions(() =>
		{
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();

			var (entryStatus1, status1) = DMSResponseMessageHelper.GetReleaseMessageStatuses(dataProviderMock.Object);
			AssertEquals("No Statuses=>EntryStatus", ZString.Empty, entryStatus1);
			AssertEquals("No Statuses=>Status", ZString.Empty, status1);

			var status = DMSResponseMessageTestHelper.MockResponseStatus(nameCode: StatusNameCodes.Released);
			dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status.Object });
			var (entryStatus2, status2) = DMSResponseMessageHelper.GetReleaseMessageStatuses(dataProviderMock.Object);
			AssertEquals("NameCode 4=>EntryStatus", EntryStatus.ReleasedAndTaxed, entryStatus2);
			AssertEquals("NameCode 4=>Status", Status.CLE, status2);

			status.Setup(x => x.NameCode).Returns(StatusNameCodes.NoRelease);
			var (entryStatus3, status3) = DMSResponseMessageHelper.GetReleaseMessageStatuses(dataProviderMock.Object);
			AssertEquals("NameCode 109=>EntryStatus", EntryStatus.NoRelease, entryStatus3);
			AssertEquals("NameCode 109=>Status", Status.Cancelled, status3);

			status.Setup(x => x.NameCode).Returns(StatusNameCodes.ProvisionalRelease);
			var (entryStatus4, status4) = DMSResponseMessageHelper.GetReleaseMessageStatuses(dataProviderMock.Object);
			AssertEquals("NameCode 115=>EntryStatus", EntryStatus.ProvisionalRelease, entryStatus4);
			AssertEquals("NameCode 115=>Status", Status.ROG, status4);
		});
	}

	public void TestGetReleaseMessageStatusesForExport()
	{
		CombineAssertions(() =>
		{
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();

			var (entryStatus1, status1) = DMSResponseMessageHelper.GetReleaseMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("EntryStatus when No Statuses", ZString.Empty, entryStatus1);
			AssertEquals("MessageStatus when No Statuses", ZString.Empty, status1);

			var status = DMSResponseMessageTestHelper.MockResponseStatus(nameCode: StatusNameCodes.Released);
			dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status.Object });
			var (entryStatus2, status2) = DMSResponseMessageHelper.GetReleaseMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("EntryStatus when NameCode 4", EntryStatusNew.Released, entryStatus2);
			AssertEquals("MessageStatus when NameCode 4", StatusNew.Accepted, status2);

			status.Setup(x => x.NameCode).Returns(StatusNameCodes.NoRelease);
			var (entryStatus3, status3) = DMSResponseMessageHelper.GetReleaseMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("EntryStatus when NameCode 109", EntryStatusNew.NoRelease, entryStatus3);
			AssertEquals("MessageStatus when NameCode 109", StatusNew.Accepted, status3);

			status.Setup(x => x.NameCode).Returns(StatusNameCodes.ProvisionalRelease);
			var (entryStatus4, status4) = DMSResponseMessageHelper.GetReleaseMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("EntryStatus when NameCode 115", EntryStatusNew.ProvisionalRelease, entryStatus4);
			AssertEquals("MessageStatus when NameCode 115", StatusNew.Accepted, status4);
		});
	}

	public void TestGetControlNotificationMessageStatuses()
	{
		CombineAssertions(() =>
		{
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();

			var (entryStatus1, status1) = DMSResponseMessageHelper.GetControlNotificationMessageStatuses(dataProviderMock.Object);
			AssertEquals("No Controls=>EntryStatus", ZString.Empty, entryStatus1);
			AssertEquals("No Controls=>Status", ZString.Empty, status1);

			var control = DMSResponseMessageTestHelper.MockResponseControl(typeCode: ControlTypes.DocumentsControl);
			dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control.Object });
			var (entryStatus2, status2) = DMSResponseMessageHelper.GetControlNotificationMessageStatuses(dataProviderMock.Object);
			AssertEquals("TypeCode 10=>EntryStatus", "310", entryStatus2);
			AssertEquals("TypeCode 10=>Status", MessageStatuses.DOC, status2);

			control.Setup(x => x.TypeCode).Returns("40");
			var (entryStatus3, status3) = DMSResponseMessageHelper.GetControlNotificationMessageStatuses(dataProviderMock.Object);
			AssertEquals("TypeCode 40=>EntryStatus", "340", entryStatus3);
			AssertEquals("TypeCode 40=>Status", MessageStatuses.FYC, status3);
		});
	}

	public void TestGetControlNotificationMessageStatusesForExport()
	{
		CombineAssertions(() =>
		{
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();

			var (entryStatus1, status1) = DMSResponseMessageHelper.GetControlNotificationMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("No Controls=>EntryStatus", ZString.Empty, entryStatus1);

			var control = DMSResponseMessageTestHelper.MockResponseControl(typeCode: ControlTypes.DocumentsControl);
			dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control.Object });
			var (entryStatus2, status2) = DMSResponseMessageHelper.GetControlNotificationMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("TypeCode 10=>EntryStatus", EntryStatusNew.DocumentsControl, entryStatus2);

			control.Setup(x => x.TypeCode).Returns("40");
			var (entryStatus3, status3) = DMSResponseMessageHelper.GetControlNotificationMessageStatusesForExport(dataProviderMock.Object);
			AssertEquals("TypeCode 40=>EntryStatus", EntryStatusNew.PhysicalInspection, entryStatus3);
		});
	}

	public void TestGetFormattedShortDateString()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Null", ZString.Empty, DMSResponseMessageHelper.GetFormattedShortDateString(null));

			AssertEquals("Not null", "20230811", DMSResponseMessageHelper.GetFormattedShortDateString(new DateTime(2023, 08, 11)));
		});
	}

	public void TestGetFormattedLocalLongTimeString()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Null", ZString.Empty, DMSResponseMessageHelper.GetFormattedLocalLongTimeString(null));

			AssertEquals("Not null", new DateTime(2023, 08, 11, 10, 34, 32).ToLocalTime().ToString("dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture), DMSResponseMessageHelper.GetFormattedLocalLongTimeString(new DateTime(2023, 08, 11, 10, 34, 32)));
		});
	}

	public void TestGetRefNumbersForRequestedDocument()
	{
		CombineAssertions(() =>
		{
			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
			AssertEquals("Ref Numbers for requested document TRA (IE460)", "TRA-123, TRA-456", DMSResponseMessageHelper.GetRefNumbersForRequestedDocument(entryHeader, "TRA"));
			AssertEquals("Ref Numbers for requested document REF (IE460)", "REF-123, REF-456", DMSResponseMessageHelper.GetRefNumbersForRequestedDocument(entryHeader, "REF"));
			AssertEquals("Ref Numbers for requested document XXX (IE460)", ZString.Empty, DMSResponseMessageHelper.GetRefNumbersForRequestedDocument(entryHeader, "XXX"));
		});
	}

	public void TestGetBranchPkFromJobBO()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals(entryHeader.RegistryBranchPK, DMSResponseMessageHelper.GetBranchPkFromJobBO(entryHeader));
			AssertEquals(ZGuid.Empty, DMSResponseMessageHelper.GetBranchPkFromJobBO(declaration));
		});
	}

	const string CorrectControlMessage = "<?xml version='1.0' encoding='UTF-8'?><XML_Control xmlns='urn:wco:datamodel:XML_Control:1' xmlns:clm63055='urn:un:unece:uncefact:codelist:standard:UNECE:AgencyIdentificationCode:D12B' xmlns:ds='XML_Control:DS:1'><DocumentMetaData><WCODataModelVersionCode>3.50</WCODataModelVersionCode><ResponsibleCountryCode>NL</ResponsibleCountryCode><ResponsibleAgencyName>DOUANE</ResponsibleAgencyName><AgencyAssignedCustomizationVersionCode>v1.1</AgencyAssignedCustomizationVersionCode><CommunicationMetaData><ApplicationReferenceID>DMS</ApplicationReferenceID><PreparationDateTime formatCode=\"304\">Z</PreparationDateTime></CommunicationMetaData></DocumentMetaData><Response><Function>Not Accepted</Function><FunctionalReferenceID>2025042300001</FunctionalReferenceID><Error><Description>Error message: Element &apos;CommunicationsAgreementID&apos; is not valid for content model: &apos;(ApplicationReferenceID,CommunicationsAgreementID?,PreparationDateTime,Recipient,Sender)&apos;</Description><Pointer><Location>Line-number: 13 ### Column-number: 27</Location></Pointer></Error><Error><Description>Error message: Datatype error: Type:InvalidDatatypeValueException, Message:Value &apos;1118 AB 1&apos; does not match regular expression facet &apos;[A-Z]{2}&apos; .</Description><Pointer><Location>Line-number: 56 ### Column-number: 49</Location></Pointer></Error></Response></XML_Control>";

	const string Correct431Message = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC431A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><Control><LimitDateTime>20211015</LimitDateTime><AdditionalInformation><StatementDescription>StatementDescription</StatementDescription></AdditionalInformation></Control></Response></MetaData>";
}
