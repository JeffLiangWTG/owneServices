namespace Enterprise.VisualBoards.Business
{
	public static class VisualBoardThreadHelper
	{
		public static string GetVisualBoardFormThreadName(string boardName)
		{
			return VisualBoardFormThreadNamePrefix + ": " + boardName;
		}

		public const string VisualBoardFormThreadNamePrefix = "VisualBoardForm";
	}
}
