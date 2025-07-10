using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class MyAccountHostingSiteLandingPageUrlLookups : ZLookups
	{
		public MyAccountHostingSiteLandingPageUrlLookups(BusinessObject parent) : base(parent)
		{
		}

		public ProductTypes ProductTypes
		{
			get { return lookupsProductTypes ?? (lookupsProductTypes = new ProductTypes(true)); }
		}

		ProductTypes lookupsProductTypes;
	}
}
