namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class FreightNotesRegistryItem : StringDbRegistryItem
	{
		public override string ItemName => "FreightNotesRegistration";

		protected override string DefaultValue => string.Empty;
	}
}
