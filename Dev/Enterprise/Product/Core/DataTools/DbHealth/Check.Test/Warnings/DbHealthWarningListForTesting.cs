namespace Enterprise.DbHealth.Check
{
	public sealed class DbHealthWarningListForTesting : DbHealthWarningList
	{
		public void AddDummyWarning(string source, string warningType, string description, string action)
		{
			DummyWarning warning = new DummyWarning(source, warningType, description, action);
			Add(warning);
		}

		public void ClearList()
		{
			Clear();
		}
	}
}
