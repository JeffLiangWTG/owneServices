using System.Windows.Forms;
using Enterprise.Customs.FR.Business.MasterFiles;

namespace Enterprise.Customs.FR.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : EU.GUI.EUOrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new OrgSupplierPartFormCustomsControl();
		}
	}
}
