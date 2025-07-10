using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.GUI
{
	public partial class SumAMessagesUserControl : MessagesUserControl
	{
		public SumAMessagesUserControl()
		{
			InitializeComponent();
			MessagesGrid.ColumnLayoutContext = TemporaryStorageApplicationCodeList.Codes.SumA;
		}
	}
}
