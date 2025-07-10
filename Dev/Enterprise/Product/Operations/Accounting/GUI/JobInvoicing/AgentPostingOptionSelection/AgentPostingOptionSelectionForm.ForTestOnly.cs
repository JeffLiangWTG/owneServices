#if DEBUG

namespace Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection
{
	public partial class AgentPostingOptionSelectionForm
	{
		public ZArchitecture.GUI.ZButton ContinueButton_ForTestOnly
		{
			get { return ContinueButton; }
			set { ContinueButton = value; }
		}

		public ZArchitecture.GUI.ZButton CancelPostingButton_ForTestOnly
		{
			get { return CancelPostingButton; }
			set { CancelPostingButton = value; }
		}
	}
}

#endif
