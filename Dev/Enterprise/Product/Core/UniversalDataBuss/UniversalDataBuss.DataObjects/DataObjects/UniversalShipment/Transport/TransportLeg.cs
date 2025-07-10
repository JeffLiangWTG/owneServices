using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class TransportLeg : IDataObject, ICustomizedFieldContainer
	{
		public TransportLeg()
		{
		}

		public TransportLeg(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZInt? Link { get; set; }

		[Mandatory]
		public ZByte? LegOrder { get; set; }
		public TransportMode? TransportMode { get; set; }
		public OrganizationAddress Carrier { get; set; }
		public OrganizationAddress Creditor { get; set; }
		public LegType? LegType { get; set; }

		[MaxLength(35)]
		public ZString? VesselName { get; set; }
		[MaxLength(7)]
		public ZString? VesselLloydsIMO { get; set; }
		[MaxLength(10)]
		public ZString? VoyageFlightNo { get; set; }
		public CodeDescriptionPair AircraftType { get; set; }

		[MaxLength(20)]
		public ZString? CarrierBookingReference { get; set; }

		public ServiceLevel CarrierServiceLevel { get; set; }

		public ZBool? IsCargoOnly { get; set; }

		// Loading
		[CandidateKey]
		public UNLOCO PortOfLoading { get; set; }

		public ZDateTime? EstimatedArrivalInPortOfLoading { get; set; }
		public ZDateTime? ActualArrivalInPortOfLoading { get; set; }

		public ZDateTime? EstimatedDeparture { get; set; }
		public ZDateTime? ActualDeparture { get; set; }

		[MaxLength(10)]
		public ZString? DepartureBerth { get; set; }
		[MaxLength(20)]
		public ZString? DepartureReference { get; set; }
		public OrganizationAddress DepartureCTO { get; set; }
		public OrganizationAddress DepartureFrom { get; set; }

		public ZDateTime? DocumentCutOff { get; set; }

		public ZDateTime? VGMCutOff { get; set; }

		public ZDateTime? FCLReceivalCommences { get; set; }
		public ZDateTime? FCLCutOff { get; set; }

		public ZDateTime? LCLReceivalCommences { get; set; }
		public ZDateTime? LCLCutOff { get; set; }

		public ZDateTime? HazzardReceivalCommences { get; set; }  //HazzardReciveStartDate
		public ZDateTime? HazzardCutOffDate { get; set; }

		public ZDateTime? EmptyReceivalCommences { get; set; }
		public ZDateTime? EmptyCutOff { get; set; }

		public ZDateTime? ReeferReceivalCommences { get; set; }
		public ZDateTime? ReeferCutOff { get; set; }

		//Discharge
		[CandidateKey]
		public UNLOCO PortOfDischarge { get; set; }

		public ZDateTime? EstimatedArrival { get; set; }
		public ZDateTime? ActualArrival { get; set; }

		[MaxLength(10)]
		public ZString? ArrivalBerth { get; set; }  //JB_Berth
		[MaxLength(20)]
		public ZString? ArrivalReference { get; set; } //JB_ArrivalReference
		public OrganizationAddress ArrivalCTO { get; set; }  //JB_Calc_ArrivalCTOAddressOrg
		public OrganizationAddress ArrivalAt { get; set; }

		public ZDateTime? FCLAvailability { get; set; }
		public ZDateTime? FCLStorage { get; set; }

		public ZDateTime? LCLAvailability { get; set; }
		public ZDateTime? LCLStorageDate { get; set; }

		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public List<AdditionalTransportMode> AdditionalTransportModeCollection { get; private set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? LegNotes { get; set; }
		public CodeDescriptionPair BookingStatus { get; set; }

		public ZDateTime? ScheduledDeparture { get; set; }
		public ZDateTime? ScheduledArrivalInPortOfLoading { get; set; }
		public ZDateTime? ScheduledArrival { get; set; }

		public GreenhouseGasEmission GreenhouseGasEmission { get; set; }
	}
}
