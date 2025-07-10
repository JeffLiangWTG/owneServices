using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Inner)]
	public class ACAS : IDataObject
	{
		[MaxLength(254)]
		public ZString? BiographicData { get; set; }

		[MaxLength(3)]
		public ZString? CustAccBillingType { get; set; }

		[MaxLength(7)]
		public ZString? CustAccEstDate { get; set; }

		[MaxLength(1)]
		public ZString? CustAccShippingFrequency { get; set; }

		[MaxLength(254)]
		public ZString? ConsigneeEmailDomain { get; set; }

		[MaxLength(64)]
		public ZString? ConsigneeEmailLocal { get; set; }

		[MaxLength(1)]
		public ZString? CustomerAccountHolder { get; set; }

		[MaxLength(254)]
		public ZString? CustomerAccountIssuer { get; set; }

		[MaxLength(254)]
		public ZString? CustomerAccountName { get; set; }

		[MaxLength(14)]
		public ZString? CustomerAccountNumber { get; set; }

		[MaxLength(45)]
		public ZString? IPAddressAccCreation { get; set; }

		[MaxLength(45)]
		public ZString? IPAddressRqShpBillCreation { get; set; }

		[MaxLength(254)]
		public ZString? ShipperEmailDomain { get; set; }

		[MaxLength(64)]
		public ZString? ShipperEmailLocal { get; set; }

		[MaxLength(1)]
		public ZString? VerifiedKnownConsignor { get; set; }
	}
}
