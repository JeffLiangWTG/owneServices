using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceHeaderReadOnlyStrategy : IReadOnlyStrategy
	{
		public DeltaIEJobComInvoiceHeaderReadOnlyStrategy(JobComInvoiceHeader invoiceHeader)
		{
			InvoiceHeader = invoiceHeader;
		}

		public JobComInvoiceHeader InvoiceHeader { get; }

		public bool IsReadonly(ZPropertyInfo propertyInfo)
		{
			switch (propertyInfo.Name)
			{
				case JobComInvoiceHeader.Schema.JZ_IncoTermPlace:
				case JobComInvoiceHeader.Schema.ZG_IncotermCountry:
					return InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other) || !InvoiceHeader.ZG_AgreedPlaceCode.IsEmpty;
				case JobComInvoiceHeader.Schema.ZG_AgreedPlaceCode:
					return InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other) || !InvoiceHeader.ZG_IncotermCountry.IsEmpty || !InvoiceHeader.JZ_IncoTermPlace.IsEmpty;
				case JobComInvoiceHeader.Schema.JZ_AdditionalTerms:
					return !InvoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other);
				default:
					return false;
			}
		}
	}
}
