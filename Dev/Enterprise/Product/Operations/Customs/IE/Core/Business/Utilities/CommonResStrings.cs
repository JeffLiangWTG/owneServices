using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public static class CommonResStrings
	{
		public static MultilingualString ShouldNotDeleteDeclaredDataString => ResString.GetMultilingualString("22D3F486-B8AC-4070-8EB9-13CCECE7A389", "This may not be deleted from a Declaration that was originally declared to Customs.");

		public static string ShouldNotAmendThisValue => Res.GetString("F57BF821-32C6-41E0-8397-7347E93E332C", "This field may not be amended to a value that is different to what was originally declared to Customs.");

		public static string Status => Res.GetString("6DD90E22-C121-46BC-AE3A-6311B171BFF9", "Status");

		public static string InvalidationRequestCancellationReason => Res.GetString("0602D673-926F-4110-AF65-9A10999D6D21", "Invalidation Request Cancellation Reason");

		public static string StoringFlag => Res.GetString("F096223D-5B52-430B-8D9B-CA235736EE10", "Storing Flag");

		public static string AdministrativeReferenceCode => Res.GetString("96EFEF13-CF0A-4CBA-9583-3614779B0C7E", "Administrative Reference Code");

		public static string LocalReferenceNumber => Res.GetString("24F9C6B8-5A75-454A-94A4-CE2668B4AFFE", "LRN");

		public static string MovementReferenceNumber => Res.GetString("8E311626-6B03-4428-9C96-5316866F92F7", "MRN");

		public static string CustomsRegistrationNumber => Res.GetString("943B54F2-58ED-42BA-858D-6ECE3557B808", "Customs Registration Number");

		public static string TransactionId => Res.GetString("A4DB7B77-2738-4A15-AEC6-C6CF8164CF41", "Transaction ID");

		public static string MessageType => Res.GetString("E4EC27A4-0B24-4248-82D9-38E7E2BA4B78", "Message Type");

		public static string MessageStatus => Res.GetString("18E2D7FD-4E45-4EF8-98DB-5D1191557641", "Message Status");

		public static string MessageNumber => Res.GetString("76632FC4-30E3-4013-89F9-58C4DFAB5D61", "Message Number");

		public static string AcceptanceDate => Res.GetString("0DE88968-EF95-487F-8FB5-C798FDC5021C", "Acceptance Date");

		public static string DeclarationAcceptanceDate => Res.GetString("6A521427-2F3F-4175-9F63-C8E967B6CB6C", "Declaration Acceptance Date");

		public static string AmendmentRejectionDate => Res.GetString("19CF1963-511A-4C7E-8D41-4C555283D582", "Amendment Rejection Date");

		public static string ReleaseDate => Res.GetString("EC193AFE-241B-424D-8592-C63EFC85AEB2", "Release Date");

		public static string Code => Res.GetString("A61954F7-3270-4791-BB1E-D89C7332A42A", "Code");

		public static string Date => Res.GetString("67FAF548-F65E-424E-A48C-3B5F6B6EB311", "Date");

		public static string ControlResult => Res.GetString("40EE0CD8-DB96-4066-A99F-61F698068138", "Control Result");

		public static string ControlResults => Res.GetString("B812C3E7-FFD1-440F-B74D-1DD7E7A96F2D", "Control Results");

		public static string DeclarationGoodsItemNumber => Res.GetString("7F8B3F38-7BD0-47CA-8326-D7D394FA23EE", "Declaration Goods Item Number");

		public static string ControlResultCode => Res.GetString("A696B5A4-FEC6-405A-888F-205FB42B86CD", "Control Result Code");

		public static string RiskAreaCode => Res.GetString("E7B2CDDA-FF7F-44B8-AB96-E3B9029C9D0B", "Risk Area Code");

		public static string ResultsOfControl => Res.GetString("3259AE23-B374-4075-890A-977F6AA30A22", "Results of Control");

		public static string RiskAreaCodeDescription => Res.GetString("182B4BCA-5B39-43B0-BA02-44F97A04C6DB", "Risk Area Code Description");

		public static string ControlTypeDescription => Res.GetString("92D76D89-3143-452A-9347-FB8BC81A56FB", "Control Type Description");

		public static string ControlDate => Res.GetString("38B68711-6715-4138-9706-B63E2CAD059C", "Control Date");

		public static string ControlDetails => Res.GetString("34826614-9318-4A28-990E-6C4F342553B8", "Control Details");

		public static string TypeOfDiscrepancies => Res.GetString("F304923A-84E2-487A-A6A3-A69D68FC67C8", "Type of Discrepancies");

		public static string AttributePointer => Res.GetString("7A16255A-88B2-4975-BE62-4B08EE1D0B7F", "Attribute Pointer");

		public static string CorrectedValue => Res.GetString("768A1B53-77A3-44FB-A0D4-BB3E84C47E50", "Corrected Value");

		public static string ControlType => Res.GetString("261E3BF4-3842-434E-85B7-18BE4D9A5A0C", "Control Type");

		public static string ControlText => Res.GetString("0734CE9F-A93D-41CD-99A0-2131CD2BF339", "Control Text");

		public static string RequestedDocumentType => Res.GetString("81FCB084-C9D0-4385-9C5E-7E40CD65401E", "Requested Document Type");

		public static string RequestedDocumentDescription => Res.GetString("5ED1C5E9-B265-4602-804B-0F5913BC439A", "Requested Document Description");

		public static string CaseId => Res.GetString("DFE87C26-9AC4-45DF-A0B2-BCC37B762298", "Case Id");

		public static string Remarks => Res.GetString("8C0B11E2-9EF1-423C-BC9C-EF7B06CE3539", "Remarks");

		public static string ErrorLineNumber => Res.GetString("52B35981-E7E0-4D56-9AFF-3F88A7E6B2F0", "Error Line Number");

		public static string ErrorColumnNumber => Res.GetString("52B35981-E7E0-4D56-9AFF-3F88A7E6B2F1", "Error Column Number");

		public static string ErrorLocation => Res.GetString("45D6B973-B5AB-438A-8872-9AA2DE9E613E", "Error Location");

		public static string ErrorPointer => Res.GetString("52B35981-E7E0-4D56-9AFF-3F88A7E6B2F2", "Error Pointer");

		public static string ErrorCode => Res.GetString("52B35981-E7E0-4D56-9AFF-3F88A7E6B2F3", "Error Code");

		public static string ErrorType => Res.GetString("D6666FAB-9D5F-4F67-B09A-242EAFF1711B", "Error Type");

		public static string ErrorText => Res.GetString("52B35981-E7E0-4D56-9AFF-3F88A7E6B2F4", "Error Text");

		public static string ErrorMessage => Res.GetString("D137B13D-2DD5-4511-BF0A-30899747455B", "Error Message");

		public static string ErrorCodeDescription => Res.GetString("2E0815DB-4A74-41F4-B199-3A6FB98CAD4A", "Error Code Description");

		public static string ErrorTypeDescription => Res.GetString("A8AAF4C0-EE9A-4714-9CE4-04BE17AD4A58", "Error Type Description");

		public static string FunctionalError => Res.GetString("6F201983-97BF-4B49-B399-B9956A000234", "Functional Error");

		public static string OriginalAttributeValue => Res.GetString("BB28E5CA-E111-4739-B25E-687B308A9454", "Original Attribute Value");

		public static string ReExportNotificationRegistrationDate => Res.GetString("97768F7A-3A85-4FC8-A0B3-5762D0EAD74B", "Re-Export Notification Registration Date");

		public static string AmendmentSubmissionDateAndTime => Res.GetString("178368FA-CF87-4667-8982-2D3B9FC76169", "Amendment Submission Date and Time");

		public static string AmendmentAcceptanceDateAndTime => Res.GetString("3B402F6C-4D08-4F44-ACEF-73862595B021", "Amendment Acceptance Date and Time");

		public static string AmendmentAcceptanceDate => Res.GetString("d3221c3b-2614-466d-8061-dcbbf3fca09c", "Amendment Acceptance Date");

		public static string AmendmentRejectionMotivationText => Res.GetString("ec558149-7587-416c-bc3d-a100af71d183", "Amendment Rejection Motivation Text");

		public static string InvalidationDecision => Res.GetString("BE55DF3C-21A4-417F-9EC0-DE29696673CD", "Invalidation Decision");

		public static string InvalidationInitiatedByCustoms => Res.GetString("8A53D7E5-7286-4412-AA8C-C049F7F0CF43", "Invalidation Initiated by Customs");

		public static string InvalidationJustification => Res.GetString("67352F01-CC8B-4BC9-AADC-EC5A744AE053", "Invalidation Justification");

		public static string DateOfInvalidationDecision => Res.GetString("801EF31F-C676-4172-B11B-B7332A7A6A9C", "Date of Invalidation Decision");

		public static string DateOfInvalidationRequest => Res.GetString("8F1BA88A-6533-4227-8A5C-8F5B1FCEC321", "Date of Invalidation Request");

		public static string DateOfInvalidation => Res.GetString("A3C7B9BE-E1BB-454B-A044-63429B7D1ED9", "Date of Invalidation");

		public static string InvalidationDecisionDateAndTime => Res.GetString("02CF07B4-5DF8-48E6-A5BE-E841690A8585", "Invalidation Decision Date and Time");

		public static string InvalidationRequestDateAndTime => Res.GetString("D2E7BA21-DBD9-4371-9ACC-E75B94A67BD8", "Invalidation Request Date and Time");

		public static string RejRejected => Res.GetString("7752C121-E806-4A0F-8173-378C2DAB3B41", "REJ - Rejected");

		public static string BusinessRejectionType => Res.GetString("33AADCFB-80A0-4AAB-9B33-78A3C41CBBBE", "Business Rejection Type");

		public static string RejectionDate => Res.GetString("04A35D39-8434-46ED-BA79-C26B050E876A", "Rejection Date");

		public static string RejectionDateAndTime => Res.GetString("33AADCFB-80A0-4AAB-9B33-78A3C41CBBBF", "Rejection Date and Time");

		public static string RejectionCode => Res.GetString("33AADCFB-80A0-4AAB-9B33-78A3C41CBBB1", "Rejection Code");

		public static string RejectionReason => Res.GetString("33AADCFB-80A0-4AAB-9B33-78A3C41CBBB2", "Rejection Reason");

		public static string RejectionMotivationText => Res.GetString("F55EEED9-9F46-4A4E-AAA4-42B594F4D610", "Rejection Motivation Text");

		public static string ControlNotificationDateAndTime => Res.GetString("8497D9AE-A825-4CF1-A0D3-B99BE1FA18DD", "Control Notification Date & Time");

		public static string ProvideAtLeastOneTransportDocument => Res.GetString("063AAD4E-B726-497F-A267-D8B409A5F80A", "Please provide at least one Transport Document.");

		public static string DeclarationAcknowledgementDate => Res.GetString("004EA8A3-B24B-4D37-818E-665A25622D03", "Declaration Acknowledgement Date");

		public static string ErrorReason => Res.GetString("DDD57C79-7393-4F0F-8C81-2520458FA304", "Error Reason");

		public static string ReleasedForExit => Res.GetString("E9D46A97-AB1E-4D8B-BAA8-3C96E4BD40E2", "Released for Exit");

		public static string DocumentType => Res.GetString("9F511A71-69B2-4560-85DF-53C605B1FA14", "Document Type");

		public static string DocumentComplementaryInformation => Res.GetString("15E74A99-1E3F-477E-88B9-6A5992FCD330", "Document Complementary Information");

		public static string DocumentsPresentRequestCancellationReason => Res.GetString("268A488D-B861-46F9-8E0F-4368A0AC5E0C", "Documents Presentation Request Cancellation Reason");

		public static string DocumentsUploadRequestCancellationReason => Res.GetString("3869145D-5436-4096-B3DF-3F4F1D00EEBE", "Documents Upload Request Cancellation Reason");

		public static string RequestDate => Res.GetString("0F19EA02-17FE-49F8-A382-B89C8D46A716", "Request Date");

		public static string ExpirationDate => Res.GetString("43208CE6-3224-4C2B-9A8A-43F8B4C2C5BE", "Expiration Date");

		public static string DateLimit => Res.GetString("477C15F6-035A-4C7E-B939-4FFB661B808F", "Date Limit");

		public static string ResponseDateLimit => Res.GetString("E9DEA7A8-3F40-424F-A6D3-9948359D8832", "Response Date Limit");

		public static string DeclarationType => Res.GetString("52B8D2F0-C556-46A7-B1B7-C2B8D62768E9", "Declaration Type");

		public static string AdditionalDeclarationType => Res.GetString("617671D7-F306-49F0-9BCD-A0137A69B08A", "Additional Declaration Type");

		public static string AmendmentRequestCancellationReason => Res.GetString("39F489E5-9FAF-451F-AAA2-582F93F16DA2", "Amendment Request Cancellation Reason");

		public static string ApplicationReferenceID => Res.GetString("842BD1DB-CDA1-4C88-93E6-F7E5F9A42406", "Application Reference ID");

		public static string ApplicationDecisionCodeType => Res.GetString("A095EEC0-BCE9-4162-8724-408D02128D04", "Application Decision Code Type");

		public static string ApplicantEORINumber => Res.GetString("240E34A5-B38F-4851-8564-359F95D1EEB9", "Applicant EORI Number");

		public static string Sequence => Res.GetString("1CB5A1CC-42CF-4F93-8BC3-3950E07C6BEC", "Sequence");

		public static string DecisionTakingCustomsAuthority => Res.GetString("91E5975B-D264-40AC-BD7C-B1CBD2689E0B", "Decision Taking Customs Authority");

		public static string PreferredPaymentMethod => Res.GetString("E345F0C1-7580-4FA6-973F-9874AE85E3EB", "Preferred Payment Method");

		public static string ReferenceNumber => Res.GetString("CAF1B456-9313-4DC4-9E9C-ECD50DC8C3E1", "Reference Number");

		public static string SequenceNumber => Res.GetString("77B5E050-B566-40B2-9A07-BF53C4FB28A7", "Sequence Number");

		public static string AmendmentRejectionReason => Res.GetString("3054D7E6-9C6C-4C26-99B5-A4C992B11FE5", "Amendment Rejection Reason");

		public static string DepositRefundApplicationApproved => Res.GetString("C3E10557-BCB8-4AF6-A56C-626F11844733", "Deposit Refund Application Approved");

		public static string RefundApplicationAccepted => Res.GetString("3C6FC861-45F3-4DC7-90D2-005A6BC7F383", "Refund Application Accepted");

		public static string ReasonNotApproved => Res.GetString("053C11BE-1C9A-4817-BD4C-C49DD523F97D", "Reason Not Approved");

		public static string StatementOfTheDecisionTakingCustomsAuthority => Res.GetString("755A4269-5030-41B3-984C-2A8AB59B1A57", "Statement of the Decision Taking Customs Authority");

		public static string DescriptionOfGrounds => Res.GetString("7B9EE506-236A-4467-97D7-A798F41FE85A", "Description of Grounds");

		public static string TimeLimitForCompletionOfFormalities => Res.GetString("A241F5EF-168F-405F-9E44-6D9822EA9F1B", "Time limit for completion of formalities");

		public static string DateAndTimeOfPresentationOfTheGoods => Res.GetString("86BF2486-9A3F-4556-AFCB-AC714188C349", "Date and Time of Presentation of the Goods");

		public static string IM429MessageFriendlyName => Res.GetString("A6EEAE8F-37BD-4124-B71C-26E799497855", "IM429: Release Notification");

		public static string DateLimitOfResponse => Res.GetString("16DE9806-E268-48D0-A320-FC91E82F5E03", "Date Limit of Response");

		public static string CustomsOfficeOfPresentation => Res.GetString("430EDAF5-8234-47FB-BA02-DE1B3382B286", "Customs Office of Presentation");

		public static string SupervisingCustomsOffice => Res.GetString("7EF9B9DE-8605-4016-8F41-49938C1DE0F9", "Supervising Customs Office");

		public static string CustomsOfficeLodgement => Res.GetString("43EF6CF7-1667-48B3-9110-49DA0659603E", "Customs Office Lodgement");

		public static string ControlResultDate => Res.GetString("7E674F51-CE93-4ABA-A674-5E8DBB055F61", "Control Result Date");

		public static string ControlResultRemarks => Res.GetString("45356226-CB96-4B52-A80A-2A176EAEB6C8", "Control Result Remarks");

		public static string GeneralRemarks => Res.GetString("65DF1C46-E38B-430E-A9D6-4991CB69E9C6", "General Remarks");

		public static string GetIM099MessageInterpreterSummary(ZString jobNumber) => Res.GetString("2D4E03F8-4F98-4EED-8480-9C432AE87EAA", "A General Notification and Request Information (IM099) message has been received for Job {0}.", jobNumber);

		public static string GetIM416MessageInterpreterSummary(ZString jobNumber) => Res.GetString("1F93EA9D-B962-44C2-B213-E3BE7338AF23", "A Customs Declaration Rejection (IM416) message has been received for Job {0}.", jobNumber);

		public static string GetRD409MessageInterpreterSummary(ZString jobNumber, bool decision) => Res.GetString("B172E41F-C7EC-4A54-8E11-E644D08D4C54", "[RD409 – Deposit Refund Application Decision] has been received and linked to job {0}. Decision: Deposit Refund Application {1}.", jobNumber, GetDecisionResult(decision));

		public static string GetRD416MessageInterpreterSummary(ZString jobNumber) => Res.GetString("8B6ABDCB-781F-4DC2-A111-4300DD64F4CC", "A Deposit Refund Application Rejection (RD416) has been received for Job {0}.", jobNumber);

		public static string GetRF409MessageInterpreterSummary(ZString jobNumber, bool decision) => Res.GetString("97325EE4-98C8-4D3C-A832-8588396E6004", "Deposit Refund Application Decision (RF409) has been received and linked to job {0}. Decision: Refund Application {1}.", jobNumber, GetDecisionResult(decision));

		public static string GetRF416MessageInterpreterSummary(ZString jobNumber) => Res.GetString("5880E70E-EF29-4F01-BA63-28E1F93B5915", "A Refund Application Rejection (RF416) has been received for Job {0}.", jobNumber);

		public static string GetTS316MessageInterpreterSummary(ZString jobNumber) => Res.GetString("40F25302-4F05-46A3-A7EA-03AB5E7D36C7", "A TSD Rejection (TS316) message has been received for Job {0}.", jobNumber);

		public static string TS316MessageInterpreterMessageDetailsSummary => Res.GetString("347F736F-8C05-4953-9D39-A8F664EFCE39", "TS316 – Temporary Storage Declaration Rejection message");

		public static string DeclarationRejectionDate => Res.GetString("6D770217-5C36-40EC-8FEE-98559BCABF5A", "Declaration Rejection Date");

		public static string DeclarationRejectionReason => Res.GetString("283BD56A-3B16-4E3C-B349-1EAD4CD8906C", "Declaration Rejection Reason");

		public static string DecisionReason => Res.GetString("fe749d96-19cb-47ae-b44f-6b6afabca57d", "Decision Reason");

		public static string TS304MessageFriendlyName => Res.GetString("65083390-26C3-4216-8246-79F42ACA9612", "TS304: Temporary Storage Declaration Amendment Request Registration");

		public static string TS305MessageFriendlyName => Res.GetString("7FDCE280-E677-4C19-A3E1-86F4BFDDC647", "TS305: TSD Amendment Request Rejection");

		public static string TS309MessageFriendlyName => Res.GetString("5AE3ED6B-FCED-48BB-95F5-7E53ACB00BCE", "TS309: Temporary Storage Declaration Invalidation Decision");

		public static string TS315VMessageFriendlyName => Res.GetString("25FF6F57-7D36-4C04-8E17-696FCCBB41FE", "TS315V: [G4 | G4+G3 | Manifest] Declaration Registration");

		public static string TS316MessageFriendlyName => Res.GetString("D6483737-D435-4DFD-B0A2-47116AC58AA4", "TS316 - Temporary Storage Declaration Invalidation Decision");

		public static string TS328MessageFriendlyName => Res.GetString("FABF5563-71C3-4B71-94E9-8CC422300115", "TS328 - [G4 | G4+G3 | Manifest] Declaration Acceptance");

		public static string TS333MessageFriendlyName => Res.GetString("5077C8E2-F2A9-4120-B9E1-5DA960F4C91C", "TS333: Presentation Notification (G3) Rejection");

		public static string TS351MessageFriendlyName => Res.GetString("19344DEA-F14A-4CCA-8978-7F82E8698CE2", "TS351: [G4 | G4+G3 | Manifest] Refusal");

		public static string RD409MessageFriendlyName => Res.GetString("0450E57C-5288-4066-AD82-771084502230", "RD409: Deposit Refund Application Decision");

		public static string RD416MessageFriendlyName => Res.GetString("E5C41E59-6EBE-4391-947A-C52777E13ACB", "RD416: Deposit Refund Application Rejection");

		public static string RF409MessageFriendlyName => Res.GetString("9DBAF79F-FFAD-4575-B05D-EDC8C9470558", "RF409: Refund Application Decision");

		public static string RF416MessageFriendlyName => Res.GetString("65D8DDB7-BEBA-4C35-AF47-5B6F4254B12C", "RF416: Refund Application Rejection Message");

		public static string BALMessageFriendlyName => Res.GetString("81F00B4E-511F-4552-BD1E-23C17CBCD8A8", "Balance");

		public static string DCTMessageFriendlyName => Res.GetString("9E41A37E-2692-4BB2-811C-EDA14DAF67B8", "Daily details – combined taxes report for Payers");

		public static string DSRMessageFriendlyName => Res.GetString("B03D5EB8-7C86-499D-B663-E575D1F0F60D", "Daily details – summary report for Payers");

		public static string DTTMessageFriendlyName => Res.GetString("C4190A91-05B7-47C8-A513-A23780E53708", "Daily details – tax type report for Payers");

		public static string PCIMessageFriendlyName => Res.GetString("9FEDECEC-5D1E-4109-B058-3B65B77D1660", "Period (monthly) details – combined taxes report for Importers");

		public static string PCTMessageFriendlyName => Res.GetString("61F9DCCB-6959-4F74-8E98-EA924203ED6D", "Period (monthly) details – combined taxes report for Payers");

		public static string PSRMessageFriendlyName => Res.GetString("B09F4127-E711-4A53-A78A-60459518496E", "Period (monthly) details – summary report for Payers");

		public static string PTTMessageFriendlyName => Res.GetString("422940EB-1B9E-48F7-B03B-1EB60207ACD9", "Period (monthly) details – tax type report for Payers");

		public static string UDRMessageFriendlyName => Res.GetString("E27C9DA9-CEE2-450D-A54A-FEA17D09F3D5", "Unpaid declarations report for Payers");

		public static string RosErrorMessageFriendlyName => Res.GetString("A8FE8C41-0BE4-4CE2-A35B-2099BE60A229", "ROS Error");

		static string GetDecisionResult(bool value) => value ? Res.GetString("103FCCC7-26AE-428D-A620-C0AD6D44E082", "Accepted") : Res.GetString("8A976D60-42FC-4112-A79E-09555A48F603", "Rejected");

		public static string EORI => Res.GetString("85EA83D7-40FE-450A-BFF9-5ECD209DFD4B", "EORI");

		public static string Period => Res.GetString("0D02CF62-C069-412D-BCE5-FA152D34E090", "Period");

		public static string DeclarationMsgType => Res.GetString("CC8B47DE-3C7C-41B8-9F61-735CDBC4D325", "Declaration Message Type");

		public static string Amendment => Res.GetString("770A7089-7C83-4475-A8F3-285EB0C72BCF", "Amendment");

		public static string Importer => Res.GetString("043CD479-16B5-4BCA-B885-2879CCB0E9B6", "Importer");

		public static string ImporterName => Res.GetString("113CC2FC-43C8-48DF-B9F2-48D3D6E35936", "Importer Name");

		public static string Declarant => Res.GetString("0281CAEF-0D51-42FC-A34A-38BF49F80C9A", "Declarant");

		public static string DeclarantName => Res.GetString("CBA0E3F0-FA0C-4D8D-A57E-AC2B587EEC43", "Declarant Name");

		public static string Payer => Res.GetString("1F04F136-78EC-473E-B6FA-C9BD0B5B3730", "Payer");

		public static string Recieved => Res.GetString("E16381F5-5CF1-4D79-85C4-B5731E087C2C", "Received");

		public static string TaxTotal => Res.GetString("507FC384-B855-49AA-B00D-2C33B12F8FCF", "Tax Total");

		public static string TotalDuty => Res.GetString("BE76D399-5720-4829-B84F-C9083CDC5991", "Total Duty");

		public static string Vat => Res.GetString("CD388838-7987-4C44-B882-147AC1CC7E54", "Value Added Tax (VAT)");

		public static string VatOnDuty => Res.GetString("E7B820C7-3D11-4D53-958E-F7F77B0B5E16", "Vat On Duty");

		public static string TotalExcise => Res.GetString("00702A52-4928-495A-A36A-4135E4A8171B", "Total Excise");

		public static string VatOnExcise => Res.GetString("5B1B73CC-CD37-4834-BB24-1F526D800027", "Vat On Excise");

		public static string PostponedVat => Res.GetString("CC0BB2FD-764E-47F0-882A-0EE497D5DD9E", "Postponed Vat");

		public static string UCR => Res.GetString("BB3BA661-20F5-4FDD-940B-170F9FE4B437", "UCR");

		public static string CommercialTransportDoc => Res.GetString("542F31AB-7B13-4C85-A317-2CACA96CCA50", "Commercial Transport Doc");

		public static string PaidOrders => Res.GetString("1B61DE85-8E17-4DB9-97E2-DAC8D964B804", "Paid Orders");

		public static string TaxBreakdowns => Res.GetString("71658E27-EC0F-4ADE-9FCD-1549274D3605", "Tax Breakdowns");

		public static string TaxType => Res.GetString("87DD89E0-8FAF-42F3-A289-032B582C1975", "Tax Type");

		public static string PayableAmount => Res.GetString("1433C6E0-E87A-43B8-925C-825DB153BB47", "Payable Amount");

		public static string Day => Res.GetString("FCD663B9-64E0-447A-81DB-D5216B3672BC", "Day");

		public static string TaxDetails => Res.GetString("49C8CD26-7D54-4595-9B49-ADE68F88B288", "Tax Details");

		public static string Version => Res.GetString("3922D4CF-C32E-4A2B-8077-5979639B4E1F", "Version");

		public static string _1D3 => Res.GetString("291EEC47-B4C3-4757-9016-41E9ABAE8012", "1DC");

		public static string _1A1 => Res.GetString("1B0E7704-360F-4C24-A317-5459202B7968", "1A1");

		public static string _1B2 => Res.GetString("49444320-ABDA-4A86-9A9A-0498F1B73DC1", "1B2");

		public static string _A00 => Res.GetString("241E53B6-64E1-4A25-8443-85FFEA5ABBD5", "A00");

		public static string _1B3 => Res.GetString("13A7E867-237C-45E4-AE90-59A52C5F891B", "1B3");

		public static string _1D5 => Res.GetString("3F654474-6A71-4AE4-8272-B92F0DB7BDAD", "1D5");

		public static string _A45 => Res.GetString("DE8D635C-5B83-4B3F-8369-355B991E12D1", "A45");

		public static string _B00 => Res.GetString("658F23D2-E094-42CE-9930-24FBA22141DE", "B00");

		public static string _1D6 => Res.GetString("0D5C98DF-C8CF-413F-91C8-80ADF29C99EF", "1D6");

		public static string _A35 => Res.GetString("C5AA451B-6292-4110-AB40-1CDF292797F2", "A35");

		public static string _B00EX => Res.GetString("5D646E8B-064F-487E-9B09-9A6EDFE87905", "B00EX");

		public static string _1S1 => Res.GetString("EAD8B274-8375-4C1B-A206-8DF3CD539009", "1S1");

		public static string _1E1 => Res.GetString("141FA4F2-EEDE-4A76-B051-971378BDFEB7", "1E1");

		public static string _A40 => Res.GetString("1751B77C-6B80-4BFC-BEE6-5DEB04277FF8", "A40");

		public static string _A30 => Res.GetString("DB309ADF-BD51-46EA-9010-51FFD153D9A2", "A30");

		public static string _1C1 => Res.GetString("560F42A0-66D8-49B7-8FB6-3B99651EFD03", "1C1");

		public static string _2E2 => Res.GetString("30F09A24-0F7B-4F3F-A24B-8D07B3BB1C7F", "2E2");

		public static string _A20 => Res.GetString("2A2608A0-2462-46AB-8F04-E1897FA36E68", "A20");
	}
}
