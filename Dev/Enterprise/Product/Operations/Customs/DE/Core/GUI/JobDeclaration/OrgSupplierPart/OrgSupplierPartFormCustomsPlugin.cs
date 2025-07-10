using System.Windows.Forms;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.GUI
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
