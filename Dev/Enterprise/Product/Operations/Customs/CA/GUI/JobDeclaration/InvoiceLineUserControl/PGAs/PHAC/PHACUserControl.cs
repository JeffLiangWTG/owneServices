using System.ComponentModel;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("PHACPGAHeader")]
	public partial class PHACUserControl : ZUserControl
	{
		public PHACUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			LPCOGridUserControl.RemoveExceptAvailableColumns(PHACPGAHeader.AvailableLPCOFields);
			if (!isOnInvoiceLine)
			{
				MainGroupBox1.Controls.Remove(UNDGGuidFindBox);
			}
		}
	}
}
