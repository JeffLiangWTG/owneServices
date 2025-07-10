using System.Windows.Forms;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

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
