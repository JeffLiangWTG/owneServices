using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED813HeaderProvider : HeaderProvider, IED813Header
	{
		public ED813HeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
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

		public IEMCSPartyTransporter NewTransportArrangerTrader => newTransportArrangerTrader ?? (newTransportArrangerTrader = PartyTransporterProvider.NewOrNull(emcsJobDeclaration.CarrierAgentDocumentaryAddress));
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

		public IReadOnlyCollection<IEMCSTransport> TransportDetails => transportDetails ?? (transportDetails = emcsJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new TransportProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(emcsJobDeclaration.SpecialInstructions);

		public string DeliveryPlaceCustomsOfficeReferenceNumber => helper.DeliveryPlaceCustomsOfficeReferenceNumber;

		public string OriginType => emcsJobDeclaration.ZG_OriginType;
	}
}
