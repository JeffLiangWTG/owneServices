#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class BasePostManager
	{
		public bool FCancelPosting_ForTestOnly
		{
			get { return fCancelPosting; }
			set { fCancelPosting = value; }
		}
	}
}

#endif
