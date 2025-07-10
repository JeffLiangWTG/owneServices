#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class AppendToUnpostedChargeDescriptionForm
	{
		public bool AllowNew_ForTestOnly => AllowNew;

		public Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl_ForTestOnly
		{
			get { return PostingButtonsUserControl; }
			set { PostingButtonsUserControl = value; }
		}
	}
}

#endif
