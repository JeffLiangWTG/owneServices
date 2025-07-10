using Enterprise.Client.EDI.TrustedMessaging.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public interface ITrustedSystemContext : ITrustedContext
	{
		public string SystemId { get; }
		public EdiTrustedSystem TrustedSystem { get; }
	}
}
