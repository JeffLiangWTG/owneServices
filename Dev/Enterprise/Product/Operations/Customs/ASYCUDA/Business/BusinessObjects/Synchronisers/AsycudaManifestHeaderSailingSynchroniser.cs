using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	sealed class AsycudaManifestHeaderSailingSynchroniser : BusinessObjectSynchroniser
	{
		public AsycudaManifestHeaderSailingSynchroniser(AsycudaManifestHeader destination, JobSailing sailing)
			: base(destination, sailing)
		{
		}

		new AsycudaManifestHeader Destination
		{
			get { return (AsycudaManifestHeader)base.Destination; }
		}

		new JobSailing Source
		{
			get { return (JobSailing)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_RL_NKPortOfLoadingInfo, Source.JX_JA_RL_NKPortOfLoadingInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_RL_NKPortOfDischargeInfo, Source.JX_JB_RL_NKPortOfDischargeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_E_DEPInfo, Source.JX_JA_E_DEPInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_E_ARVInfo, Source.JX_JB_E_ARVInfo));

			var vessel = Source.Vessel;
			if (vessel != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_VesselNameInfo, vessel.RV_CodeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_LloydsNumberInfo, vessel.RV_LloydsNumberInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_RadioCallSignInfo, vessel.RV_RadioCallSignInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_RN_NKConveyanceNationalityInfo, vessel.RV_RN_NKCountryOfRegInfo));
			}

			var voyage = Source.Voyage;

			if (voyage != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_VoyageInfo, voyage.JV_VoyageFlightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_VesselNameInfo, voyage.JV_RV_NKVesselInfo));

				var line = voyage.Line;
				if (line != null)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.AMA_OA_CarrierInfo, () => line.MainAddress?.PK, () => new[] { voyage.JV_OH_LineInfo }));
				}
			}
		}
	}
}
