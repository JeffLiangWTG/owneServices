using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IAsycudaBill : ManifestBase.IAsycudaBill
			{
				ZGuid ShipperOrgPK { get; set; }
				ZGuid ConsigneeOrgPK { get; set; }
				ZGuid NotifyPartyOrgPK { get; set; }
				ZGuid ForwarderOrgPK { get; set; }
				ZDecimal DiscountValue { get; set; }
				ZString DiscountValueCurrency { get; set; }
				ZString MatchingReference { get; set; }
				ZDecimal OtherChargesValue { get; set; }
				ZString OtherChargesValueCurrency { get; set; }
				ZString BillIssuerName { get; }
				ZString CustomsEntryNumber { get; set; }
				ZString CustomsEntryNumberType { get; set; }
				ZString CustomsJobNumber { get; set; }
				ZDecimal DutyAmount { get; set; }
				ZDateTime RegistrationDate { get; set; }
				ZString RegistrationNumber { get; }
				ZString StatusDescription { get; }
				ZDecimal TaxAmount { get; set; }

				void SuspendValidation();
			}
		}
	}
}
