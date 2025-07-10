using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class DVDCommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IDVDCommonDataProvider
	{
		public DVDCommonSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
		{
		}

		public ZString MRN => MRNCore;
		protected virtual ZString MRNCore => entryHeader.MovementReferenceNumber;
	}
}
