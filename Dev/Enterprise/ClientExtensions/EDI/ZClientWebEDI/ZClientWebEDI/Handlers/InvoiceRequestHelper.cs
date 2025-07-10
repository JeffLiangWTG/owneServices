using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class InvoiceRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "InvoiceRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return true; }
		}
	}
}