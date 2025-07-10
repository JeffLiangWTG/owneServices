using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Freight
{
	public class SailingSource : ITransportDetails
	{
		public SailingSource(JobSailing sailing)
		{
			this.sailing = sailing;
		}

		#region ITransportDetails Members

		public ZString ParentDescription
		{
			get { return ""; }
		}

		public ZString TransportMode
		{
			get { return sailing.Voyage.JV_AirSeaRoad; }
		}

		public ZString TransportType
		{
			get { return ""; }
		}

		public ZString TransportTypeDescription
		{
			get { return ""; }
		}

		public ZString Vessel
		{
			get { return sailing.JX_JV_NKVessel; }
		}

		public ZString VoyageFlight
		{
			get { return sailing.JX_JV_VoyageFlight; }
		}

		public ZString Load
		{
			get { return sailing.JX_JA_RL_NKPortOfLoading; }
		}

		public ZString Discharge
		{
			get { return sailing.JX_JB_RL_NKPortOfDischarge; }
		}

		public ZByte LegOrder
		{
			get { return 0; }
		}

		public ZDateTime ETD
		{
			get { return sailing.JX_JA_E_DEP; }
		}

		public ZDateTime ETA
		{
			get { return sailing.JX_JB_E_ARV; }
		}

		public ZDateTime ATD
		{
			get { return sailing.JX_JA_A_DEP; }
		}

		public ZDateTime ATA
		{
			get { return sailing.JX_JB_A_ARV; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return sailing.JX_DepotReceivalCommences; }
		}

		public ZDateTime LCLCutOff
		{
			get { return sailing.JX_DepotCutOff; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return sailing.JX_DepotAvailabilityDate; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return sailing.JX_DepotStorageDate; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return sailing.JX_JA_CTOReceivalCommences; }
		}

		public ZDateTime FCLCutOff
		{
			get { return sailing.JX_JA_CTOCutOff; }
		}

		public ZDateTime FCLAvailabilityDate
		{
			get { return sailing.JX_JB_CTOAvailabilityDate; }
		}

		public ZDateTime FCLStorageDate
		{
			get { return sailing.JX_JB_CTOStorageDate; }
		}

		public ZGuid Carrier
		{
			get { return sailing.JX_JV_OH_Line; }
		}

		IFlightDetailsSuppression ITransportDetails.SuppressingBizO
		{
			get { return sailing; }
		}

		#endregion

		readonly JobSailing sailing;
	}
}
