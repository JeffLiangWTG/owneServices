using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class SpecialMentionResponseData
	{
		public ZString Description { get; set; }
		public ZString TypeCode { get; set; }
		public ZBool NctsExportFromEC { get; set; }
		public ZString NctsExportFromCountry { get; set; }
	}
}
