using System.Windows.Forms;

namespace Enterprise.Customs.KR.GUI
{
	public class OrgSupplierPartFormCustomsPluginGlobal : Customs.GUI.OrgSupplierPartFormCustomsPluginGlobal
	{
		public OrgSupplierPartFormCustomsPluginGlobal(Business.OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return userControl = new OrgSupplierPartFormCustomsControlGlobal();
		}
	}
}
