namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public enum DeliveryInstructionDestination
	{
		Print,
		Sms,
		Auto,
		None,
		Preview,
		DocConfigPreview,
		Disk,
		DocManager,
		UserCancelled,
		TakenFromContact,
		Memory,
#if DEBUG
		DummyDestinationForTesting
#endif
	}
}
