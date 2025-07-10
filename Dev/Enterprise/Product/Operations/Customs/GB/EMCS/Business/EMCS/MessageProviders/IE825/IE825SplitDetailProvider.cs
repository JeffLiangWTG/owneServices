using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE825SplitDetailProvider : SplitProvider, IIE825SplitDetail
	{
		public IE825SplitDetailProvider(EMCSJobComInvoiceHeader emcsJobComInvoiceHeader) : base(emcsJobComInvoiceHeader) { }

		public string LocalReferenceNumber => EMCSJobDeclaration.JE_DeclarationReference;

		public string JourneyTime => string.Concat(EMCSJobDeclaration.JourneyTimeFormatPart, EMCSJobDeclaration.JourneyTimeNumericPart.ToString().PadLeft(2, '0'));

		public string ChangedTransportArrangement => EMCSJobDeclaration.ZG_TransportArrangement;

		public bool ChangedTransportArrangementSpecified => !ChangedTransportArrangement.IsNullOrEmpty();

		public string DestinationChangedSplittingDestinationTypeCode => EMCSJobDeclaration.JE_MessageSubType;

		public IEMCSPartyConsignee NewConsigneeTrader
		{
			get
			{
				if (newConsigneeTrader == null)
				{
					if (EMCSJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown && EMCSJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport)
					{
						newConsigneeTrader = PartyConsigneeProvider.NewOrNull(EMCSJobDeclaration.ImporterDocumentaryAddress);
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
					if (EMCSJobDeclaration.ZG_SubmissionType != EMCSSubmissionTypeList.Codes.SubmissionForExport
						&& EMCSJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown
						&& EMCSJobDeclaration.JE_MessageSubType != EMCSDestinationTypeList.Codes.DestinationExport)
					{
						deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(EMCSJobDeclaration.DestinationWarehouseDocumentaryAddress);
					}
				}
				return deliveryPlaceTrader;
			}
		}
		IEMCSPartyDeliveryPlace deliveryPlaceTrader;

		public string DeliveryPlaceCustomsOfficeReferenceNumber => EMCSJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDelivery);

		public IEMCSPartyTransporter NewTransportArrangerTrader => newTransportArrangerTrader ?? (newTransportArrangerTrader = PartyTransporterProvider.NewOrNull(EMCSJobDeclaration.CarrierAgentDocumentaryAddress));
		IEMCSPartyTransporter newTransportArrangerTrader;

		public IEMCSPartyTransporter NewTransporterTrader => newTransporterTrader ?? (newTransporterTrader = PartyTransporterProvider.NewOrNull(EMCSJobDeclaration.TransporterDocumentaryAddress));
		IEMCSPartyTransporter newTransporterTrader;

		public IReadOnlyCollection<IEMCSTransport> TransportDetails => transportDetails ?? (transportDetails = EMCSJobDeclaration.CusContainers.Cast<EMCSCusContainer>().Select(x => new TransportProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSTransport> transportDetails;

		public IReadOnlyCollection<IIE825Line> Lines => lines ?? (lines = EMCSJobDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Select(x => new IE825LineProvider(x)).ToArray());
		IReadOnlyCollection<IIE825Line> lines;
	}
}
