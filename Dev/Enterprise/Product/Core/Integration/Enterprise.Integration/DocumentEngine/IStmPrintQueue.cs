using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IStmPrintQueue
	{
		ZString QueueName { get; set; }
		ZGuid PK { get; }
		ZBool SQ_AllowPrinting { get; set; }
		ZString SQ_DisplayName { get; set; }
		ZString SQ_ServerName { get; set; }
		ZGuid SQ_SPS_Server { get; set; }
	}
}
