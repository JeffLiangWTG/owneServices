using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUOrgSupplierPartFormCustomsPlugin : OrgSupplierPartFormCustomsPlugin
	{
		public AUOrgSupplierPartFormCustomsPlugin(AUOrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl() => new AUOrgSupplierPartFormCustomsControl();
	}
}
