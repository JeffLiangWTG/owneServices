namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class WebServiceUsernameRegistryItem : StringDbRegistryItem
	{
		public override string ItemName
		{
			get { return "WebServiceUsername"; }
		}

		protected override string DefaultValue
		{
			get { return ""; }
		}
	}
}
