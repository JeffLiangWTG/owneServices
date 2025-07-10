using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DeliveryTermsWrapper : IDeliveryTerms
	{
		DeliveryTermsWrapper(JobComInvoiceHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly JobComInvoiceHeader header;

		public static DeliveryTermsWrapper New(JobComInvoiceHeader header) => header == null ? null : new DeliveryTermsWrapper(header);

		ZString PlaceCode => header.ZG_AgreedPlaceCode.IsEmpty ? (header.JobDeclaration?.ZG_AgreedPlaceCode ?? ZString.Empty) : header.ZG_AgreedPlaceCode;

		public string Country => country ?? (country = header.ZG_IncotermCountry);
		string country;

		public string IncotermCode => incotermCode ?? (incotermCode = header.JZ_IncoTerm);
		string incotermCode;

		public string Location => location ?? (location = header.JZ_IncoTermPlace);
		string location;

		public string Text => header.JZ_AdditionalTerms;

		public string UNLOCODE => uNLOCODE ?? (uNLOCODE = PlaceCode);
		string uNLOCODE;
	}
}
