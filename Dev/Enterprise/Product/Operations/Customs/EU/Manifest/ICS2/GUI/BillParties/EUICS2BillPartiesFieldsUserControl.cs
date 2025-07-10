using System.ComponentModel;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class EUICS2BillPartiesFieldsUserControl : ZUserControl
	{
		public EUICS2BillPartiesFieldsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var newDataSource = dataSource as AsycudaBill;
			base.SetDataBinding(newDataSource, "");
		}

		void ConvertSellerToOrganizationButton_Click(object sender, System.EventArgs e)
		{
			SellerAddressControl.ShowEditOrViewForm();
		}
	}
}
