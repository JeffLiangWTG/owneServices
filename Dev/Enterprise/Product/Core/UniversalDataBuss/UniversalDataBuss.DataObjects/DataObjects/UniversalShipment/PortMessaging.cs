using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class PortMessaging : IDataObject
	{
		public ZDateTime? Date { get; set; }
		public ZDateTime? CustomsReleaseDate { get; set; }

		[MaxLength(10)]
		public ZString? Berth { get; set; }
		[MaxLength(100)]
		public ZString? OperatorName { get; set; }
		[MaxLength(20)]
		public ZString? Telephone { get; set; }
		[MaxLength(20)]
		public ZString? Fax { get; set; }
		[MaxLength(128)]
		public ZString? Email { get; set; }
		[MaxLength(24)]
		public ZString? ShippingLine { get; set; }
		[MaxLength(70)]
		public ZString? SenderAgentCode { get; set; }
		[MaxLength(100)]
		public ZString? SenderAgentName { get; set; }
		[MaxLength(70)]
		public ZString? SendingAgentQuayAccount { get; set; }
		[MaxLength(35)]
		public ZString? VesselName { get; set; }
		[MaxLength(35)]
		public ZString? BillNo { get; set; }
		public ZDateTime? Departure { get; set; }
		[MaxLength(10)]
		public ZString? VoyageNo { get; set; }
		[MaxLength(5)]
		public ZString? Destination { get; set; }
		[MaxLength(100)]
		public ZString? Remarks { get; set; }
		[MaxLength(70)]
		public ZString? ShipperCode { get; set; }
		[MaxLength(100)]
		public ZString? ShipperName { get; set; }
		[MaxLength(70)]
		public ZString? ShipperQuayAccount { get; set; }
		public CodeDescriptionPair TypeOfDeclaration { get; set; }
		[MaxLength(70)]
		public ZString? MRN { get; set; }
		[MaxLength(1)]
		public ZString? MRNComplete { get; set; }
		[MaxLength(3)]
		public ZString? MessagePurpose { get; set; }
		public CodeDescriptionPair ExemptionReason { get; set; }
		[MaxLength(25)]
		public ZString? ATB { get; set; }
		public CodeDescriptionPair Annex30AType { get; set; }
		public ZBool? Annex30AFailureProcess { get; set; }
		[MaxLength(18)]
		public ZString? ExportDeclarationNumber { get; set; }
		[MaxLength(8)]
		public ZString? ForwardingCustomsOfficeCode { get; set; }
		[MaxLength(70)]
		public ZString? LRN { get; set; }
		[MaxLength(1)]
		public ZString? LRNComplete { get; set; }
	}
}
