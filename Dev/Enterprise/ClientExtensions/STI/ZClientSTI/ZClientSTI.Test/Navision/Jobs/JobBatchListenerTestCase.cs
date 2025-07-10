namespace Enterprise.Client.STI.Navision.Testing
{
	public abstract class JobBatchListenerTestCase : NavisionBatchListenerTestCase
	{
		protected override string ExportDirectory
		{
			get
			{
				return STIDataRegistry.Instance.ShipmentExportDirectory;
			}

			set
			{
				STIDataRegistry.Instance.ShipmentExportDirectory = value;
			}
		}

		protected override string FileNamePreFix
		{
			get
			{
				return Constants.FileNamePrefixes.ShipmentFiles;
			}
		}
	}
}
