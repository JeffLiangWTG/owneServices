namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum OutturnStatus
	{
		ReadyForScanning,
		AwaitingResponseFromCustoms,
		Sent
	}

	public enum UnderbondStatus
	{
		NotSend,
		PartiallySent,
		FullySent,
		WaitingCustomsResponse,
		CompletedOnEarlierUnderbond
	}
}
