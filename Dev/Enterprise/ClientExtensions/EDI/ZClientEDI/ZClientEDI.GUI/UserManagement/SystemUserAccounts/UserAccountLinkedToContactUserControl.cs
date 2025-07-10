using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class UserAccountLinkedToContactUserControl : ZUserControl
	{
		public UserAccountLinkedToContactUserControl(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection)
			: base()
		{
			InitializeComponent();
			SetDataBinding(ediCustomerUserAccountCollection, null);
		}
	}
}
