using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public class NctsPlugin : EU.NCTS.GUI.NctsPlugin
	{
		public NctsPlugin(ICusInBondParent host) : base(host)
		{
		}

		protected override Control GetNewUserControl() => new Phase5NctsUserControlForPlugin(InternalInBond);

		protected override void SetNCTSPhaseIfNeeded(NctsHeader header)
		{
			header.BH_ApplicationCode = ApplicationCode;
		}

		protected override string ApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;
	}
}
