//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class DuimpCompleteConsultMessageSender : BaseConsultMessageSender
	{
		public DuimpCompleteConsultMessageSender(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override IMessageSendingObject GetMessageSendingObject() => new DuimpMessageSendingObject(EntryHeader);

		public override string CanSendMessage => EntryHeader.CH_AuthorityVersion.IsEmpty || EntryHeader.CH_AuthorityVersion.Equals("0") ? Res.GetString("5D0DD835-2660-4D60-B515-48ED9724B416", "The selected entry cannot be consulted. The entry version is zero or empty.") : null;
	}
}
