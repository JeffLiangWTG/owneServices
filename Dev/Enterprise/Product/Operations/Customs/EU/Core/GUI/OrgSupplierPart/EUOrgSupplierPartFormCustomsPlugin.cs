using System.Windows.Forms;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class EUOrgSupplierPartFormCustomsPlugin : OrgSupplierPartFormCustomsPlugin
	{
		public EUOrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new EUOrgSupplierPartFormCustomsControl();
		}
	}
}
