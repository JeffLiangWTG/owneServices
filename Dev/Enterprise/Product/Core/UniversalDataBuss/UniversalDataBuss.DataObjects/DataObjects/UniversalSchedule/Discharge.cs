using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Discharge : IDataObject
	{
		[Mandatory]
		public UNLOCO Port { get; set; } // JB_RL_NKPortOfDischarge

		public ZDateTime? EstimatedArrival { get; set; } // JB_E_ARV 
		public ZDateTime? ActualArrival { get; set; } //JB_A_ARV 

		[MaxLength(10)]
		public ZString? ArrivalBerth { get; set; }  //JB_Berth
		[MaxLength(20)]
		public ZString? ArrivalReference { get; set; } //JB_ArrivalReference
		public OrganizationAddress ArrivalCTO { get; set; }  //JB_Calc_ArrivalCTOAddressOrg

		public ZDateTime? FCLAvailability { get; set; } // JB_AvailabilityDate
		public ZDateTime? FCLStorage { get; set; } //JB_StorageDate
	}
}
