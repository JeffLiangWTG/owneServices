using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class Confirmation : IDataObject
	{
		[MaxLength(80)]
		public ZString? DateDescription { get; set; }
		public ZDateTime? ActualDate { get; set; }
		public ZDateTime? ActualOutDate { get; set; }
		public ZDateTime? EstimatedDate { get; set; }
		public ZDateTime? EstimatedOutDate { get; set; }
		public ZDateTime? RequiredFromDate { get; set; }
		public ZDateTime? RequiredToDate { get; set; }
		public ZDateTime? SlotDate { get; set; }
		[MaxLength(20)]
		public ZString? SlotReference { get; set; }
		public ZInt? Quantity { get; set; }
		[MaxLength(50)]
		public ZString? Reference { get; set; }
		[MaxLength(50)]
		public ZString? ReceivedBy { get; set; }

		public ZDateTime? Demurrage { get; set; }

		public ZDecimal? Distance { get; set; }
		public UnitOfLength DistanceUnit { get; set; }

		public ZBool? IsEmptyContainer { get; set; }

		[MaxLength(2147483646), AllowLineControlWhiteSpace]
		public ZString? ServiceInstruction { get; set; }

		public ZInt? LegLink { get; set; }

		[MaxLength(10)]
		public ZString? VehicleRegistration { get; set; }

		[MaxLength(25)]
		public ZString? DriverDocumentID { get; set; }

		public OrganizationContact Driver { get; set; }

		public List<PackingLink> PackingLinkCollection { get; private set; }
	}
}
