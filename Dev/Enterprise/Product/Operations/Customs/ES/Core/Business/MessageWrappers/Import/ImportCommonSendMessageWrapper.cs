using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class ImportCommonSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IImportCommonDataProvider
	{
		public ImportCommonSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public ZString MRN => MRNCore;

		protected virtual ZString MRNCore => entryHeader.MovementReferenceNumber;
	}
}
