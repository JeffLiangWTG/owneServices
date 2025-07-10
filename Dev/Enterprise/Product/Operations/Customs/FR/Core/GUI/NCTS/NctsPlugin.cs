using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class NctsPlugin : EU.NCTS.GUI.NctsPlugin
	{
		public NctsPlugin(ICusInBondParent host) : base(host)
		{
		}

		protected override Control GetNewUserControl() => InternalInBond.IsPhase5 ? new Phase5NctsUserControlForPlugin(InternalInBond) : new NctsUserControlForPlugin(InternalInBond);
	}
}
