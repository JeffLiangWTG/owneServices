using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.GUI
{
	public partial class REXDISMessagesUserControl : MessagesUserControl
	{
		public REXDISMessagesUserControl()
		{
			InitializeComponent();
			MessagesGrid.ColumnLayoutContext = TemporaryStorageApplicationCodeList.Codes.REX;
		}
	}
}
