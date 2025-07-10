using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI
{
	public class ReportGrid : ZGrid
	{
		public ReportGrid()
		{
		}

		protected override bool IsDeleteMenuItemVisible
		{
			get
			{
				return false;
			}
		}
	}
}
