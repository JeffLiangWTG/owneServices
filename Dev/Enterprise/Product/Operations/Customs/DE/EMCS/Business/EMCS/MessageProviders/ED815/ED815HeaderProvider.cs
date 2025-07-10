using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED815HeaderProvider : HeaderProvider, IED815Header
	{
		public ED815HeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
			helper = new Message815HeaderProviderHelper(emcsJobDeclaration);
		}
		readonly Message815HeaderProviderHelper helper;

		public string JourneyTime => helper.JourneyTime;

		public string TransportModeCode => helper.TransportModeCode;

		public string InvoiceNumber
		{
			get
			{
				var result = emcsJobDeclaration.InvoiceNumber;
				return result.IsEmpty && emcsJobDeclaration.IsConsolidatedDocument() ? (ZString)Constants.ConsolidatedDocumentDefaultString : result;
			}
		}

		public DateTime? InvoiceDate => helper.InvoiceDate;

		public string DestinationTypeCode => helper.DestinationTypeCode;

		public string GuarantorType => helper.GuarantorType;

		public IEMCSPartyGuarantor GuarantorTrader => guarantorTrader ?? (guarantorTrader = GetGuarantorTrader());
		IEMCSPartyGuarantor guarantorTrader;

		public string DispatchImportOfficeReferenceNumber => helper.DispatchImportOfficeReferenceNumber;

		public string CompetentAuthorityDispatchOfficeReferenceNumber => helper.CompetentAuthorityDispatchOfficeReferenceNumber;

		public IEMCSPartyConsignee ConsigneeTrader => CachedValueHelper.GetValue(ref consigneeTrader, () =>
		{
			IEMCSPartyConsignee result = null;
			if (emcsJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown ||
				emcsJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
			{
				result = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress);
			}

			return result;
		});
		CachedValue<IEMCSPartyConsignee> consigneeTrader;

		public IEMCSPartyConsignor ConsignorTrader => consignorTrader ?? (consignorTrader = PartyConsignorProvider.NewOrNull(emcsJobDeclaration.SupplierDocumentaryAddress));
		IEMCSPartyConsignor consignorTrader;

		public IEMCSPartyPlaceOfDispatch PlaceOfDispatchTrader =>
			CachedValueHelper.GetValue(ref placeOfDispatchTrader, () =>
			{
				IEMCSPartyPlaceOfDispatch result = null;
				if (emcsJobDeclaration.ZG_OriginType == EMCSOriginTypeList.Codes.TaxWarehouse)
				{
					result = PartyPlaceOfDispatchProvider.NewOrNull(helper.GetJobDocAddressWithFallback(emcsJobDeclaration.DispatchWarehouseDocumentaryAddress, emcsJobDeclaration.SupplierDocumentaryAddress));
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
				result = PartyDeliveryPlaceProvider.NewOrNull(helper.GetJobDocAddressWithFallback(emcsJobDeclaration.DestinationWarehouseDocumentaryAddress, emcsJobDeclaration.ImporterDocumentaryAddress));
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

		public string ComplementConsigneeMemberStateCode => helper.ComplementConsigneeMemberStateCode;

		public string ComplementConsigneeSerialNumberOfCertificateOfExemption => helper.ComplementConsigneeSerialNumberOfCertificateOfExemption;

		public string DeferredSubmissionFlag => helper.DeferredSubmissionFlag;

		public string SubmissionMessageType => helper.SubmissionMessageType;

		public string TransportArrangement => helper.TransportArrangement;

		public DateTime? DispatchDateTime => helper.DispatchDateTime;

		public string OriginType => helper.OriginType;

		public string LocalReferenceNumber => helper.LocalReferenceNumber;

		public IReadOnlyCollection<string> ImportSadNumbers => helper.ImportSadNumbers;

		public IReadOnlyCollection<IEMCSDocument> Documents => documents ?? (documents = emcsJobDeclaration.Documents.Cast<EMCSDocument>().Select(x => new DocumentProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSDocument> documents;

		public IReadOnlyCollection<IEMCSTransport> TransportDetails => transportDetails ?? (transportDetails = emcsJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new ED815TransportProvider(x, emcsJobDeclaration)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public IReadOnlyCollection<IED815Line> Lines => lines ?? (lines = emcsJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Select(x => new ED815LineProvider(x)).ToArray());
		IReadOnlyCollection<IED815Line> lines;

		public string DeliveryPlaceCustomsOfficeReferenceNumber => helper.DeliveryPlaceCustomsOfficeReferenceNumber;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(emcsJobDeclaration.SpecialInstructions);
	}
}
