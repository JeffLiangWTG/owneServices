#if DEBUG

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimUserControl
	{
		public ZTextBox DetailsTextBox_ForTestOnly
		{
			get { return DetailsTextBox; }
			set { DetailsTextBox = value; }
		}

		public ZGroupBox IntercompanyClaimDetailsGroupBox_ForTestOnly
		{
			get { return IntercompanyClaimDetailsGroupBox; }
			set { IntercompanyClaimDetailsGroupBox = value; }
		}

		public Business.AccQueryClaims.AccQueryClaimBase Claim_ForTestOnly => Claim;

		public void ShowAddLogCommentPopupForm_ForTestOnly()
		{
			ShowAddLogCommentPopupForm();
		}

		public ZTextBox MawbTextBox_ForTestOnly
		{
			get { return mawbTextBox; }
			set { mawbTextBox = value; }
		}
	}
}

#endif
