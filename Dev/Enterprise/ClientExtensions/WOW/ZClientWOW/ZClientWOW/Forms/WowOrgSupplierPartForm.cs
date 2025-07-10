
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.Wow
{
	public partial class WowOrgSupplierPartForm : OrgSupplierPartForm
	{
		public WowOrgSupplierPartForm()
		{
			InitializeComponent();
		}

		public WowOrgSupplierPartForm(OrgSupplierPart part)
			: base(part)
		{
			InitializeComponent();
		}
	}
}
