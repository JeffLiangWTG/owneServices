using System.Collections;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class TransportMeanLookups : JobConsolTransportLookups
	{
		public TransportMeanLookups(TransportMean parent) : base(parent)
		{
		}

		public RefCountryCollection VehicleCountryList => countryList ??= new RefCountryCollection(Factory);
		RefCountryCollection countryList;

		public ICollection TruckKindList => TruckKindListCore;

		protected virtual ICollection TruckKindListCore => new CodeDescriptionPairList();
	}
}
