using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	public partial class PrintTaskDeliveryForm : ZChildForm
	{
		public PrintTaskDeliveryForm(PrintTaskSettings taskInstructions)
			: base(taskInstructions)
		{
			InitializeComponent();
			SetupEvents();

			printTaskDeliveryButtonsControl.AllowOverlap(printTaskSettingsControl);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void SetupEvents()
		{
			printTaskDeliveryButtonsControl.ShowErrorsEvent += new PrintTaskDeliveryButtonsControl.ShowErrorsEventHandler(ShowErrorsDialogRequested);
		}

		void ShowErrorsDialogRequested(object sender)
		{
			ShowErrorsDialog();
		}
	}
}
