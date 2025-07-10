#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AssignClaimPopupForm
	{
		public ZButton OKButton_ForTestOnly
		{
			get { return OKButton; }
			set { OKButton = value; }
		}

		public ZButton CancelButtonX_ForTestOnly
		{
			get { return CancelButtonX; }
			set { CancelButtonX = value; }
		}
	}
}

#endif
