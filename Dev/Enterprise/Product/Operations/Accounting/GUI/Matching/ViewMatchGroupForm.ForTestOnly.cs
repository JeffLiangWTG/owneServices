#if DEBUG

using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ViewMatchGroupForm
	{
		public ContinueWithDelete ShowPreDeleteDialogs_ForTestOnly()
		{
			return ShowPreDeleteDialogs();
		}

		public ZPostingButtonsUserControl OPostingButtonsUserControl1_ForTestOnly
		{
			get { return oPostingButtonsUserControl1; }
			set { oPostingButtonsUserControl1 = value; }
		}

		public ZDateEdit UnmatchDateEdit_ForTestOnly
		{
			get { return UnmatchDateEdit; }
			set { UnmatchDateEdit = value; }
		}
	}
}

#endif
