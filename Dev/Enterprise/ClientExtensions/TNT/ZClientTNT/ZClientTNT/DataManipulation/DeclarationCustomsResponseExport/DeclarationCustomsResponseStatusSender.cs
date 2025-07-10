using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT
{
	[Serializable]
	class DeclarationCustomsResponseStatusSender : TNTReturnExitStatus
	{
		public DeclarationCustomsResponseStatusSender() : base(true) { }
		protected override StringRegistryItem GetReplyDirectoryCore()
		{
			return TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw;
		}
	}
}
