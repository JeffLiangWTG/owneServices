namespace Enterprise.RemotePrinting.Client.Setup.Custom
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var action = args[0];
			var installLocation = (args.Length > 1) ? args[1].Trim('"') : null;

			new CustomActionProcessor(action, installLocation).Process();
		}
	}
}
