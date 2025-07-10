using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class PartyInfo
	{
		public ZString? Name { get; set; }

		public ZString? TaxNumber { get; set; }

		public ZString? GroupMemberTaxNumber { get; set; }

		public ZString? CommunityMemberVatNumber { get; set; }

		public ZString? ThirdStateTaxId { get; set; }

		public ZString? CountryCode { get; set; }

		public ZString? PostCode { get; set; }

		public ZString? City { get; set; }

		public ZString? AdditionalAddressDetail { get; set; }

		public string VATStatus { get; set; }

		public bool IsPrivatePerson { get; set; }

		public InvoiceDeliveryMethod? InvoiceDeliveryMethod { get; set; }
	}
}
