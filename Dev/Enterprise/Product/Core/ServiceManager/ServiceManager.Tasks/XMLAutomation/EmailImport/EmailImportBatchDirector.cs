using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class EmailImportBatchDirector : XMLDirector
	{
		public EmailImportBatchDirector(INotifications notifications) : base(notifications) { }

		protected override void RunCore(CancellationToken token)
		{
			ZString directory = SystemDataRegistry.Instance.LocalCartageDataImportDirectory.Value;
			if (!directory.IsEmpty && Directory.Exists(directory))
			{
				LocalCartageBookingEmailReader localCartageReader = GetNewEmailReader();
				localCartageReader.ExecuteBatch(token);
			}
		}

#if DEBUG
		protected virtual
#endif
 LocalCartageBookingEmailReader GetNewEmailReader()
		{
			return new LocalCartageBookingEmailReader(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, Notify);
		}
	}
}
