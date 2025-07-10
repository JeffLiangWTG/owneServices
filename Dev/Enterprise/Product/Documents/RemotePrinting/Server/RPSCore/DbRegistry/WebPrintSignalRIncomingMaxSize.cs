namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class WebPrintSignalRIncomingMaxSize : IntDbRegistryItem
	{
		public const int DefaultMaxSize = 100;

		protected override int DefaultValue => DefaultMaxSize;

		public override string ItemName => "WebPrintSignalRIncomingMaxSize";
	}
}
