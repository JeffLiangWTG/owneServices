namespace Enterprise.DbHealth.IndexUpdate
{
	public enum ObserverAction
	{
		None,
		Cancel,
		Requeue,
		KillBlockers,
	}
}
