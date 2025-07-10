using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE813HeaderProvider : HeaderProvider, IIE813Header
	{
		public IE813HeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }

		public string JourneyTime => string.Concat(emcsJobDeclaration.JourneyTimeFormatPart, emcsJobDeclaration.JourneyTimeNumericPart.ToString().PadLeft(2, '0'));

		public string TransportModeCode => new EU.EMCS.Business.TransportModeTranslator().TranslateToWCOCode(emcsJobDeclaration.JE_TransportMode);

		public string InvoiceNumber => emcsJobDeclaration.InvoiceNumber;

		public DateTime? InvoiceDate => emcsJobDeclaration.InvoiceDate.ToNullableDateTime();

		public string DestinationTypeCode => emcsJobDeclaration.JE_MessageSubType;

		public string GuarantorType => emcsJobDeclaration.ZG_GuarantorType;

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

		public string ChangedTransportArrangement => emcsJobDeclaration.ZG_TransportArrangement;

		IReadOnlyCollection<IEMCSTransport> IIE813Header.TransportDetails => transportDetails ?? (transportDetails = emcsJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new TransportProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(emcsJobDeclaration.SpecialInstructions);

		public string DeliveryPlaceCustomsOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDelivery);

		public DateTime? DateAndTimeOfValidationOfChangeOfDestination => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;
	}
}
