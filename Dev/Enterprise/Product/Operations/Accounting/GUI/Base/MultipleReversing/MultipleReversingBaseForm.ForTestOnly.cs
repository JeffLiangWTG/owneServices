#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.Base
{
	public partial class MultipleReversingBaseForm
	{
		public void RemoveErrorTransactionsButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			RemoveErrorTransactionsButton_Click(sender, e);
		}

		public Core.Forms.ZPostingButtonsUserControl PostingButtons_ForTestOnly
		{
			get { return PostingButtons; }
			set { PostingButtons = value; }
		}

		public void HandleSaveException_ForTestOnly(Exception ex)
		{
			HandleSaveException(ex);
		}
	}
}

#endif
