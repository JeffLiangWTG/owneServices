
using CargoWise.Types;

namespace Enterprise.Client.STI.Navision
{
	public abstract class JobBatchListener : NavisionBatchListener
	{
		protected JobBatchListener(ZDateTime dateTimeExportStarted) : base(dateTimeExportStarted)
		{
		}

		protected override ZString ExportDirectory
		{
			get { return STIDataRegistry.Instance.ShipmentExportDirectory; }
		}

		protected override ZString FileNamePreFix
		{
			get { return Constants.FileNamePrefixes.ShipmentFiles; }
		}
	}
}
