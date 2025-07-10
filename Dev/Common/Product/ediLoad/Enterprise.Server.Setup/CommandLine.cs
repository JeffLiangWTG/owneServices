namespace Enterprise.Server.Setup
{
	sealed class CommandLine
	{
		public CommandLine(string fullCommandLine)
		{
			if (fullCommandLine.StartsWith("\""))
			{
				ExecutableName = fullCommandLine.Substring(1, fullCommandLine.IndexOf('"', 1) - 1);
			}
			else
			{
				int spaceIndex = fullCommandLine.IndexOf(' ');
				if (spaceIndex >= 0)
				{
					ExecutableName = fullCommandLine.Substring(0, spaceIndex);
				}
				else
				{
					ExecutableName = fullCommandLine;
				}
			}
		}

		public readonly string ExecutableName;
	}
}
