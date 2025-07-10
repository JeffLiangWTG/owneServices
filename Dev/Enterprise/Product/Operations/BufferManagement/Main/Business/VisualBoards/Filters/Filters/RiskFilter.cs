using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class RiskFilter : BoardMeetingModeFilter
	{
		public RiskFilter()
		{
			FilterName = Name;
		}

		public static string Name => Res.GetString("58feaf5e-4552-4181-94ec-d2e71dc5e4c7", "Risk Filter");
	}
}
