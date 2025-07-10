using System;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	class WebPrintNotificationGroupRegistryItem : GuidDbRegistryItem
	{
		public override string ItemName
		{
			get { return "WebPrintNotificationGroup"; }
		}

		protected override Guid DefaultValue
		{
			get { return new Guid("94755E71-A87A-4034-8DFA-785773A49607"); }
		}
	}
}
