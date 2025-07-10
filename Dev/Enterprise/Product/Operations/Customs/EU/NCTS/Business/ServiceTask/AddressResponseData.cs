using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class AddressResponseData
	{
		public ZString CompanyName { get; set; }
		public ZString Address1 { get; set; }
		public ZString Postcode { get; set; }
		public ZString City { get; set; }
		public ZString CountryCode { get; set; }
		public ZString GovRegNum { get; set; }
		public ZString AuthorisedNumber { get; set; }
	}
}
