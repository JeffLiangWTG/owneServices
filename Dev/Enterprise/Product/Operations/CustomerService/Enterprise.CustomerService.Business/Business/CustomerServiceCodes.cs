using Enterprise.CustomerService.Integration;

namespace Enterprise.CustomerService.Business
{
	public class CustomerServiceCodes : ICustomerServiceCodes
	{
		public CustomerServiceCodes()
		{
		}

		string ICustomerServiceCodes.ReopenClosedGLPeriodCode
		{
			get { return Cr9ModuleList.Codes.ReopenClosedGlPeriod; }
		}
	}
}
