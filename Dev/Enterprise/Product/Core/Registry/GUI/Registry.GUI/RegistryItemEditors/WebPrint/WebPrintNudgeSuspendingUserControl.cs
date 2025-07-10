using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class WebPrintNudgeSuspendingUserControl : RegistryZUserControl
	{
		public WebPrintNudgeSuspendingUserControl(WebPrintNudgeSuspendingWrapper wrapper)
		{
			InitializeComponent();
			NudgeSuspendingWrapper = wrapper;
		}

		public WebPrintNudgeSuspendingWrapper NudgeSuspendingWrapper { get; }

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			OptionGroupBox.Enabled = !readOnly;
		}

		public void SetBusinessEntityValue(WebPrintNudgeSuspending nudgeSuspending)
		{
			NudgeSuspendingWrapper.NudgeSuspending = nudgeSuspending;
			SetDataBinding(NudgeSuspendingWrapper, "");
		}
	}
}
