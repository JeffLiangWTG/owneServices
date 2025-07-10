using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public class IDNumberAndType
	{
		public ZString Type { get; set; }
		public ZString Number { get; set; }
		public ZString CountryOfIssue { get; set; }
	}
}
