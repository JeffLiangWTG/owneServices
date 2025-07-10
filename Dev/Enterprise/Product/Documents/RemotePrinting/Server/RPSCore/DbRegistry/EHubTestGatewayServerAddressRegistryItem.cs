namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class EHubTestGatewayServerAddressRegistryItem : StringDbRegistryItem
	{
		public override string ItemName => "EHubTestGatewayServerAddress";

		protected override string DefaultValue => "ehub-ausyd-test.wisegrid.net";
	}
}
