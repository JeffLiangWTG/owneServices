using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC140C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE141MessageProvider : NctsDepartureHeaderMessageProvider, IIE141Header
	{
		public IE141MessageProvider(NctsHeader nctsHeader, NctsHeaderMessageSendingObject sendingObject) : base(nctsHeader)
		{
			this.sendingObject = sendingObject;
		}

		readonly NctsHeaderMessageSendingObject sendingObject;

		public IMRN TransitOperation => transitOperation ?? (transitOperation = new IE141TransitOperationProvider(NctsHeader));
		IMRN transitOperation;

		public string CustomsOfficeOfDestinationActual => sendingObject.EnquiryText.IsEmpty ? null : sendingObject.DestinationCustomsOfficeCode;

		public string CustomsOfficeOfEnquiryAtDeparture => CachedValueHelper.GetValue(ref customsOfficeOfEnquiryAtDeparture, () =>
		{
			var previousMessage = NctsHeader.Messages.GetLastMessage(NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS, NCTSIncomingMessageTypeList.Codes.IE140, NCTSInboundEDIMessage.Direction.Receive) as NCTSInboundEDIMessage;
			return previousMessage != null ? GetCustomsOfficeOfEnquiryFromPreviousMessage(previousMessage) : null;
		});
		CachedValue<string> customsOfficeOfEnquiryAtDeparture;

		public IHolder HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedure, () => HolderOfTransitProcedureProvider.New(NctsHeader.Principal, MovementHeader.BM_InBondEntryType));
		CachedValue<IHolder> holderOfTheTransitProcedure;

		public DateTime? EnquiryTC11DeliveryDate => sendingObject.TC11DeliveryDate.Date is ZDate deliveryDate && deliveryDate.IsValid ? deliveryDate.ToDateTime() : (DateTime?)null;

		public string EnquiryText => sendingObject.EnquiryText;

		public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () =>
		{
			IParty result = null;
			if (!sendingObject.TC11DeliveryDate.IsEmpty && !sendingObject.EnquiryText.IsEmpty)
			{
				result = PartyProvider.New(OrgHeader.LoadFromCode(NctsHeader.Factory, sendingObject.Consignee)?.MainAddress);
			}
			return result;
		});
		CachedValue<IParty> consignee;

		string GetCustomsOfficeOfEnquiryFromPreviousMessage(NCTSInboundEDIMessage previousMessage)
		{
			var dataProvider = previousMessage.GetDataProvider<CC140CProvider>(typeof(Cc140CType));
			return dataProvider == null ? null : dataProvider.CustomsOfficeOfEnquiry;
		}
	}
}
