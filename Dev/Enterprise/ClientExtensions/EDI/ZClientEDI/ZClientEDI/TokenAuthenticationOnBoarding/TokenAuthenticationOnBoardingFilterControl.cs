using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding
{
	public partial class TokenAuthenticationOnBoardingFilterControl : ZFilterStripControl
	{
		public TokenAuthenticationOnBoardingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
