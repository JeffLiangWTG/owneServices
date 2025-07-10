#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GLJournalController
	{
		public Business.Base.Reversing.ReversingBase FReversing_ForTestOnly
		{
			get { return fReversing; }
			set { fReversing = value; }
		}

		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}
	}
}

#endif