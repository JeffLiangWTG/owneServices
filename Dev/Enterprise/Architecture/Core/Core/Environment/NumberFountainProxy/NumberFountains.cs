using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumberFountains
	{
		public NumberFountains(NumberFountain.NumberFountains inner)
		{
			this.inner = inner;
		}

		readonly NumberFountain.NumberFountains inner;

		public static long USMinimumInBondNumber { get { return Enterprise.NumberFountain.FountainUtils.MinNumber; } }
		public static long USMaximumInBondNumber { get { return 99999999; } }

		public static byte USInBondNumberSeedLength { get { return Enterprise.NumberFountain.NumberFountains.USInBondNumberSeedLength; } }

		public static string JobConsolFountainCFSPrefix { get { return Enterprise.NumberFountain.NumberFountains.JobConsolFountainCFSPrefix; } }
		public static string JobConsolFountainPrefix { get { return Enterprise.NumberFountain.NumberFountains.JobConsolFountainPrefix; } }

		public static string JobShipmentFountainCFSPrefix { get { return Enterprise.NumberFountain.NumberFountains.JobShipmentFountainCFSPrefix; } }
		public static string JobShipmentFountainPrefix { get { return Enterprise.NumberFountain.NumberFountains.JobShipmentFountainPrefix; } }

		public static string ECIManifestReferencePrefix { get { return Enterprise.NumberFountain.NumberFountains.ECIManifestReferencePrefix; } }
		public static string OldECIManifestReferencePrefix { get { return Enterprise.NumberFountain.NumberFountains.OldECIManifestReferencePrefix; } }

		public static string CustomsJobNumberFountainPrefix { get { return Enterprise.NumberFountain.NumberFountains.CustomsJobNumberFountainPrefix; } }

		public static string HVLVConsignmentIdPrefix => Enterprise.NumberFountain.NumberFountains.HVLVConsignmentIdPrefix;
		public static int HVLVConsignmentIdFormatDigits => Enterprise.NumberFountain.NumberFountains.HVLVConsignmentIdFormatDigits;

		public static string HVLVItemIdPrefix => Enterprise.NumberFountain.NumberFountains.HVLVItemIdPrefix;
		public static int HVLVItemIdFormatDigits => Enterprise.NumberFountain.NumberFountains.HVLVItemIdFormatDigits;

		public static string HVLVOuterPackageBarcodePrefix => Enterprise.NumberFountain.NumberFountains.HVLVOuterPackageBarcodePrefix;
		public static int HVLVOuterPackageBarcodeDigits => Enterprise.NumberFountain.NumberFountains.HVLVOuterPackageBarcodeDigits;

		public static string LimitedFiscalRepresentationNumberFountainPrefix { get { return Enterprise.NumberFountain.NumberFountains.LimitedFiscalRepresentationNumberFountainPrefix; } }

		public INumberFountainProxy OrgCodeNumberFountain
		{
			get { return inner.OrgCodeNumberFountain.Wrap(); }
		}

		public AccountingNumberFountainPooler EPaymentBeneficiaryRequestInternalRef => inner.EPaymentBeneficiaryRequestInternalRef.Wrap();

		public AccountingNumberFountainPooler EPaymentDealInternalReference => inner.EPaymentDealInternalReference.Wrap();

		public AccountingNumberFountainPooler AccBillingHeaderInternalReference => inner.AccBillingHeaderInternalReference.Wrap();

		public AccountingNumberFountainPooler APPaymentApprovalReference => inner.APPaymentApprovalReference.Wrap();

		public AccountingNumberFountainPooler ARPaymentApprovalReference => inner.ARPaymentApprovalReference.Wrap();

		public AccountingNumberFountainPooler APAdjustmentNoteInternalRef
		{
			get { return inner.APAdjustmentNoteInternalRef.Wrap(); }
		}

		public AccountingNumberFountainPooler APAdjustmentNoteNo
		{
			get { return inner.APAdjustmentNoteNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APCreditNoteInternalRef
		{
			get { return inner.APCreditNoteInternalRef.Wrap(); }
		}

		public AccountingNumberFountainPooler APCreditNoteNo
		{
			get { return inner.APCreditNoteNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APDiscountNo
		{
			get { return inner.APDiscountNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APExchangeDifferenceNo
		{
			get { return inner.APExchangeDifferenceNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APInvoiceInternalRef
		{
			get { return inner.APInvoiceInternalRef.Wrap(); }
		}

		public AccountingNumberFountainPooler APInvoiceNo
		{
			get { return inner.APInvoiceNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APJournalNo
		{
			get { return inner.APJournalNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APOverpaymentsNo
		{
			get { return inner.APOverpaymentsNo.Wrap(); }
		}

		public INumberFountainProxy APQueryClaimNo
		{
			get { return inner.APQueryClaimNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APTransferNo
		{
			get { return inner.APTransferNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARAdjustmentNoteNo
		{
			get { return inner.ARAdjustmentNoteNo.Wrap(); }
		}

		public INumberFountainProxy ArchiveVolumeNo
		{
			get { return inner.ArchiveVolumeNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARCreditNoteNo
		{
			get { return inner.ARCreditNoteNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARDiscountNo
		{
			get { return inner.ARDiscountNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARExchangeDifferenceNo
		{
			get { return inner.ARExchangeDifferenceNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARInvoiceNo
		{
			get { return inner.ARInvoiceNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARJournalNo
		{
			get { return inner.ARJournalNo.Wrap(); }
		}

		public AccountingNumberFountainPooler AROverpaymentsNo
		{
			get { return inner.AROverpaymentsNo.Wrap(); }
		}

		public AccountingNumberFountainPooler ARTransferNo
		{
			get { return inner.ARTransferNo.Wrap(); }
		}

		public INumberFountainProxy AUAirCargoJobNumber
		{
			get { return inner.AUAirCargoJobNumber.Wrap(); }
		}

		public INumberFountainProxy AUAirCargoPartShipment
		{
			get { return inner.AUAirCargoPartShipment.Wrap(); }
		}

		public INumberFountainProxy AUOneStopInterchangeNumber
		{
			get { return inner.AUOneStopInterchangeNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler BatchReceiptNo
		{
			get { return inner.BatchReceiptNo.Wrap(); }
		}

		public AccountingNumberFountainPooler EPaymentQuoteInternalRef
		{
			get { return inner.EPaymentQuoteInternalRef.Wrap(); }
		}

		public AccountingNumberFountainPooler ARCashAdvanceRequestReference
		{
			get { return inner.ARCashAdvanceRequestReference.Wrap(); }
		}

		public INumberFountainProxy CHLocalReferenceNumber(Guid companyPk)
		{
			return inner.CHLocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy CHDeclarationActivationJobNumber()
		{
			return inner.CHDeclarationActivationJobNumber().Wrap();
		}

		public INumberFountainProxy BELocalReferenceNumber(Guid companyPk)
		{
			return inner.BELocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy NLLocalReferenceNumber(Guid companyPk)
		{
			return inner.NLLocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy EULocalReferenceNumber(Guid companyPk)
		{
			return inner.EULocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy PNTSLocalReferenceNumber(Guid companyPk)
		{
			return inner.PNTSLocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy G3LocalReferenceNumber(Guid companyPk)
		{
			return inner.G3LocalReferenceNumber(companyPk).Wrap();
		}

		public INumberFountainProxy NODecReferenceNumber(string date) => inner.NODecReferenceNumber(date).Wrap();

		public INumberFountainProxy BGMNo
		{
			get { return inner.BGMNo.Wrap(); }
		}

		public INumberFountainProxy NACCSInputReference
		{
			get { return inner.NACCSInputReference.Wrap(); }
		}

		public INumberFountainProxy CAMasterBilleManifest
		{
			get { return inner.CAMasterBilleManifest.Wrap(); }
		}

		public INumberFountainProxy CAHouseBilleManifest
		{
			get { return inner.CAHouseBilleManifest.Wrap(); }
		}

		public INumberFountainProxy CanadaCargoControlNumber(Guid companyPk)
		{
			return inner.CanadaCargoControlNumber(companyPk).Wrap();
		}

		public INumberFountainProxy CarrierVoyageTransactionId
		{
			get { return inner.CarrierVoyageTransactionId.Wrap(); }
		}

		public INumberFountainProxy CartageLegNotificationSequence
		{
			get { return inner.CartageLegNotificationSequence.Wrap(); }
		}

		public INumberFountainProxy CertificateExamCampaignID
		{
			get { return inner.CertificateExamCampaignID.Wrap(); }
		}

		public INumberFountainProxy AccreditationCertificateID
		{
			get { return inner.AccreditationCertificateID.Wrap(); }
		}

		public INumberFountainProxy CFSReference
		{
			get { return inner.CFSReference.Wrap(); }
		}

		public INumberFountainProxy CIMNumber
		{
			get { return inner.CIMNumber.Wrap(); }
		}

		public INumberFountainProxy CMDNumber
		{
			get { return inner.CMDNumber.Wrap(); }
		}

		public INumberFountainProxy CMRMessageNumberSequence
		{
			get { return inner.CMRMessageNumberSequence.Wrap(); }
		}

		public INumberFountainProxy CMSInterchangeNumber
		{
			get { return inner.CMSInterchangeNumber.Wrap(); }
		}

		public INumberFountainProxy CodecoNumber
		{
			get { return inner.CodecoNumber.Wrap(); }
		}

		public INumberFountainProxy CommissionApprovalRequestBatchNo
		{
			get { return inner.CommissionApprovalRequestBatchNo.Wrap(); }
		}

		public INumberFountainProxy CommunicationID
		{
			get { return inner.CommunicationID.Wrap(); }
		}

		public INumberFountainProxy ContainerLogicalNumber
		{
			get { return inner.ContainerLogicalNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler ContraNo
		{
			get { return inner.ContraNo.Wrap(); }
		}

		public INumberFountainProxy CTOCusHAWBNumber
		{
			get { return inner.CTOCusHAWBNumber.Wrap(); }
		}

		public INumberFountainProxy CusOutturnHeader
		{
			get { return inner.CusOutturnHeader.Wrap(); }
		}

		public INumberFountainProxy CusSCAHouseNumber
		{
			get { return inner.CusSCAHouseNumber.Wrap(); }
		}

		public INumberFountainProxy CusSeaManArrivalPortNumber
		{
			get { return inner.CusSeaManArrivalPortNumber.Wrap(); }
		}

		public INumberFountainProxy CusSeaManOBLHeaderNumber
		{
			get { return inner.CusSeaManOBLHeaderNumber.Wrap(); }
		}

		public INumberFountainProxy AUPartShipConRef
		{
			get { return inner.AUPartShipConRef.Wrap(); }
		}

		public INumberFountainProxy CusSeaManTranHeaderNumber
		{
			get { return inner.CusSeaManTranHeaderNumber.Wrap(); }
		}

		public INumberFountainProxy CustomsJobNo
		{
			get { return inner.CustomsJobNo.Wrap(); }
		}

		public INumberFountainProxy CustomsJobNoByExternalAgent
		{
			get { return inner.CustomsJobNoByExternalAgent.Wrap(); }
		}

		public INumberFountainProxy CusUnderbondNumberFountain
		{
			get { return inner.CusUnderbondNumberFountain.Wrap(); }
		}

		public INumberFountainProxy CusMAWBMessageReferenceNumberFountain
		{
			get { return inner.CusMAWBMessageReferenceNumberFountain.Wrap(); }
		}

		public AccountingNumberFountainPooler DDRBatchNo
		{
			get { return inner.DDRBatchNo.Wrap(); }
		}

		public INumberFountainProxy DescartesMessageNumberFountain
		{
			get { return inner.DescartesMessageNumberFountain.Wrap(); }
		}

		public INumberFountainProxy DetentionInvoiceNumbers(Guid companyPk)
		{
			return inner.DetentionInvoiceNumbers(companyPk).Wrap();
		}

		public AccountingNumberFountainPooler DirectPaymentNo
		{
			get { return inner.DirectPaymentNo.Wrap(); }
		}

		public AccountingNumberFountainPooler DirectReceiptNo
		{
			get { return inner.DirectReceiptNo.Wrap(); }
		}

		public INumberFountainProxy ECIWriteOffManifestReference
		{
			get { return inner.ECIWriteOffManifestReference.Wrap(); }
		}

		public INumberFountainProxy EDIFACTNumberFountain(string messageInterchange, string sender, string receiver)
		{
			return inner.EDIFACTNumberFountain(messageInterchange, sender, receiver).Wrap();
		}

		public INumberFountainProxy ZACustomsEDIFACTNumberFountain(string messageInterchange, string receiver)
		{
			return inner.ZACustomsEDIFACTNumberFountain(messageInterchange, receiver).Wrap();
		}

		public INumberFountainProxy ESCustomsEDIFACTNumberFountain(string messageInterchange, string receiver) => inner.ESCustomsEDIFACTNumberFountain(messageInterchange, receiver).Wrap();

		public INumberFountainProxy KREntryNumberFountain(string messageInterchange, string sender, string year, long maxValue)
		{
			return inner.KREntryNumberFountain(messageInterchange, sender, year, maxValue, false).Wrap();
		}
		public INumberFountainProxy KRNumberFountain(string seed, long maxValue)
		{
			return inner.KRNumberFountain(seed, maxValue, true).Wrap();
		}

		public INumberFountainProxy ESBGMReference(string year, string enterpriseCode, string serverCode) => inner.ESBGMReference(year, enterpriseCode, serverCode).Wrap();

		public INumberFountainProxy ESBGMLocalReferenceSuffix(string localReference) => inner.ESBGMLocalReferenceSuffix(localReference).Wrap();

		public INumberFountainProxy GetCNCustomsLocalReferenceNumber(string fountainKey)
		{
			return inner.GetCNCustomsLocalReferenceNumber(fountainKey).Wrap();
		}

		public INumberFountainProxy ENettMessageNo(Guid companyPk)
		{
			return inner.ENettMessageNo(companyPk).Wrap();
		}

		public INumberFountainProxy EnquiryID
		{
			get { return inner.EnquiryID.Wrap(); }
		}

		public INumberFountainProxy ErrorReporting
		{
			get { return inner.ErrorReporting.Wrap(); }
		}

		public INumberFountainProxy EUH7ManifestJobReference => inner.EUH7ManifestJobReference.Wrap();

		public AccountingNumberFountainPooler ExchangeDifferenceNo
		{
			get { return inner.ExchangeDifferenceNo.Wrap(); }
		}

		public INumberFountainProxy ExpressECIWriteOffReference
		{
			get { return inner.ExpressECIWriteOffReference.Wrap(); }
		}

		public AccountingNumberFountainPooler FinancialTransactionsExportBatchNo
		{
			get { return inner.FinancialTransactionsExportBatchNo.Wrap(); }
		}

		public INumberFountainProxy GateBookingNumber
		{
			get { return inner.GateBookingNumber.Wrap(); }
		}

		public INumberFountainProxy GteGateActionNumber
		{
			get { return inner.GteGateActionNumber.Wrap(); }
		}

		public INumberFountainProxy GteMovementBookingNumber
		{
			get { return inner.GteMovementBookingNumber.Wrap(); }
		}

		public INumberFountainProxy GeneralLedgerConsolidationBatchNo
		{
			get { return inner.GeneralLedgerConsolidationBatchNo.Wrap(); }
		}

		public INumberFountainProxy AccountingExportWebServiceBatchNo(Guid companyPK)
		{
			return inner.AccountingExportWebServiceBatchNo(companyPK).Wrap();
		}

		public AccountingNumberFountainPooler GenExportBatchSequenceBatchNo
		{
			get { return inner.GenExportBatchSequenceBatchNo.Wrap(); }
		}

		public AccountingNumberFountainPooler AccCollectionBatchNo
		{
			get { return inner.AccCollectionBatchNo.Wrap(); }
		}

		public INumberFountainProxy GetLocalJobRefNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetLocalJobRefNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetDtbTransportGeneratorFountain(string fountainKey)
		{
			return inner.GetDtbTransportGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetAccountingNumberGeneratorFountain(string fountainKey, Guid companyPK)
		{
			return inner.GetAccountingNumberGeneratorFountain(fountainKey, companyPK).Wrap();
		}

		public INumberFountainProxy GetAgencyGeneratorFountain(string fountainKey)
		{
			return inner.GetAgencyGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetCarrierShipmentReferenceGeneratorFountain(string fountainKey) => inner.GetCarrierShipmentReferenceGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy GetShippingInstructionReferenceGeneratorFountain(string fountainKey) => inner.GetShippingInstructionReferenceGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy GetCAEntryNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetCAEntryNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetCustomsJobNoGeneratorFountain(string fountainKey)
		{
			return inner.GetCustomsJobNoGeneratorFountain(fountainKey).Wrap();
		}
		public INumberFountainProxy GetCustomsJobNoByExternalAgent(string fountainKey)
		{
			return inner.GetCustomsJobNoByExternalAgent(fountainKey).Wrap();
		}

		public INumberFountainProxy GetForwardingGeneratorFountain(string fountainKey)
		{
			return inner.GetForwardingGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetForwardingConsolGeneratorFountain(string fountainKey)
		{
			return inner.GetForwardingConsolGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetJobSupplierBookingNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetJobSupplierBookingNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetImporterSecurityFilingReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetImporterSecurityFilingReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetStorageNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetStorageNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetSundryChargesGeneratorFountain(string fountainKey, Guid companyPk)
		{
			return inner.GetSundryChargesGeneratorFountain(fountainKey, companyPk).Wrap();
		}

		public INumberFountainProxy GetPackageIDGeneratorFountain(string fountainKey)
		{
			return inner.GetPackageIDGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetPackageIDGeneratorFountainWithPackingParent(string fountainKey, Guid packingParentPK)
		{
			return inner.GetPackageIDGeneratorFountainWithPackingParent(fountainKey, packingParentPK).Wrap();
		}

		public INumberFountainProxy GetDocketIDGeneratorFountain(string fountainKey)
		{
			return inner.GetDocketIDGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GLJournalApprovalRequestReferenceID
		{
			get { return inner.GLJournalApprovalRequestReferenceID.Wrap(); }
		}

		public INumberFountainProxy SSCCBarCode(string ssccPrefix)
		{
			return inner.SSCCBarCodeNumber(ssccPrefix).Wrap();
		}

		public AccountingNumberFountainPooler GLJournal
		{
			get { return inner.GLJournal.Wrap(); }
		}

		public INumberFountainProxy ImporterJobReference(Guid importerGuid)
		{
			return inner.ImporterJobReference(importerGuid).Wrap();
		}

		public INumberFountainProxy ImporterSecurityFilingReference
		{
			get { return inner.ImporterSecurityFilingReference.Wrap(); }
		}

		public INumberFountainProxy IncidentApprovalClientRef
		{
			get { return inner.IncidentApprovalClientRef.Wrap(); }
		}

		public INumberFountainProxy CustomerServiceIncidentNo
		{
			get { return inner.CustomerServiceIncidentNo.Wrap(); }
		}

		public INumberFountainProxy InterimNo
		{
			get { return inner.InterimNo.Wrap(); }
		}

		public INumberFountainProxy LimitedFiscalRepresentationNo
		{
			get { return inner.LimitedFiscalRepresentationNo.Wrap(); }
		}

		public AccountingNumberFountainPooler InvoiceBatchNo
		{
			get { return inner.InvoiceBatchNo.Wrap(); }
		}

		public AccountingNumberFountainPooler JCJournalNo
		{
			get { return inner.JCJournalNo.Wrap(); }
		}

		public AccountingNumberFountainPooler APInvoiceApproval
		{
			get { return inner.APInvoiceApproval.Wrap(); }
		}

		public AccountingNumberFountainPooler CreditControlApproval
		{
			get { return inner.CreditControlApproval.Wrap(); }
		}

		public AccountingNumberFountainPooler EXVBuyerControlNumber
		{
			get { return inner.EXVBuyerControlNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler EXVSellerControlNumber
		{
			get { return inner.EXVSellerControlNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler JRJournalNo
		{
			get { return inner.JRJournalNo.Wrap(); }
		}

		public AccountingNumberFountainPooler PaymentBatchNo
		{
			get { return inner.PaymentBatchNo.Wrap(); }
		}

		public INumberFountainProxy JobCartageNumber
		{
			get { return inner.JobCartageNumber.Wrap(); }
		}

		public INumberFountainProxy JobCartageRunSheetNumber
		{
			get { return inner.JobCartageRunSheetNumber.Wrap(); }
		}

		public INumberFountainProxy JobConsolidatedTransportBookingNumber
		{
			get { return inner.JobConsolidatedTransportBookingNumber.Wrap(); }
		}

		public INumberFountainProxy JobConsolNumber
		{
			get { return inner.JobConsolNumber.Wrap(); }
		}

		public INumberFountainProxy JobConsolNumberCFS
		{
			get { return inner.JobConsolNumberCFS.Wrap(); }
		}

		public INumberFountainProxy JobContainerJobID
		{
			get { return inner.JobContainerJobID.Wrap(); }
		}

		public INumberFountainProxy JobNumber
		{
			get { return inner.JobNumber.Wrap(); }
		}

		public INumberFountainProxy JobServiceId
		{
			get { return inner.JobServiceId.Wrap(); }
		}

		public INumberFountainProxy JobPackLineId(string globalPrefix, int maxTotalLength)
		{
			return inner.JobPackLineID(globalPrefix, maxTotalLength).Wrap();
		}

		public INumberFountainProxy JobComInvoiceLineMatchingKey(string globalPrefix, int maxTotalLength)
		{
			return inner.JobComInvoiceLineMatchingKey(globalPrefix, maxTotalLength).Wrap();
		}

		public INumberFountainProxy JobSupplierBookingLineID
		{
			get { return inner.JobSupplierBookingLineID.Wrap(); }
		}

		public INumberFountainProxy ExternalRequestID
		{
			get { return inner.ExternalRequestID.Wrap(); }
		}

		public INumberFountainProxy JobShipmentNumber
		{
			get { return inner.JobShipmentNumber.Wrap(); }
		}

		public INumberFountainProxy JobShipmentNumberAgency
		{
			get { return inner.JobShipmentNumberAgency.Wrap(); }
		}

		public INumberFountainProxy CarrierShipmentCargoID => inner.CarrierShipmentCargoID.Wrap();

		public INumberFountainProxy CarrierVoyagePortCallID => inner.CarrierVoyagePortCallID.Wrap();

		public INumberFountainProxy CarrierVoyageID => inner.CarrierVoyageID.Wrap();

		public INumberFountainProxy CarrierShipmentReference => inner.CarrierShipmentReference.Wrap();

		public INumberFountainProxy ShippingInstructionReference => inner.ShippingInstructionReference.Wrap();

		public INumberFountainProxy JobShipmentNumberCFS
		{
			get { return inner.JobShipmentNumberCFS.Wrap(); }
		}

		public INumberFountainProxy JobSupplierBookingNumber
		{
			get { return inner.JobSupplierBookingNumber.Wrap(); }
		}

		public INumberFountainProxy JobVoyageNumber(int maxTotalLength) => inner.JobVoyageNumber(maxTotalLength).Wrap();

		public INumberFountainProxy ManifestJobLineNo
		{
			get { return inner.ManifestJobLineNo.Wrap(); }
		}

		public INumberFountainProxy ManifestJobNo
		{
			get { return inner.ManifestJobNo.Wrap(); }
		}

		public AccountingNumberFountainPooler MatchNo
		{
			get { return inner.MatchNo.Wrap(); }
		}

		public INumberFountainProxy NZCustomsInterchangeNumber
		{
			get { return inner.NZCustomsInterchangeNumber.Wrap(); }
		}

		public INumberFountainProxy NZMessageReferenceNumber
		{
			get { return inner.NZMessageReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy NZTSWSenderReferenceNumberFountain(string entryTypeCode)
		{
			return inner.NZTSWSenderReferenceNumberFountain(entryTypeCode).Wrap();
		}

		public INumberFountainProxy NZOCRReferenceNumber
		{
			get { return inner.NZOCRReferenceNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler OpeningPaymentNo
		{
			get { return inner.OpeningPaymentNo.Wrap(); }
		}

		public AccountingNumberFountainPooler OpeningReceiptNo
		{
			get { return inner.OpeningReceiptNo.Wrap(); }
		}

		public INumberFountainProxy OrderNumber
		{
			get { return inner.OrderNumber.Wrap(); }
		}

		public INumberFountainProxy PayableOrderNumber(Guid companyPk)
		{
			return inner.PayableOrderNumber(companyPk).Wrap();
		}

		public INumberFountainProxy PackageID
		{
			get { return inner.PackageID.Wrap(); }
		}

		public INumberFountainProxy PackingID
		{
			get { return inner.PackingID.Wrap(); }
		}

		public INumberFountainProxy PackingListID
		{
			get { return inner.PackingListID.Wrap(); }
		}

		public AccountingNumberFountainPooler Payment
		{
			get { return inner.Payment.Wrap(); }
		}

		public AccountingNumberFountainPooler PaymentMatchNo
		{
			get { return inner.PaymentMatchNo.Wrap(); }
		}

		public AccountingNumberFountainPooler PositivePayFileExportBatchNumber
		{
			get { return inner.PositivePayFileExportBatchNumber.Wrap(); }
		}

		public INumberFountainProxy ProcessID
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.ProcessID.Wrap();
			}
		}

		public INumberFountainProxy ProcessTaskID
		{
			get { return inner.ProcessTaskID.Wrap(); }
		}

		public INumberFountainProxy AnalyzerRunJobID
		{
			get { return inner.AnalyzerRunJobID.Wrap(); }
		}

		public INumberFountainProxy ProjectNo
		{
			get { return inner.ProjectNo.Wrap(); }
		}

		public INumberFountainProxy CustomerServiceTicketNumber
		{
			get { return inner.CustomerServiceTicketNumber.Wrap(); }
		}

		public INumberFountainProxy QueryClaimNo
		{
			get { return inner.QueryClaimNo.Wrap(); }
		}

		public INumberFountainProxy QuoteCustomisedNumber(string branchCode)
		{
			return inner.QuoteCustomisedNumber(branchCode).Wrap();
		}

		public INumberFountainProxy QuoteNumber
		{
			get { return inner.QuoteNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler Receipt
		{
			get { return inner.Receipt.Wrap(); }
		}

		public INumberFountainProxy RepairEstimateNo
		{
			get { return inner.RepairEstimateNo.Wrap(); }
		}

		public INumberFountainProxy SalesOpportunityID
		{
			get { return inner.SalesOpportunityID.Wrap(); }
		}

		public INumberFountainProxy CrmOpportunityID
		{
			get { return inner.CrmOpportunityID.Wrap(); }
		}

		public INumberFountainProxy GlbCompanyCampaignID
		{
			get { return inner.GlbCompanyCampaignID.Wrap(); }
		}

		public INumberFountainProxy SeaCargoMessageNumberFountain
		{
			get { return inner.SeaCargoMessageNumberFountain.Wrap(); }
		}

		public AccountingNumberFountainPooler SelfBillingInvoiceNo
		{
			get { return inner.SelfBillingInvoiceNo.Wrap(); }
		}

		public INumberFountainProxy SGCustomsInterchangeNumber
		{
			get { return inner.SGCustomsInterchangeNumber.Wrap(); }
		}

		public INumberFountainProxy SGMessageNumberSequence
		{
			get { return inner.SGMessageNumberSequence.Wrap(); }
		}

		public INumberFountainProxy ShipmentPreplanningNumbers
		{
			get { return inner.ShipmentPreplanningNumbers.Wrap(); }
		}

		public INumberFountainProxy StorageNumber
		{
			get { return inner.StorageNumber.Wrap(); }
		}

		public INumberFountainProxy SundryChargesNumber
		{
			get { return inner.SundryChargesNumber.Wrap(); }
		}

		public INumberFountainProxy SupplierBookingNumber
		{
			get { return inner.SupplierBookingNumber.Wrap(); }
		}

		public AccountingNumberFountainPooler TransferNo
		{
			get { return inner.TransferNo.Wrap(); }
		}

		#region Transit Warehouse

		public INumberFountainProxy TransitWarehouseReceiveID
		{
			get { return inner.TransitWarehouseReceiveID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseDispatchID
		{
			get { return inner.TransitWarehouseDispatchID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehousePackageID
		{
			get { return inner.TransitWarehousePackageID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseTransferID
		{
			get { return inner.TransitWarehouseTransferID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseReceiveExpectedPackingID
		{
			get { return inner.TransitWarehouseReceiveExpectedPackingID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseDispatchPackingGroupReferenceNumber
		{
			get { return inner.TransitWarehouseDispatchPackingGroupReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseDispatchLoadListReferenceNumber
		{
			get { return inner.TransitWarehouseDispatchLoadListReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseReceiveConsignmentID
		{
			get { return inner.TransitWarehouseReceiveConsignmentID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseDispatchConsignmentID
		{
			get { return inner.TransitWarehouseDispatchConsignmentID.Wrap(); }
		}

		public INumberFountainProxy TransitWarehouseCycleCountID
		{
			get { return inner.TransitWarehouseCycleCountID.Wrap(); }
		}

		#endregion

		#region Container Yard

		public INumberFountainProxy CYDReceiveAdviceJobNumber
		{
			get { return inner.CYDReceiveAdviceJobNumber.Wrap(); }
		}

		public INumberFountainProxy CYDReleaseAdviceJobNumber
		{
			get { return inner.CYDReleaseAdviceJobNumber.Wrap(); }
		}

		public INumberFountainProxy CYDTransportationUnitID
		{
			get { return inner.CYDTransportationUnitID.Wrap(); }
		}

		public INumberFountainProxy MNRWorkOrderJobNumber
		{
			get { return inner.MNRWorkOrderJobNumber.Wrap(); }
		}

		public INumberFountainProxy CYDMovementHeaderJobNumber
		{
			get { return inner.CYDMovementHeaderJobNumber.Wrap(); }
		}
		public INumberFountainProxy CYDAdHocServiceOrderJobNumber
		{
			get { return inner.CYDAdHocServiceOrderJobNumber.Wrap(); }
		}

		public INumberFountainProxy CYDDeliveryID
		{
			get { return inner.CYDDeliveryID.Wrap(); }
		}

		public INumberFountainProxy CYDPickupID
		{
			get { return inner.CYDPickupID.Wrap(); }
		}

		#endregion

		#region Transport Booking

		public INumberFountainProxy DtbBookingConsolidationMultiJobID
		{
			get { return inner.DtbBookingConsolidationMultiJobID.Wrap(); }
		}

		public INumberFountainProxy DtbBookingConsolidationID
		{
			get { return inner.DtbBookingConsolidationID.Wrap(); }
		}

		public INumberFountainProxy DtbBookingID
		{
			get { return inner.DtbBookingID.Wrap(); }
		}

		public INumberFountainProxy DtbAgentBookingID
		{
			get { return inner.DtbAgentBookingID.Wrap(); }
		}

		#endregion

		#region Transport Consignment

		public INumberFountainProxy DtbConsignmentConsolidationID
		{
			get { return inner.DtbConsignmentConsolidationID.Wrap(); }
		}

		public INumberFountainProxy DtbConsignmentID
		{
			get { return inner.DtbConsignmentID.Wrap(); }
		}

		public INumberFountainProxy DtbConsignmentActionID
		{
			get { return inner.DtbConsignmentActionID.Wrap(); }
		}

		public INumberFountainProxy DtbConsignmentRunSheetID
		{
			get { return inner.DtbConsignmentRunSheetID.Wrap(); }
		}

		#endregion

		public INumberFountainProxy TransportInterchangeNumber
		{
			get { return inner.TransportInterchangeNumber.Wrap(); }
		}

		public INumberFountainProxy TransportJobMessageNo
		{
			get { return inner.TransportJobMessageNo.Wrap(); }
		}

		public INumberFountainProxy UAEDeliveryOrderNumber(Guid companyPk)
		{
			return inner.UAEDeliveryOrderNumber(companyPk).Wrap();
		}

		public INumberFountainProxy ManifestJobReference
		{
			get { return inner.ManifestJobReference.Wrap(); }
		}

		public INumberFountainProxy GetManifestJobReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetManifestJobReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy TSDJobReference
		{
			get { return inner.TSDJobReference.Wrap(); }
		}

		public INumberFountainProxy GetTSDJobReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetTSDJobReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy TRETradeJobReference
		{
			get { return inner.TRETradeJobReference.Wrap(); }
		}

		public INumberFountainProxy GetTREtradeJobReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetTRETradeJobReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy USAirAMSPackageTrackingID
		{
			get { return inner.USAirAMSPackageTrackingID.Wrap(); }
		}

		public INumberFountainProxy GetUSAirAMSPackageTrackingIDGeneratorFountain(string fountainKey)
		{
			return inner.GetUSAirAMSPackageTrackingIDGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GBCDSEntryLocalReferenceNumber
		{
			get { return inner.GBCDSEntryLocalReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy GetGBCDSEntryLocalReferenceNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetGBCDSEntryLocalReferenceNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy ZAOutturnGateInOutJobReference
		{
			get { return inner.ZAOutturnGateInOutJobReference.Wrap(); }
		}

		public INumberFountainProxy GetZAOutturnGateInOutJobReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetZAOutturnGateInOutJobReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy JPAFRJobReference
		{
			get { return inner.JPAFRJobReference.Wrap(); }
		}

		public INumberFountainProxy ITTempStorageJobReference
		{
			get { return inner.ITTempStorageJobReference.Wrap(); }
		}

		public INumberFountainProxy ITMessageLocalReferenceNumber(string year) => inner.ITMessageLocalReferenceNumberFountain(year).Wrap();

		public INumberFountainProxy NOCustomsWarehouseGoodsNumber(DateTime arrivalDate, string grantId)
		{
			return inner.NOCustomsWarehouseGoodsNumberFountain(arrivalDate, grantId).Wrap();
		}

		public INumberFountainProxy DETempStorageJobReference => inner.DETempStorageJobReference.Wrap();

		public INumberFountainProxy DEMessageControlNumber(Guid companyPk, string prefix, int formatDigits) => inner.DEMessageControlNumber(companyPk, prefix, formatDigits).Wrap();

		public INumberFountainProxy DEInterchangeControlReference => inner.DEInterchangeControlReference.Wrap();

		public INumberFountainProxy DEEMCSInterchangeControlReference => inner.DEEMCSInterchangeControlReference.Wrap();

		public INumberFountainProxy MonthlyClosingJobReference => inner.MonthlyClosingJobReference.Wrap();

		public INumberFountainProxy GetMonthlyClosingJobReferenceGeneratorFountain(string fountainKey) => inner.GetMonthlyClosingJobReferenceGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy FRTempStorageJobReference => inner.FRTempStorageJobReference.Wrap();

		public INumberFountainProxy IEMessageControlNumber(string prefix) => inner.GetIEMessageControlNumber(prefix).Wrap();

		public INumberFountainProxy GetIENumberFountain(string fountainKey) => inner.GetIENumberFountain(fountainKey).Wrap();

		public INumberFountainProxy EUMessageControlNumber(string prefix) => inner.EUMessageControlNumber(prefix).Wrap();

		public INumberFountainProxy TRMessageControlNumber(Guid companyPk) => inner.TRMessageControlNumber(companyPk).Wrap();

		public INumberFountainProxy NLMessageControlNumber(Guid companyPk) => inner.NLMessageControlNumber(companyPk).Wrap();

		public INumberFountainProxy ILMessageControlNumber(Guid companyPk) => inner.ILMessageControlNumber(companyPk).Wrap();

		public INumberFountainProxy ILGatePassMovementNumber => inner.ILGatePassMovementNumber.Wrap();

		public INumberFountainProxy ILDeliveryOrderNumber => inner.ILDeliveryOrderNumber.Wrap();

		public INumberFountainProxy PLMessageControlNumber(string applicationCode, string year2Digits, string optionalMessageCode5Symbols = null)
			=> inner.PLMessageControlNumber(applicationCode, year2Digits, optionalMessageCode5Symbols).Wrap();

		public INumberFountainProxy PLBGMReferenceNumber(string branch, string year) => inner.PLBGMReferenceNumber(branch, year).Wrap();

		public INumberFountainProxy ACAReferenceNumber(string badge, string importsOrExport)
		{
			return inner.ACAReferenceNumber(badge, importsOrExport).Wrap();
		}

		public INumberFountainProxy NctsLocalReferenceNumber
		{
			get { return inner.NctsLocalReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy GetNctsLocalReferenceNumberFountain(string fountainKey)
		{
			return inner.GetNctsLocalReferenceNumberFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy EUICS2LocalReferenceNumber(Guid companyOrBranchPK)
		{
			return inner.EUICS2LocalReferenceNumber(companyOrBranchPK).Wrap();
		}

		public INumberFountainProxy USInBondJobReference
		{
			get { return inner.USInBondJobReference.Wrap(); }
		}

		public INumberFountainProxy USAMSJobReference
		{
			get { return inner.USAMSJobReference.Wrap(); }
		}

		public INumberFountainProxy USeManifestTripReference
		{
			get { return inner.USeManifestTripReference.Wrap(); }
		}

		public INumberFountainProxy GetUSeManifestTripReferenceGeneratorFountain(string fountainKey)
		{
			return inner.GetUSeManifestTripReferenceGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy USInBondNumberFountain(Guid entryCompanyOrBranchPK)
		{
			return inner.USInBondNumberFountain(entryCompanyOrBranchPK).Wrap();
		}

		public INumberFountainProxy AMSManifestSequenceNumberFountain(string carrierCode)
		{
			return inner.AMSManifestSequenceNumberFountain(carrierCode).Wrap();
		}

		public INumberFountainProxy VoyageAccountingNumber(Guid companyPk)
		{
			return inner.VoyageAccountingNumber(companyPk).Wrap();
		}

		public INumberFountainProxy VoyageDestinationNumber(int maxTotalLength) => inner.VoyageDestinationNumber(maxTotalLength).Wrap();

		public INumberFountainProxy VoyageOriginNumber(int maxTotalLength) => inner.VoyageOriginNumber(maxTotalLength).Wrap();

		public INumberFountainProxy WarehouseDocketID
		{
			get { return inner.WarehouseDocketID.Wrap(); }
		}

		public INumberFountainProxy WarehouseInvoiceNumber
		{
			get { return inner.WarehouseInvoiceNumber.Wrap(); }
		}

		public INumberFountainProxy WarehouseLoadID => inner.WarehouseLoadID.Wrap();

		public INumberFountainProxy WhsCycleCountLocationID => inner.WhsCycleCountLocationID.Wrap();

		public INumberFountainProxy WarehousePickNo
		{
			get { return inner.WarehousePickNo.Wrap(); }
		}

		public INumberFountainProxy WarehouseStocktakeNumber
		{
			get { return inner.WarehouseStocktakeNumber.Wrap(); }
		}

		public INumberFountainProxy WarehouseVASOrderJobID
		{
			get { return inner.WarehouseVASOrderJobID.Wrap(); }
		}

		public AccountingNumberFountainPooler WIPAccrualsJournal
		{
			get { return inner.WIPAccrualsJournal.Wrap(); }
		}

		public INumberFountainProxy WorkItemNo
		{
			get { return inner.WorkItemNo.Wrap(); }
		}

		public INumberFountainProxy XmlEDIInterchangeNumber
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.XmlEDIInterchangeNumber.Wrap();
			}
		}

		public INumberFountainProxy XmlEDIMessageNumber
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.XmlEDIMessageNumber.Wrap();
			}
		}

		public INumberFountainProxy UniversalValidationRuleSetNumber
			=> inner.UniversalValidationRuleSetNumber.Wrap();

		public INumberFountainProxy UsageEDIMessageNumber
		{
			get { return inner.UsageEDIMessageNumber.Wrap(); }
		}

		public INumberFountainProxy DpsEDIMessageNumber
		{
			get { return inner.DpsEDIMessageNumber.Wrap(); }
		}

		public INumberFountainProxy StmEntityScreeningLogNumber
		{
			get { return inner.StmEntityScreeningLogNumber.Wrap(); }
		}

		public INumberFountainProxy HttpXmlEDIMessageNumber
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.HttpXmlEDIMessageNumber.Wrap();
			}
		}

		public INumberFountainProxy ZA_DA63Number
		{
			get { return inner.ZA_DA63Number.Wrap(); }
		}

		public INumberFountainProxy ZA_EntryHeaderNo
		{
			get { return inner.ZA_EntryHeaderNo.Wrap(); }
		}

		public INumberFountainProxy SystemEDIInterchangeNumber
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.SystemEDIInterchangeNumber.Wrap();
			}
		}

		public INumberFountainProxy SystemEDIMessageNumber
		{
			get
			{
				SetLossyFountainAccessed();
				return inner.SystemEDIMessageNumber.Wrap();
			}
		}

		public INumberFountainProxy TelematicsEDIMessageNumber
		{
			get { return inner.TelematicsEDIMessageNumber.Wrap(); }
		}

		public INumberFountainProxy TelematicsRimDataBatchNumber => inner.TelematicsRimDataBatchNumber.Wrap();

		public INumberFountainProxy TelematicsRimRegistrationNumber => inner.TelematicsRimRegistrationNumber.Wrap();

		public INumberFountainProxy TelematicsRimEnrolmentReportNumber => inner.TelematicsRimEnrolmentReportNumber.Wrap();

		public INumberFountainProxy LinehaulManifestNumber
		{
			get { return inner.LinehaulManifestNumber.Wrap(); }
		}

		public INumberFountainProxy PalletTransactionID
		{
			get { return inner.PalletTransactionID.Wrap(); }
		}

		public INumberFountainProxy SailingScheduleReferenceNumber(int maxTotalLength) => inner.SailingScheduleReferenceNumber(maxTotalLength).Wrap();

		public INumberFountainProxy TemplateRecordID
		{
			get { return inner.TemplateRecordID.Wrap(); }
		}

		public INumberFountainProxy HVLVConsignmentId
		{
			get { return inner.HVLVConsignmentId.Wrap(); }
		}

		public INumberFountainProxy HVLVItemId
		{
			get { return inner.HVLVItemId.Wrap(); }
		}

		public INumberFountainProxy HVLVOuterPackageBarcode
		{
			get { return inner.HVLVOuterPackageBarcode.Wrap(); }
		}

		public INumberFountainProxy HVLVOriginLoadListReference
		{
			get { return inner.HVLVOriginLoadListReference.Wrap(); }
		}

		#region Cluster Key

		public INumberFountainProxy ClusterKey => inner.ClusterKeyNumber.Wrap();

		#endregion

		#region HVLVBooking

		public INumberFountainProxy HVLVConsignmentClusterKey => inner.HVLVConsignmentClusterKey.Wrap();
		public INumberFountainProxy HVLVBookingHeaderJobNumber => inner.HVLVBookingHeaderJobNumber.Wrap();
		public INumberFountainProxy HVLVConsignmentHeaderJobNumber => inner.HVLVConsignmentHeaderJobNumber.Wrap();

		#endregion

		public INumberFountainProxy EMCSLocalReferenceNumber
		{
			get { return inner.EMCSLocalReferenceNumber.Wrap(); }
		}

		public INumberFountainProxy GetEMCSLocalReferenceNumberFountain(string fountainKey)
		{
			return inner.GetEMCSLocalReferenceNumberFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy ConsolidatedDeclarationNumber
		{
			get { return inner.ConsolidatedDeclarationNumber.Wrap(); }
		}

		public INumberFountainProxy GetConsolidatedDeclarationNumberFountain(string fountainKey)
		{
			return inner.GetConsolidatedDeclarationNumberFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy OrgARClientNumber(Guid companyPk)
		{
			return inner.OrgARClientNumber(companyPk).Wrap();
		}

		public INumberFountainProxy InvoiceTransactionReference(Guid companyPk)
		{
			return inner.InvoiceTransactionReference(companyPk).Wrap();
		}

		public INumberFountainProxy ComplianceDocumentInternalReference(string ledger, string transactionType, Guid companyPk)
		{
			return inner.ComplianceDocumentInternalReference(ledger, transactionType, companyPk).Wrap();
		}

		public AccountingNumberFountainPooler AccEInvoicingBatchNo => inner.AccEInvoicingBatchNo.Wrap();

		public AccountingNumberFountainPooler JobChargePostingQueueGroupId => inner.JobChargePostingQueueGroupId.Wrap();

		public INumberFountainProxy GetCINSendCounter()
		{
			return inner.CINSendCounter.Wrap();
		}

		public INumberFountainProxy GetTransactionIDSendCounter()
		{
			return inner.CorrelationIDCounter.Wrap();
		}
		public INumberFountainProxy GetCorrelationIDGeneratorFountain(string fountainKey)
		{
			return inner.GetCorrelationIDGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetNLNctsFallbackEntryNumberSendCounter()
		{
			return inner.NLNctsFallbackEntryNumber.Wrap();
		}

		public INumberFountainProxy GetNLNctsFallbackEntryNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetNLNctsFallbackEntryNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetFRCustomsFallbackEntryNumberSendCounter()
		{
			return inner.FRCustomsFallbackEntryNumber.Wrap();
		}
		public INumberFountainProxy GetFRCustomsFallbackEntryNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetFRCustomsFallbackEntryNumberGeneratorFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy FRStatementNumber => inner.FRStatementNumber.Wrap();

		public INumberFountainProxy GetFRStatementNumberGeneratorFountain(string fountainKey) => inner.GetFRStatementNumberGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy FRMessageBatchNumber => inner.FRMessageBatchNumber.Wrap();

		public INumberFountainProxy GetFRMessageBatchNumberGeneratorFountain(string fountainKey) => inner.GetFRMessageBatchNumberGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy GetIncomingTWCustomsMessageNumber()
		{
			return inner.IncomingTWCustomsMessageNumber.Wrap();
		}
		public INumberFountainProxy GetOutgoingTWCustomsMessageNumber()
		{
			return inner.OutgoingTWCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy GetTRSPTSJobReferenceNumber()
		{
			return inner.TRSPTSJobReferenceNumberFountain().Wrap();
		}

		public INumberFountainProxy GetTRCusStatementNumber(string prefix)
		{
			return inner.TRCusStatementNumberFountain(prefix).Wrap();
		}
		public INumberFountainProxy GetTRStampDutyLedgerNumber(string company, string year, long minValue)
		{
			return inner.TRStampDutyLedgerNumberFountain(company, year, minValue).Wrap();
		}

		public string GetKey(string generatorKey, string fountainKey)
		{
			return inner.GetKey(generatorKey, fountainKey);
		}

		public INumberFountainProxy GetTWCustomsInterchangeNumSequence(string messageOwner)
		{
			return inner.TWCustomsInterchangeNumSequence(messageOwner).Wrap();
		}

		public INumberFountainProxy GetTWFunctionalReferenceIDNumber()
		{
			return inner.GetTWFunctionalReferenceIDNumber().Wrap();
		}

		public INumberFountainProxy GetTWN5101HFunctionalReferenceIdNumberFountain()
		{
			return inner.GetTWN5101HFunctionalReferenceIdNumberFountain().Wrap();
		}

		public INumberFountainProxy GetTWLicensingMessageFunctionalReferenceID(string currentDate)
		{
			return inner.GetTWLicensingMessageFunctionalReferenceID(currentDate).Wrap();
		}

		public INumberFountainProxy GetTWSWLicensingMessageFunctionalReferenceID(string currentDate)
		{
			return inner.GetTWSWLicensingMessageFunctionalReferenceID(currentDate).Wrap();
		}

		public INumberFountainProxy JobApplicationNo
		{
			get { return inner.JobApplicationNo.Wrap(); }
		}

		public INumberFountainProxy GetTWJobReferenceNumberFountain(string company, string year)
		{
			return inner.TWJobReferenceNumberFountain(company, year).Wrap();
		}

		public INumberFountainProxy GetITCustomsMessageFilenameNumberFountain(string fountainKey, int minValue, int maxValue)
		{
			return inner.ITCustomsMessageFilenameNumberFountain(fountainKey, minValue, maxValue).Wrap();
		}

		public INumberFountainProxy GetINCustomsMessageNumberFountain(string fountainKey)
		{
			return inner.INCustomsMessageNumberFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetINLocalReferenceNumberFountain(string fountainKey)
		{
			return inner.INLocalReferenceNumberFountain(fountainKey).Wrap();
		}

		public INumberFountainProxy GetOutgoingUYCustomsMessageNumber()
		{
			return inner.OutgoingUYCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy GetOutgoingMXCustomsMessageNumber()
		{
			return inner.OutgoingMXCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy GetOutgoingCLCustomsMessageNumber()
		{
			return inner.OutgoingCLCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy GetOutgoingARCustomsMessageNumber()
		{
			return inner.OutgoingARCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy GetKoreaSouthIssueId()
		{
			return inner.KoreaSouthIssueId.Wrap();
		}

		public INumberFountainProxy GetBRLicenseMessageBatchNumber()
		{
			return inner.BRLicenseMessageBatchNumber.Wrap();
		}

		public INumberFountainProxy GetBRLicenseEntryNumber()
		{
			return inner.BRLicenseEntryNumber.Wrap();
		}

		public INumberFountainProxy GetEdiIdentityCertificateSequenceNumber()
		{
			return inner.EdiIdentityCertificateSequenceNumber.Wrap();
		}

		public INumberFountainProxy GetCHTraderDeclarationNumber(Guid companyPk, string prefix)
		{
			return inner.GetCHTraderDeclarationNumber(companyPk, prefix).Wrap();
		}

		public INumberFountainProxy GetProfitShareRedistributionNumber()
		{
			return inner.ProfitShareRedistributionNumber.Wrap();
		}

		public INumberFountainProxy GetOutgoingCOCustomsMessageNumber()
		{
			return inner.OutgoingCOCustomsMessageNumber.Wrap();
		}

		public INumberFountainProxy BECustomsRegistryNumberFountain(Guid branchPK, Guid organisationPK, string declarationType, ZDateTime startDate, int startValue)
		{
			return inner.BECustomsRegistryNumberFountain(branchPK, organisationPK, declarationType, startDate.ToBestReadableDateString(), startValue).Wrap();
		}

		public INumberFountainProxy CYDDeliveryHeaderJobNumber
		{
			get { return inner.CYDDeliveryHeaderJobNumber.Wrap(); }
		}

		public INumberFountainProxy CYDPickupHeaderJobNumber
		{
			get { return inner.CYDPickupHeaderJobNumber.Wrap(); }
		}

		public INumberFountainProxy BRCatalogCode => inner.BRCatalogCode.Wrap();

		public INumberFountainProxy GetBRCatalogCodeGeneratorFountain(string fountainKey) => inner.GetBRCatalogCodeGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy BRLPCOJobNumber => inner.BRLPCOJobNumber.Wrap();

		public INumberFountainProxy GetBRLPCOJobNumberGeneratorFountain(string fountainKey) => inner.GetBRLPCOJobNumberGeneratorFountain(fountainKey).Wrap();

		public INumberFountainProxy GetAECustomsNumberFountain(string fountainKey, int formatDigits, int startingNo = 1)
		{
			return inner.AECustomsNumberFountain(fountainKey, formatDigits, startingNo).Wrap();
		}

		void SetLossyFountainAccessed()
		{
#if DEBUG
			Core.Testing.FountainTestListener.SetLossyFountainAccess();
#endif
		}

		#region Exit Control Sequences
		public INumberFountainProxy ExitControlJobNumber => inner.ExitControlJobNumber.Wrap();

		public INumberFountainProxy GetExitControlJobNumberGeneratorFountain(string fountainKey)
		{
			return inner.GetExitControlJobNumberGeneratorFountain(fountainKey).Wrap();
		}
		#endregion
	}
}
