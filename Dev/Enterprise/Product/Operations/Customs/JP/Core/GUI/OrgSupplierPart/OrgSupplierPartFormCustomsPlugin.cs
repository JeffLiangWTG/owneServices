using System.Windows.Forms;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : Customs.GUI.OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new OrgSupplierPartFormCustomsControlGlobal();
		}
	}
}
