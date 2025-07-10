using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.GUI
{
	public class ExitSummaryPlugIn : EU.GUI.PlugIn.ExitSummaryPlugIn
	{
		public ExitSummaryPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControl() => new ExitSummaryUserControl();
	}
}
