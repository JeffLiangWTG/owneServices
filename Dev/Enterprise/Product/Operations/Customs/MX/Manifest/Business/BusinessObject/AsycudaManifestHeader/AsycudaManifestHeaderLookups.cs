using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override ICollection GetCustomsDischargePortListCore()
		{
			return (Parent.AMA_RL_NKPortOfDischarge.IsEmpty && Parent.IsImport) || Parent.AMA_RL_NKPortOfDischarge.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Mexico
				? base.GetCustomsDischargePortListCore()
				: ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
		}

		protected override ICollection GetCustomsLoadingPortListCore()
		{
			return (Parent.AMA_RL_NKPortOfLoading.IsEmpty && !Parent.IsImport) || Parent.AMA_RL_NKPortOfLoading.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Mexico
				? base.GetCustomsLoadingPortListCore()
				: ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
		}
	}
}
