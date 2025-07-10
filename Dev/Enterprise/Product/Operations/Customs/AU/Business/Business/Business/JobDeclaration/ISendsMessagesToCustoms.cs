
using System;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISendsMessagesToCustoms : Customs.Business.ISendsMessagesToCustoms
	{
		bool ContinueWithDoCPDecsAndRemergeEntry();
		EXIT1MessageType GetExit1MessageType();
	}

	public class SendsMessagesToCustomsShutterUpperer : Customs.Business.SendsMessagesToCustomsShutterUpperer, ISendsMessagesToCustoms
	{
		public SendsMessagesToCustomsShutterUpperer()
			: base()
		{
		}

		public SendsMessagesToCustomsShutterUpperer(bool throwExceptionOnError)
			: base(throwExceptionOnError)
		{
		}

		public new bool ContinueWithAction(string message, string caption)
		{
			OnContinueWithAction?.Invoke(this, new WarningEventArgs(caption, message));
			return base.ContinueWithAction(message, caption);
		}

		public event EventHandler<WarningEventArgs> OnContinueWithAction;

		#region ISendsMessagesToCustoms Members

		public bool ContinueWithDoCPDecsAndRemergeEntry()
		{
			ContinueWithDoCPDecsAndRemergeEntryAsked = true;
			return true;
		}

		public EXIT1MessageType GetExit1MessageType()
		{
			return new EXIT1MessageType();
		}

		public bool ContinueWithDoCPDecsAndRemergeEntryAsked;

		#endregion
	}
}
