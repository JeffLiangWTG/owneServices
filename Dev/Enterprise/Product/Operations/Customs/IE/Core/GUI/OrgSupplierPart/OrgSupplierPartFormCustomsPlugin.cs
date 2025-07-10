using System.Windows.Forms;
using Enterprise.Customs.IE.Business.MasterFiles;

namespace Enterprise.Customs.IE.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : EU.GUI.EUOrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part) : base(part)
		{
		}

		protected override Control GetNewUserControl() => new OrgSupplierPartFormCustomsControl();
	}
}
