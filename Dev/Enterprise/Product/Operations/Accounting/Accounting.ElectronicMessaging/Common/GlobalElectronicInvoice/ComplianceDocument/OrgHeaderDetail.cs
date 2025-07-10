using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class OrgHeaderDetail
	{
		public ZString Category { get; set; }
		public ZString CompanyName { get; set; }
		public ZString VATRegistrationNum { get; set; }
		public ZString MCIRegistrationNum { get; set; }
		public ZString PIGRegistrationNum { get; set; }
		public ZString CountryCode { get; set; }
	}
}
