namespace Enterprise.ZArchitecture.Web.GUI
{
	public delegate void ZWebResourceExtractEventHandler(object sender, ZWebResourceExtractEventArgs e);

	public class ZWebResourceExtractEventArgs
	{
		public ZWebResourceExtractEventArgs(string diskFileName)
		{
			this.DiskFileName = diskFileName;
		}

		public readonly string DiskFileName;
	}
}
