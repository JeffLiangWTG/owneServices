using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderSailingSynchroniser : BusinessObjectSynchroniser
	{
		public JPAFRHeaderSailingSynchroniser(JPAFRHeader destination)
			: base(destination, destination.Sailing)
		{
		}

		protected new JPAFRHeader Destination
		{
			get { return (JPAFRHeader)base.Destination; }
		}

		protected new JobSailing Source
		{
			get { return (JobSailing)base.Source; }
		}

		#region Hook and Unhook FieldSynchronisers

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			carrierAddressSynchroniser = new FieldSynchroniser(Destination.Carrier.E2_OA_AddressInfo, GetCarrier, GetInfosAffectingCarrier);
			carrierCodeSynchroniser = new FieldSynchroniser(Destination.JPH_CarrierCodeInfo, GetCarrierCode, GetInfosAffectingCarrier);
			voyageSynchroniser = new FieldSynchroniser(Destination.JPH_VoyageInfo, Source.JX_JV_VoyageFlightInfo);
			vesselSynchroniser = new FieldSynchroniser(Destination.JPH_VesselNameInfo, Source.JX_JV_NKVesselInfo);
			loadingSynchroniser = new FieldSynchroniser(Destination.JPH_RL_NKLoadingInfo, Source.JX_JA_RL_NKPortOfLoadingInfo);
			dischargeSynchroniser = new FieldSynchroniser(Destination.JPH_RL_NKDischargeInfo, Source.JX_JB_RL_NKPortOfDischargeInfo);
			etdSynchroniser = new FieldSynchroniser(Destination.JPH_ETDInfo, GetETD, GetETDInfo);
			etaSynchroniser = new FieldSynchroniser(Destination.JPH_ETAInfo, GetETA, GetETAInfo);

			Synchronisers.Add(carrierAddressSynchroniser);
			Synchronisers.Add(carrierCodeSynchroniser);
			Synchronisers.Add(voyageSynchroniser);
			Synchronisers.Add(vesselSynchroniser);
			Synchronisers.Add(loadingSynchroniser);
			Synchronisers.Add(dischargeSynchroniser);
			Synchronisers.Add(etdSynchroniser);
			Synchronisers.Add(etaSynchroniser);
		}

		FieldSynchroniser carrierAddressSynchroniser;
		FieldSynchroniser carrierCodeSynchroniser;
		FieldSynchroniser voyageSynchroniser;
		FieldSynchroniser vesselSynchroniser;
		FieldSynchroniser loadingSynchroniser;
		FieldSynchroniser dischargeSynchroniser;
		FieldSynchroniser etdSynchroniser;
		FieldSynchroniser etaSynchroniser;

		protected override void UnHookSynchronisers()
		{
			carrierAddressSynchroniser = null;
			carrierCodeSynchroniser = null;
			voyageSynchroniser = null;
			vesselSynchroniser = null;
			loadingSynchroniser = null;
			dischargeSynchroniser = null;
			etdSynchroniser = null;
			etaSynchroniser = null;
			base.UnHookSynchronisers();
		}

		IZType GetCarrierCode()
		{
			return ConsolDataCalculator.GetSCAC(Destination.Carrier);
		}

		IZType GetCarrier()
		{
			var carrierOrgHeader = Destination.Factory.Load<OrgHeader>(Source.JX_JV_OH_Line);
			var mainAddress = carrierOrgHeader == null ? null : carrierOrgHeader.MainAddress;
			return mainAddress == null ? ZGuid.Empty : mainAddress.PK;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingCarrier()
		{
			yield return Source.JX_JV_OH_LineInfo;
		}

		IZType GetETD()
		{
			var result = Source.JX_JA_A_DEP;
			return result.IsEmpty ? Source.JX_JA_E_DEP : result;
		}

		IEnumerable<ZPropertyInfo> GetETDInfo()
		{
			yield return Source.JX_JA_A_DEPInfo;
			yield return Source.JX_JA_E_DEPInfo;
		}

		IZType GetETA()
		{
			var result = Source.JX_JB_A_ARV;
			return result.IsEmpty ? Source.JX_JB_E_ARV : result;
		}

		IEnumerable<ZPropertyInfo> GetETAInfo()
		{
			yield return Source.JX_JB_A_ARVInfo;
			yield return Source.JX_JB_E_ARVInfo;
		}

		#endregion
	}
}
