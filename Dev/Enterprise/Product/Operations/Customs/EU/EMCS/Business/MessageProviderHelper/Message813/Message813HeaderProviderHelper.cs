using System;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message813HeaderProviderHelper : HeaderProviderHelper
	{
		public Message813HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration)
		{
		}

		public string JourneyTime => string.Concat(emcsJobDeclaration.JourneyTimeFormatPart, emcsJobDeclaration.JourneyTimeNumericPart.ToString().PadLeft(2, '0'));

		public string TransportModeCode => new TransportModeTranslator().TranslateToWCOCode(emcsJobDeclaration.JE_TransportMode);

		public string InvoiceNumber => emcsJobDeclaration.InvoiceNumber;

		public DateTime? InvoiceDate => emcsJobDeclaration.InvoiceDate.ToNullableDateTime();

		public string DestinationTypeCode => emcsJobDeclaration.JE_MessageSubType;

		public string GuarantorType => emcsJobDeclaration.ZG_GuarantorType;

		public string ChangedTransportArrangement => emcsJobDeclaration.ZG_TransportArrangement;

		public string DeliveryPlaceCustomsOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDelivery);
	}
}
