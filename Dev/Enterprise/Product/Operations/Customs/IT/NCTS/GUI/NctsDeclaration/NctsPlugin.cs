using System.Windows.Forms;
namespace Enterprise.Customs.IT.NCTS.GUI;

public class NctsPlugin : EU.NCTS.GUI.NctsPlugin
{
	public NctsPlugin(Freight.Integration.ICusInBondParent host) : base(host)
	{
	}

	protected override Control GetNewUserControl() => InternalInBond.IsPhase5 ? new Phase5NctsUserControlForPlugin(InternalInBond) : new NctsUserControlForPlugin(InternalInBond);
}
