using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	public partial class DeliveryInstructionsInfoControl : ZUserControl
	{
		public DeliveryInstructionsInfoControl()
		{
			InitializeComponent();
		}

		public ZGrid RecipientsGrid
		{
			get { return recipientsGrid; }
		}

		public ZGrid DocumentsGrid
		{
			get { return documentsGrid; }
		}
	}
}
