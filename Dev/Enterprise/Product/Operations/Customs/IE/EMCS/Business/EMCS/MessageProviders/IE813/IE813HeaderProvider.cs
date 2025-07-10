using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE813HeaderProvider : HeaderProvider, IIE813Header
	{
		public IE813HeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
			helper = new Message813HeaderProviderHelper(emcsJobDeclaration);
		}
		readonly Message813HeaderProviderHelper helper;

		public string JourneyTime => helper.JourneyTime;

		public string TransportModeCode => helper.TransportModeCode;

		public string InvoiceNumber => helper.InvoiceNumber;

		public DateTime? InvoiceDate => helper.InvoiceDate;

		public string DestinationTypeCode => helper.DestinationTypeCode;

		public string GuarantorType => helper.GuarantorType;

		public IEMCSPartyGuarantor GuarantorTrader => guarantorTrader ?? (guarantorTrader = GetGuarantorTrader());
		IEMCSPartyGuarantor guarantorTrader;

		public IEMCSPartyTransporter NewTransportArrangerTrader
		{
			get
			{
				IEMCSPartyTransporter result = null;
				if (ChangedTransportArrangement != EMCSTransportArrangementList.Codes.Consignor && ChangedTransportArrangement != EMCSTransportArrangementList.Codes.Consignee)
				{
					result = newTransportArrangerTrader ?? (newTransportArrangerTrader = PartyTransporterProvider.NewOrNull(emcsJobDeclaration.CarrierAgentDocumentaryAddress));
				}

				return result;
			}
		}
		IEMCSPartyTransporter newTransportArrangerTrader;

		public IEMCSPartyTransporter NewTransporterTrader => newTransporterTrader ?? (newTransporterTrader = PartyTransporterProvider.NewOrNull(emcsJobDeclaration.TransporterDocumentaryAddress));
		IEMCSPartyTransporter newTransporterTrader;

		public IEMCSPartyConsignee NewConsigneeTrader
		{
			get
			{
				if (newConsigneeTrader == null)
				{
					if (emcsJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown && emcsJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
					{
						newConsigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress);
					}
				}
				return newConsigneeTrader;
			}
		}
		IEMCSPartyConsignee newConsigneeTrader;

		public IEMCSPartyDeliveryPlace DeliveryPlaceTrader
		{
			get
			{
				if (deliveryPlaceTrader == null)
				{
					if (emcsJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport
						&& emcsJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown
						&& emcsJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationExport)
					{
						deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(emcsJobDeclaration.DestinationWarehouseDocumentaryAddress);
					}
				}
				return deliveryPlaceTrader;
			}
		}
		IEMCSPartyDeliveryPlace deliveryPlaceTrader;

		public string ChangedTransportArrangement => helper.ChangedTransportArrangement;

		IEnumerable<IEMCSTransport> IIE813Header.TransportDetails => transportDetails ?? (transportDetails = emcsJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new TransportProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(emcsJobDeclaration.SpecialInstructions);

		public string DeliveryPlaceCustomsOfficeReferenceNumber => helper.DeliveryPlaceCustomsOfficeReferenceNumber;
	}
}
