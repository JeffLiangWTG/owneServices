namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class EhubGatewayServerAddressRegistryItem : StringDbRegistryItem
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string ItemName => "Ehubgatewayserveraddress";

		protected override string DefaultValue => "ehubgateway.wisegrid.net";
	}
}
