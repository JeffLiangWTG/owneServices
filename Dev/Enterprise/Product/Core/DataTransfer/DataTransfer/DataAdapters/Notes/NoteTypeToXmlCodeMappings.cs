using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using Type = Enterprise.DataTransfer.Xml.XsdVersion1.NotesNoteNoteType;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class NoteTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		NoteTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description, nameof(Type.AccountsPayableAccountManagementNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description, nameof(Type.AccountsReceivableAccountManagementNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote.Description, nameof(Type.AccountsReceivableCreditManagementNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.AllocationLog.Description, nameof(Type.AllocationLog));
			yield return new Mapping(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description, nameof(Type.AutoRatingAuditLog));
			yield return new Mapping(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description, nameof(Type.AWBRatelineOvertypedNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.BookingNotes.Description, nameof(Type.BookingNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.CarrierBookingRequest.Description, nameof(Type.CarrierBookingRequest));
			yield return new Mapping(PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, nameof(Type.CertificateOfOriginNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, nameof(Type.ClientVisibleJobNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.ContainerReleaseNote.Description, nameof(Type.ContainerReleaseNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.CustomsDeliveryInstructions.Description, nameof(Type.CustomsDeliveryInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description, nameof(Type.CustomsMessageRemarks));
			yield return new Mapping(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, nameof(Type.DangerousGoodsAdditionalHandlingInformation));
			yield return new Mapping(PredefinedNoteTypes.Instance.DeliveryInstructions.Description, nameof(Type.DeliveryInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, nameof(Type.DeliveryInstructionsNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description, nameof(Type.DeliveryOrderReceiptNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.UnloadLoadNotes.Description, nameof(Type.UnloadLoadNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.CIN750MessageNotes.Description, nameof(Type.CIN750MessageNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.CRESAMessageNotes.Description, nameof(Type.CRESAMessageNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, nameof(Type.DetailedGoodsDescription));
			yield return new Mapping(PredefinedNoteTypes.Instance.ExportCustomsHandlingNotes.Description, nameof(Type.ExportCustomsHandlineNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks.Description, nameof(Type.ExportReceivalAdviceRemarks));
			yield return new Mapping(PredefinedNoteTypes.Instance.ExtraOrderDetails.Description, nameof(Type.ExtraOrderDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, nameof(Type.ExtendedCommercialDescription));
			yield return new Mapping(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.Description, nameof(Type.FaxEmailTransmissionLog));
			yield return new Mapping(PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, nameof(Type.ForwardingInstructionNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.FullJobRoleDescription.Description, nameof(Type.FullJobRoleDescription));
			yield return new Mapping(PredefinedNoteTypes.Instance.GatePassNotes.Description, nameof(Type.GatePassNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.HandlingInstructions.Description, nameof(Type.HandlingInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description, nameof(Type.ImportCustomsHandlingNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.InactiveRecordDetails.Description, nameof(Type.InactiveRecordDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.InternalWorkNotes.Description, nameof(Type.InternalWorkNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.InvoiceDetails.Description, nameof(Type.InvoiceDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.InvoicingPreferences.Description, nameof(Type.InvoicingPreferences));
			yield return new Mapping(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote.Description, nameof(Type.ReceiveConfirmationInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.IssueResolutionNotes.Description, nameof(Type.IssueResolutionNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.IssueWorkingNotes.Description, nameof(Type.IssueWorkingNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.LoadListInstructions.Description, nameof(Type.LoadListInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, nameof(Type.ManifestGoodsDescription));
			yield return new Mapping(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, nameof(Type.MarksAndNumbers));
			yield return new Mapping(PredefinedNoteTypes.Instance.MessageInterpretation.Description, nameof(Type.MessageInterpretation));
			yield return new Mapping(PredefinedNoteTypes.Instance.OrderManagementNote.Description, nameof(Type.OrderManagementNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.OrderManagementUpdate.Description, nameof(Type.OrderManagementUpdate));
			yield return new Mapping(PredefinedNoteTypes.Instance.OrderUpdateHistory.Description, nameof(Type.OrderUpdateHistory));
			yield return new Mapping(PredefinedNoteTypes.Instance.OutturnNotes.Description, nameof(Type.OutturnNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.PaymentHandlingInstructions.Description, nameof(Type.PaymentHandlingInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.PickingInstructions.Description, nameof(Type.PickingInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.PickupInstructions.Description, nameof(Type.PickupInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, nameof(Type.PickupInstructionsNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, nameof(Type.PrealertArrivalNoticeRemarks));
			yield return new Mapping(PredefinedNoteTypes.Instance.QuoteCoverPageText.Description, nameof(Type.QuoteCoverPageText));
			yield return new Mapping(PredefinedNoteTypes.Instance.SpecialInstructions.Description, nameof(Type.SpecialInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.SurveyInstruction.Description, nameof(Type.SurveyInstructions));
			yield return new Mapping(PredefinedNoteTypes.Instance.TranshipmentNotes.Description, nameof(Type.TranshipmentNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, nameof(Type.UnmatchedOrgDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.WebUserNote.Description, nameof(Type.WebUserNote));
			yield return new Mapping(PredefinedNoteTypes.Instance.TradeLaneChargeInformation.Description, nameof(Type.TradeLaneChargeInformation));
			yield return new Mapping(PredefinedNoteTypes.Instance.CustomsInstructionNotes.Description, nameof(Type.CustomsInstructionNotes));
			yield return new Mapping(PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, nameof(Type.AdditionalSecurityInformation));
			yield return new Mapping(PredefinedNoteTypes.Instance.AdditionalBillClauses.Description, nameof(Type.AdditionalBillClauses));
			yield return new Mapping(PredefinedNoteTypes.Instance.CountryRules.Description, nameof(Type.CountryRules));
			yield return new Mapping(PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride.Description, nameof(Type.HouseBillGoodsDetailsOverride));
			yield return new Mapping(PredefinedNoteTypes.Instance.HouseBillChargesOverride.Description, nameof(Type.HouseBillChargesOverride));
			yield return new Mapping(PredefinedNoteTypes.Instance.HouseBillFollowOnOverride.Description, nameof(Type.HouseBillFollowOnOverride));
			yield return new Mapping(PredefinedNoteTypes.Instance.OtherThingsToReport.Description, nameof(Type.OtherThingsToReport));
			yield return new Mapping(PredefinedNoteTypes.Instance.SupplierBookingRejectReason.Description, nameof(Type.SupplierBookingRejectReason));
			yield return new Mapping(PredefinedNoteTypes.Instance.IncoTermsAgreedPlace.Description, nameof(Type.IncoTermsAgreedPlace));
			yield return new Mapping(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog.Description, nameof(Type.DeliveryDueDateCalculationLog));
			yield return new Mapping(PredefinedNoteTypes.Instance.BillOfDischargeDetails.Description, nameof(Type.BillOfDischargeDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.PeriodForDischargeDetails.Description, nameof(Type.PeriodForDischargeDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.DetailsOfPlannedActivities.Description, nameof(Type.DetailsOfPlannedActivities));
			yield return new Mapping(PredefinedNoteTypes.Instance.IdentificationofGoodsDetails.Description, nameof(Type.IdentificationofGoodsDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.ProcessedProductDescription.Description, nameof(Type.ProcessedProductDescription));
			yield return new Mapping(PredefinedNoteTypes.Instance.ProcessingProcedureDetails.Description, nameof(Type.ProcessingProcedureDetails));
			yield return new Mapping(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description, nameof(Type.EmissionsCalculationLog));
			yield return new Mapping(PredefinedNoteTypes.Instance.ReasonForCustoms.Description, nameof(Type.ReasonForCustoms));
			yield return new Mapping(PredefinedNoteTypes.Instance.MessageFromCustoms.Description, nameof(Type.MessageFromCustoms));
		}

		public static readonly NoteTypeToXmlCodeMappings Instance = new NoteTypeToXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Note Type"; }
		}

		public Xsd.NotesNoteNoteType GetEnumExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return base.GetEnumExternalCode(enterpriseCode, Xsd.NotesNoteNoteType.Custom, errorContext, notifications);
		}
	}
}
