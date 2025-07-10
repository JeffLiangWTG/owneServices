namespace Enterprise.BufferManagement.Business
{
	public sealed class BufferStatus
	{
		public BufferStatus(ConstraintStatus constraintStatus, decimal bufferPenetration, int bufferZone)
		{
			ConstraintStatus = constraintStatus;
			BufferPenetration = bufferPenetration;
			BufferZone = bufferZone;
		}

		public ConstraintStatus ConstraintStatus { get; private set; }
		public decimal BufferPenetration { get; private set; }
		public int BufferZone { get; private set; }
	}
}
