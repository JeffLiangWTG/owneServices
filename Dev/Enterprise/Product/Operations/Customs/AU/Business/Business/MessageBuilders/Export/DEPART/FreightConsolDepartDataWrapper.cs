using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FreightConsolDepartDataWrapper : IDepartDataWrapper
	{
		public FreightConsolDepartDataWrapper(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public bool IsAir
		{
			get
			{
				return consol.IsAir;
			}
		}

		public bool IsSea
		{
			get
			{
				return consol.IsSea;
			}
		}

		public ZString FlightNumber
		{
			get
			{
				return consol.JK_JX_JV_VoyageFlight.KeepChars("0123456789");
			}
		}

		public ZString AirlineCode
		{
			get
			{
				return consol.JK_JX_JV_VoyageFlight.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				return consol.JK_JX_JV_VoyageFlight;
			}
		}

		public ZString VesselID
		{
			get
			{
				return consol.Vessel == null ? ZString.Empty : consol.Vessel.RV_LloydsNumber;
			}
		}

		public ZString CTOEstablishmentID
		{
			get { return consol.DepartureCTOAddress != null ? consol.DepartureCTOAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZDateTime DepartureDateTime
		{
			get
			{
				return consol.JK_JX_JA_A_DEP;
			}
		}

		public ZString PortOfDestination
		{
			get
			{
				return consol.JK_JX_JB_RL_NKPortOfDischarge;
			}
		}

		public ZString CarrierPartyID
		{
			get
			{
				ZString result = GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo;
				if (result.IsEmpty)
				{
					result = GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode;
				}

				return result;
			}
		}

		#region Implementation

		protected ForwardingConsol consol;

		#endregion
	}
}
