using System.Windows.Forms;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class NctsPlugin : EU.NCTS.GUI.NctsPlugin
{
	public NctsPlugin(ICusInBondParent host) : base(host)
	{
	}

	protected override Control GetNewUserControl() => new Phase5NctsUserControlForPlugin(InternalInBond);
}
