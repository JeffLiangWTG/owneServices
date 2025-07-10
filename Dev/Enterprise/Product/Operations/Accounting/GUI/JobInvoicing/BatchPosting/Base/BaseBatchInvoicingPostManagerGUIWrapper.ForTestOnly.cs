#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class BaseBatchInvoicingPostManagerGUIWrapper
	{
		public void AppendMessageToStack_ForTestOnly(ZString message)
		{
			AppendMessageToStack(message);
		}

		public string GetNothingPostedMessage_ForTestOnly()
		{
			return GetNothingPostedMessage();
		}

		public ZString CurrentObjectMessageStack_ForTestOnly
		{
			get { return CurrentObjectMessageStack; }
			set { CurrentObjectMessageStack = value; }
		}

		public ZBool CurrentObjectPosted_ForTestOnly
		{
			get { return CurrentObjectPosted; }
			set { CurrentObjectPosted = value; }
		}
	}
}

#endif
