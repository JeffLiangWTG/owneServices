namespace CargoWise.EntityFramework
{
	public static class ActionCounter
	{
		public static void IncreaseActionCount()
		{
			if (ActionCount < int.MaxValue)
			{
				ActionCount += 1;
			}
			else
			{
				ActionCount = 1;
			}
		}

		public static int ActionCount { get; private set; }
	}
}
