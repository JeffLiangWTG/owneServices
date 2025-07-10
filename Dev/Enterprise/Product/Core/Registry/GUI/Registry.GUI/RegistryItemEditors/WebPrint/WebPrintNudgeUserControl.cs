using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class WebPrintNudgeUserControl : RegistryZUserControl
	{
		public WebPrintNudgeUserControl(WebPrintNudgeWrapper wrapper)
		{
			InitializeComponent();
			NudgeWrapper = wrapper;
		}

		public WebPrintNudgeWrapper NudgeWrapper { get; }

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			OptionGroupBox.Enabled = !readOnly;
			SwtichBackToIPAddressIntervalInHoursCalcEdit.Enabled = !readOnly;
		}

		public void SetBusinessEntityValue(WebPrintNudge nudge)
		{
			NudgeWrapper.Nudge = nudge;
			SetDataBinding(NudgeWrapper, "");

			EnableIPAddressRadioButton.Checked = nudge.EnableIPAddress;
			EnableURLAddressRadioButton.Checked = !nudge.EnableIPAddress;
		}
	}
}
