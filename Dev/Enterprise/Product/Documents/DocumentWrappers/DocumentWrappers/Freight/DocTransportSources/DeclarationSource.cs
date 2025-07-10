using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Freight
{
	public class DeclarationSource : ITransportDetails
	{
		public DeclarationSource(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		#region ITransportDetails Members

		public ZString ParentDescription
		{
			get { return declaration.JE_DeclarationReference; }
		}

		public ZString TransportMode
		{
			get { return declaration.JE_TransportMode; }
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
			get { return declaration.JE_VesselName; }
		}

		public ZString VoyageFlight
		{
			get { return declaration.JE_VoyageFlightNo; }
		}

		public ZString Load
		{
			get { return declaration.JE_RL_NKPortOfLoading; }
		}

		public ZString Discharge
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		public ZByte LegOrder
		{
			get { return 0; }
		}

		public ZDateTime ETD
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime ETA
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime ATD
		{
			get { return declaration.JE_ExportDate; }
		}

		public ZDateTime ATA
		{
			get { return declaration.JE_DateOfArrival; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLAvailabilityDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime FCLStorageDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZGuid Carrier
		{
			get { return ZGuid.Empty; }
		}

		IFlightDetailsSuppression ITransportDetails.SuppressingBizO
		{
			get { return declaration; }
		}

		#endregion

		readonly BaseJobDeclaration declaration;
	}
}
