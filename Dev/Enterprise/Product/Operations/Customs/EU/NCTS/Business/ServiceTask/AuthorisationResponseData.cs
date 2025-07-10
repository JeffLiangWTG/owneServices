using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class AuthorisationResponseData
	{
		public ZString SequenceNumber { get; set; }

		public ZString Type { get; set; }

		public ZString ReferenceNumber { get; set; }
	}
}
