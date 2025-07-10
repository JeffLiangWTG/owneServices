using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DeliveryTermsProvider : IDeliveryTerms
	{
		public static DeliveryTermsProvider New(JobComInvoiceHeader invoiceHeader) => new DeliveryTermsProvider(invoiceHeader.JZ_IncoTerm, invoiceHeader.JZ_IncoTermPlace, invoiceHeader.ZG_AgreedPlaceCode, invoiceHeader.JZ_AdditionalTerms, invoiceHeader.IsUCC6AndIsExport() || invoiceHeader.IsUCC6AndIsImport());

		DeliveryTermsProvider(ZString incotermCode, ZString incoTermPlace, ZString agreedPlaceCode, ZString additionalTerms, ZBool isUCC6)
		{
			IncotermCode = incotermCode.ToUpper();
			if (!isUCC6 || IncotermCode != "XXX")
			{
				switch (agreedPlaceCode.Length)
				{
					case 2:
						CountryCode = agreedPlaceCode;
						Place = incoTermPlace;
						break;
					case 5:
						UNLOCODE = agreedPlaceCode;
						break;
				}
			}
			else
			{
				Text = additionalTerms;
			}
		}

		public string IncotermCode { get; }
		public string UNLOCODE { get; }
		public string CountryCode { get; }
		public string Place { get; }
		public string Text { get; }
	}
}
