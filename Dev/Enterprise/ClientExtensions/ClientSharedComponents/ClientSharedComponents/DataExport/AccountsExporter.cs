using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.ZArchitecture;

namespace Enterprise.ClientSharedComponents
{
	public abstract class AccountsExporter : FlatFileAccountingTransactionExporter
	{
		protected AccountsExporter(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory)
		{
			this.notifications = notifications;
		}

		public virtual ZString FileName
		{
			get
			{
				return FileNamePrefix + "_" + ZDateTime.Now.ToString(fileNameDateFormat) + "_" +
					new ZString(FilterProvider.CurrentBatchNo.ToString()).Right(4).PadLeft(4, '0') + "." +
					Format.FileExtensionForExport.ToString().ToLower();
			}
		}

		public abstract ZString FileNamePrefix { get; }
		protected NotificationBuffer notifications;
		const string fileNameDateFormat = "yyyyMMddhhmmss";
	}
}
