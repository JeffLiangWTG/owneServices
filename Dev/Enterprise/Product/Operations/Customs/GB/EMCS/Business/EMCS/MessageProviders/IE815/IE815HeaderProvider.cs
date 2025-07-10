using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE815HeaderProvider : HeaderProvider, IIE815Header
	{
		public IE815HeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }

		public string JourneyTime => string.Concat(emcsJobDeclaration.JourneyTimeFormatPart, emcsJobDeclaration.JourneyTimeNumericPart.ToString().PadLeft(2, '0'));

		public string TransportModeCode => new EU.EMCS.Business.TransportModeTranslator().TranslateToWCOCode(emcsJobDeclaration.JE_TransportMode);

		public string InvoiceNumber => emcsJobDeclaration.InvoiceNumber;

		public DateTime? InvoiceDate => emcsJobDeclaration.InvoiceDate.ToNullableDateTime();

		public string DestinationTypeCode => emcsJobDeclaration.JE_MessageSubType;

		public string GuarantorType => emcsJobDeclaration.ZG_GuarantorType;

		public IReadOnlyCollection<IEMCSPartyGuarantor> GuarantorTraders => guarantorTraders ?? (guarantorTraders = GetGuarantorTraders());
		IReadOnlyCollection<IEMCSPartyGuarantor> guarantorTraders;

		public string DispatchImportOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDispatch);

		public string CompetentAuthorityDispatchOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch);

		public IEMCSPartyConsignee ConsigneeTrader => CachedValueHelper.GetValue(ref consigneeTrader, () =>
		{
			IEMCSPartyConsignee result = null;
			if (emcsJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown && emcsJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
			{
				result = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress);
			}

			return result;
		});
		CachedValue<IEMCSPartyConsignee> consigneeTrader;

		public IEMCSPartyConsignor ConsignorTrader => consignorTrader ?? (consignorTrader = PartyConsignorProvider.NewOrNull(emcsJobDeclaration.SupplierDocumentaryAddress));
		IEMCSPartyConsignor consignorTrader;

		public IEMCSPartyPlaceOfDispatch PlaceOfDispatchTrader => CachedValueHelper.GetValue(ref placeOfDispatchTrader, () =>
		{
			IEMCSPartyPlaceOfDispatch result = null;
			if (emcsJobDeclaration.ZG_OriginType == EMCSOriginTypeList.Codes.TaxWarehouse)
			{
				result = PartyPlaceOfDispatchProvider.NewOrNull(GetJobDocAddressWithFallback(emcsJobDeclaration.DispatchWarehouseDocumentaryAddress, emcsJobDeclaration.SupplierDocumentaryAddress));
			}
			return result;
		});
		CachedValue<IEMCSPartyPlaceOfDispatch> placeOfDispatchTrader;

		public IEMCSPartyDeliveryPlace DeliveryPlaceTrader => CachedValueHelper.GetValue(ref deliveryPlaceTrader, () =>
		{
			IEMCSPartyDeliveryPlace result = null;
			var destinationTypeCode = emcsJobDeclaration.JE_MessageSubType;
			if (destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee
				&& destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationExport
				&& destinationTypeCode != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown)
			{
				result = PartyDeliveryPlaceProvider.NewOrNull(GetJobDocAddressWithFallback(emcsJobDeclaration.DestinationWarehouseDocumentaryAddress, emcsJobDeclaration.ImporterDocumentaryAddress));
			}
			return result;
		});
		CachedValue<IEMCSPartyDeliveryPlace> deliveryPlaceTrader;

		public IEMCSPartyTransporter TransportArrangerTrader => CachedValueHelper.GetValue(ref transportArrangerTrader, () =>
		{
			IEMCSPartyTransporter result = null;
			var transportArrangement = emcsJobDeclaration.ZG_TransportArrangement;
			if (transportArrangement != EMCSTransportArrangementList.Codes.Consignor
				&& transportArrangement != EMCSTransportArrangementList.Codes.Consignee)
			{
				result = PartyTransporterProvider.NewOrNull(emcsJobDeclaration.CarrierAgentDocumentaryAddress);
			}
			return result;
		});
		CachedValue<IEMCSPartyTransporter> transportArrangerTrader;

		public IEMCSPartyTransporter FirstTransporterTrader => CachedValueHelper.GetValue(ref firstTransporterTrader, () => PartyTransporterProvider.NewOrNull(emcsJobDeclaration.TransporterDocumentaryAddress));
		CachedValue<IEMCSPartyTransporter> firstTransporterTrader;

		public string ComplementConsigneeMemberStateCode => emcsJobDeclaration.ZG_CCTMSA;

		public string ComplementConsigneeSerialNumberOfCertificateOfExemption => emcsJobDeclaration.ZG_CertOfExemption;

		public string DeferredSubmissionFlag => emcsJobDeclaration.ZG_DeferredSubmission;

		public string SubmissionMessageType => emcsJobDeclaration.ZG_SubmissionType;

		public string TransportArrangement => emcsJobDeclaration.ZG_TransportArrangement;

		public DateTime? DispatchDateTime => emcsJobDeclaration.JE_DateAtOrigin.ToNullableDateTime();

		public string OriginType => emcsJobDeclaration.ZG_OriginType;

		public string LocalReferenceNumber => emcsJobDeclaration.JE_DeclarationReference;

		public IReadOnlyCollection<string> ImportSadNumbers => importSadNumbers ?? (importSadNumbers = emcsJobDeclaration.ImportSADNumbers.Cast<ImportSADNumber>().Select(x => x.CSI_Description.ToString()).ToArray());
		IReadOnlyCollection<string> importSadNumbers;

		public IReadOnlyCollection<IEMCSDocument> Documents => documents ?? (documents = emcsJobDeclaration.Documents.Cast<EMCSDocument>().Select(x => new DocumentProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSDocument> documents;

		public IReadOnlyCollection<IEMCSTransport> TransportDetails => transportDetails ?? (transportDetails = emcsJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new TransportProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public IReadOnlyCollection<IIE815Line> Lines => lines ?? (lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Select(x => new IE815LineProvider(x)).ToArray());
		IReadOnlyCollection<IIE815Line> lines;

		public string DeliveryPlaceCustomsOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDelivery);

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(emcsJobDeclaration.SpecialInstructions);

		JobDocAddress GetJobDocAddressWithFallback(JobDocAddress jobDocAddress, JobDocAddress fallbackJobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? jobDocAddress : fallbackJobDocAddress;
	}
}
