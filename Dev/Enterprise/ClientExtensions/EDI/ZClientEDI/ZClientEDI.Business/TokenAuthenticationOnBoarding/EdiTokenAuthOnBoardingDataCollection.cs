using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business
{
	public class EdiTokenAuthOnBoardingDataCollection : ActiveBusinessObjectCollection<EdiTokenAuthOnBoardingData>
	{
		public EdiTokenAuthOnBoardingDataCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiTokenAuthOnBoardingDataCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
