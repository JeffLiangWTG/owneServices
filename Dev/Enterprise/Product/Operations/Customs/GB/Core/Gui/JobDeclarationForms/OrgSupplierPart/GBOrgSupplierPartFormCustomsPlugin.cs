using System.Windows.Forms;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class GBOrgSupplierPartFormCustomsPlugin : EUOrgSupplierPartFormCustomsPlugin
	{
		public GBOrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new GBOrgSupplierPartFormCustomsControl();
		}
	}
}
