using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class PortProvider : IPort
	{
		public string UNLocode { get; set; }
		public string Country { get; set; }
		public string Location { get; set; }
	}
}
