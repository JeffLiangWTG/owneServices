#if DEBUG

using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.GUI.Base
{
	public partial class MultipleReversingForHeaderForm
	{
		public ResourceStringData RemoveErrorTransactionsButtonCaption_ForTestOnly => RemoveErrorTransactionsButtonCaption;

		public ZArchitecture.ZGrid Grid_ForTestOnly
		{
			get { return Grid; }
			set { Grid = value; }
		}

		public Business.Base.Reversing.MultipleReversingProviderForHeader MultipleReversingProvider_ForTestOnly => MultipleReversingProvider;

		public bool ReOpenClosedJob_ForTestOnly()
		{
			return ReOpenClosedJob();
		}

		public void HandleApplyPostingButtonClickUnsafe_ForTestOnly(bool closeOnSave)
		{
			HandleApplyPostingButtonClickUnsafe(closeOnSave);
		}
	}
}

#endif
