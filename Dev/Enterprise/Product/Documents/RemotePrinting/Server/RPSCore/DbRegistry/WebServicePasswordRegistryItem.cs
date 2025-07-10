
namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class WebServicePasswordRegistryItem : StringDbRegistryItem
	{
		public override string ItemName
		{
			get { return "WebServicePassword"; }
		}

		protected override string DefaultValue
		{
			get { return ""; }
		}
	}
}
