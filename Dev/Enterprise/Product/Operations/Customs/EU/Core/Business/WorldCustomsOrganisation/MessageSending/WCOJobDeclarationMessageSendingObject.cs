using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending
{
	public class WCOJobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public new CusEntryHeader Header => base.Header;

		public WCOJobDeclarationMessageSendingObject(CusEntryHeader header)
			: base(header)
		{
		}

		public AmendmentDetails AmendmentDetails { get; set; }
	}
}
