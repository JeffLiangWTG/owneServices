using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture;
using Res = Enterprise.Messaging.Business.Res;

namespace Enterprise.BatchProcessor
{
	public abstract class EmailReaderBatchProcess : BatchProcess
	{
		protected EmailReaderBatchProcess(NotificationBuffer buffer)
		{
			Factory = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			Buffer = buffer;
		}

		public readonly NotificationBuffer Buffer;

		protected override void Execute(CancellationToken token)
		{
			var processor = new MailBatchProcessor(MailFilter, (mail, index) =>
			{
				ProcessMailItem(mail);
				Buffer.Notify(new InfoNotification(Res.GetString("8B7009AC-9239-459D-A519-AC9F47E02A50", "Processing New Email, From : {0}, Subject : {1}", mail.MI_From, mail.MI_Subject)));
				return MailProcessingResult.MarkSuccess;
			}, 10);

			processor.Process(Buffer, token);
		}

#if DEBUG
		protected internal void ExecuteInternal() => Execute(CancellationToken.None);
#endif

		#region MailFilter

		protected abstract internal IMailFilter MailFilter { get; }

		#endregion

		protected abstract void ProcessMailItem(MailItem mail);
		readonly BusinessObjectFactory Factory;
	}
}
