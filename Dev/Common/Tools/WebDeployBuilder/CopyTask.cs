namespace WebDeployBuilder
{
	public struct CopyTask
	{
		public string Source;
		public string Destination;

		public CopyTask(string source, string destination)
		{
			Source = source;
			Destination = destination;
		}
	}
}
