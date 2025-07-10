using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class PreviousDocumentResponseData
	{
		public ZString TypeCode { get; set; }
		public ZString Reference { get; set; }
		public ZString MoreInfo { get; set; }
	}
}
