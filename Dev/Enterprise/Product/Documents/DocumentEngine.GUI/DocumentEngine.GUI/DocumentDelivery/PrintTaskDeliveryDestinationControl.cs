using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	public partial class PrintTaskDeliveryDestinationControl : ZUserControl
	{
		#region Constructors

		public PrintTaskDeliveryDestinationControl()
		{
			InitializeComponent();
		}

		#endregion

		#region Related Objects

		public PrintTaskSettings TaskSettings
		{
			get { return (PrintTaskSettings)CurrentDataItem; }
		}

		#endregion

	}
}
