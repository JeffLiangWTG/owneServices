using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaManifestHeaderLookups : AutoAsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AutoAsycudaManifestHeader parent) : base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public RefVesselCollection Vessels
		{
			get
			{
				var result = new RefVesselCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.VesselName, "Property", Parent.AMA_VesselName));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.LloydsNumber, "Property", Parent.AMA_LloydsNumber));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.RadioCallSign, "Property", Parent.AMA_RadioCallSign));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.CountryOfRegistration, "Property", Parent.AMA_RN_NKConveyanceNationality));
				return result;
			}
		}
	}
}

