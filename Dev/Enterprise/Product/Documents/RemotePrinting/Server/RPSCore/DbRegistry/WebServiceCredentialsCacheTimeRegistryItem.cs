namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class WebServiceCredentialsCacheTimeRegistryItem : IntDbRegistryItem
	{
		public override string ItemName => "WebServiceCredentialsCacheTime";

		protected override int DefaultValue => 15;
	}
}
