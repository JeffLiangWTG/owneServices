
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public enum ExportType
	{
		File,
		Email,
		Ftp
	}

	public abstract class ExportMethod
	{
		protected ExportMethod(ExportInstructions instructions, INotifications notifications)
		{
			this.Instructions = instructions;
			this.Notifications = notifications;
		}

		public readonly ExportInstructions Instructions;
		public abstract ExportType ExportType { get; }
		public abstract void Deliver(ZString savedExportFile);
		public abstract bool CanDeliver { get; }
		protected readonly INotifications Notifications;
	}
}
