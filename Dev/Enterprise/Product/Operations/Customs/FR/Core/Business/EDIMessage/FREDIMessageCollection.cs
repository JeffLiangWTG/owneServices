using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FREDIMessageCollection : EDIMessageCollection
	{
		public FREDIMessageCollection(BusinessObject master)
			: base(master)
		{
		}

		public new FREDIMessage this[int index]
		{
			get { return (FREDIMessage)base[index]; }
		}

		public new FREDIMessage AddNew()
		{
			return (FREDIMessage)base.AddNew();
		}
		public new FREDIMessage AddNew(Type bizOType)
		{
			return (FREDIMessage)base.AddNew(bizOType);
		}
		public FREDIMessage LastIncomingMessageWithValuedEntryStatus
		{
			get
			{
				var receivedMsgs = GetMatchingMessages(ZString.Empty, Array.Empty<ZString>(), EDIMessage.Direction.Receive);
				return receivedMsgs.Cast<FREDIMessage>().FirstOrDefault(x => !EntryActionHelper.GetEntryStatus(x).IsEmpty && (!(x.MessageDataObject as IResponseDataProvider)?.HasErrors ?? false));
			}
		}
	}
}
