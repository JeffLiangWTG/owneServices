using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDISalesInquiryForm : SalesEnquiryForm
	{
		public EDISalesInquiryForm(EDISalesInquiry inquiry)
			: base(inquiry)
		{
			AddSalesDiscoveryMenuItem();
		}

		#region Actions Menu

		void AddSalesDiscoveryMenuItem()
		{
			if (BusinessEntity != null)
			{
				ActionsMenuItem.MenuItems.Add(SalesDiscoveryConductorUrlHelper.SalesMenuItem(BusinessEntity));
			}
		}

		#endregion
	}
}
