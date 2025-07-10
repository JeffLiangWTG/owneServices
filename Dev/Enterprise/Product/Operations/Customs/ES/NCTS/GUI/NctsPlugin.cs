using System.Windows.Forms;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class NctsPlugin : EU.NCTS.GUI.NctsPlugin
	{
		public NctsPlugin(ICusInBondParent host) : base(host)
		{
		}

		protected override Control GetNewUserControl() => InternalInBond.IsPhase5 ? new Phase5NctsUserControlForPlugin((NctsHeader)InternalInBond) : new NctsUserControlForPlugin(InternalInBond);
	}
}
