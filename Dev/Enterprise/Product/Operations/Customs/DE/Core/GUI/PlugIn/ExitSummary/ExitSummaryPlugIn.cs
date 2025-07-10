using System.Windows.Forms;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public class ExitSummaryPlugIn : EU.GUI.PlugIn.ExitSummaryPlugIn
	{
		public ExitSummaryPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControl() => new ExitSummaryUserControl();
	}
}
