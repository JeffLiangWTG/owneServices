using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSContainerMessagingDataLayer : FreightDataLayer
	{
		#region Interface

		public CFSContainerMessagingDataLayer(CFSContainer container)
			: base(container)
		{
			this.Container = Argument.NotNull(container, "container");
		}
		protected new readonly CFSContainer Container;

		#endregion

		#region Consol Level FieldMappers

		protected override string GetVesselName()
		{
			ZString result = Container.JC_JV_NKVessel;
			if (result.IsEmpty)
			{
				result = MainTransport == null ? "" : (string)MainTransport.JW_Vessel;
			}
			return result;
		}

		protected override string GetVoyage()
		{
			ZString result = Container.JC_JV_VoyageFlight;
			if (result.IsEmpty)
			{
				result = MainTransport == null ? "" : (string)MainTransport.JW_VoyageFlight;
			}
			return result;
		}

		protected override string GetECNorCRN()
		{
			string result = "";
			result = Consol.JK_CRN;
			if (string.IsNullOrEmpty(result))
			{
				result = Container.JC_ExportDepotCustomsReference;
			}
			return result;
		}

		protected override string GetCartageCompanyABN()
		{
			return Consol.CartageCo == null ? ZString.Empty : Consol.CartageCo.PrimaryRegistrationNumber.Number;
		}

		protected override string GetLoadTerminal1StopCode()
		{
			return LoadListCTO.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, GlbBranch.CurrentBranch.Country);
		}

		#endregion

		#region LoadListCTO

		protected OrgHeader LoadListCTO
		{
			get
			{
				if (loadListCTO == null && !Consol.JK_OA_CTOAddress.IsEmpty)
				{
					OrgAddress cTOAddress = SavedFactory.Load<OrgAddress>(Consol.JK_OA_CTOAddress);
					if (cTOAddress != null && cTOAddress.Header != null)
					{
						loadListCTO = cTOAddress.Header;
					}
				}
				if (loadListCTO == null)
				{
					loadListCTO = SavedFactory.GetNull<OrgHeader>();
				}
				return loadListCTO;
			}
		}
		OrgHeader loadListCTO;

		#endregion

		#region Container

		protected override decimal GetContainerGrossWeight()
		{
			return Container.JC_GrossWeight;
		}

		protected override decimal GetContainerNetWeight()
		{
			return Container.JC_Calc_NetWeight;
		}

		protected override string GetTruckRegoNumber()
		{
			return Container.JC_DepartureTruckRegistration;
		}

		protected override ZDateTime GetRoadScheduledDeparture()
		{
			return Container.JC_DepartureTime;
		}

		#endregion

		#region GUI Location for Error Message

		protected override string ECNorCRNGUILocation
		{
			get { return "ECN/CRN on the container.\r\n"; }
		}

		protected override string ShippingLineBookingReferenceGUILocation
		{
			get { return "Carrier Ref on the load list"; }
		}

		protected override string VesselNameGUILocation
		{
			get { return "on the container or the main transport leg"; }
		}

		protected override string VoyageGUILocation
		{
			get { return VesselNameGUILocation; }
		}

		protected override string LloydsNumberGUILocation
		{
			get { return "Vessel Master File for selected vessel on the container or the main transport leg"; }
		}

		protected override string PortOfDischargeGUILocation
		{
			get { return "on the load list"; }
		}

		protected override string PortOfLoadingGUILocation
		{
			get { return "on the load list"; }
		}

		protected override string Commodity1StopCodeGUILocation
		{
			get { return "Container Details on the container"; }
		}

		protected override string ContainerGrossWeightGUILocation
		{
			get { return "Dimensions on the container"; }
		}

		protected override string SealNumberGUILocation
		{
			get { return "Container Details on the container"; }
		}

		#endregion

		#region Consol

		protected CFSLoadListConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Container.Consol;
				}
				if (consol == null)
				{
					consol = SavedFactory.GetNull<CFSLoadListConsol>();
				}
				return consol;
			}
		}
		CFSLoadListConsol consol;

		#endregion
	}
}
