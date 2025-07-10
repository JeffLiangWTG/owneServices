using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class EmrReport : ErsReport, IRoutingProvider
	{
		public ZString CustomsReturnCode  //CRC
		{
			get { return customsReturnCode; }
			set
			{
				customsReturnCode = value;
				CustomsReturnCodeHuman = new CRC().GetDescriptionFromCode(value);
			}
		}
		ZString customsReturnCode;
		public ZString CustomsReturnCodeHuman { get; private set; }
		public ZString MasterRouteOfEntry { get; set; } //MASTER-ROE
		public ZString MasterStyleOfEntry { get; set; } //MASTER-SOE		

		ZString IRoutingProvider.StyleOfEntry
		{
			get { return MasterStyleOfEntry; }
		}

		ZString IRoutingProvider.RouteOfEntry
		{
			get { return MasterRouteOfEntry; }
		}
	}
}
