using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class TransportMeansLookups : CusCodeDataLookups
	{
		public TransportMeansLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		public RefVesselCollection RefVessels => new RefVesselCollection(Factory);
	}
}
