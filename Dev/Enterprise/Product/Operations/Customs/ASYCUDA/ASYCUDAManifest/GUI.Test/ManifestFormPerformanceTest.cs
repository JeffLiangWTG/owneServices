using Enterprise.Customs.ASYCUDAManifest.Business;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	sealed class ManifestFormPerformanceTest : ASYCUDA.GUI.Testing.ManifestFormPerformanceTest<AsycudaManifestHeader, AsycudaContainer, AsycudaBill, AsycudaPack, ASYCUDA.Business.AsycudaPackedItem, ASYCUDA.Business.ABLEntryNum, ASYCUDA.Business.AsycudaPackedItemEntryNum>
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Fiji;
	}
}
