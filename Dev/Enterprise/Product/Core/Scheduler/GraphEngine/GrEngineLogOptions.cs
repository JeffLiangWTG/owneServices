namespace Enterprise.Scheduler.GraphEngine
{
	public class GrEngineLogOptions
	{
		public bool FirstEnqueueLoad { get; set; }
		public bool MessageAtFront { get; set; }
		public bool ChainStatistics { get; set; }
		public bool OldestMessage { get; set; }
		public bool EnqueueLoads { get; set; }
		public bool AllKeygenLoads { get; set; }
		public bool AllKeygenLocks { get; set; }
		public bool AllWorkerLoads { get; set; }
		public bool AllWorkerLocks { get; set; }
		public bool SetChainID { get; set; }
	}
}
