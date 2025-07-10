using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AnnexSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IAnnexMessageDataProvider
	{
		public AnnexSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, IeDoc doc, ZString docDescription, bool endOfAnnexes) : base(cusEntryHeader, certificateData)
		{
			annexDocument = Argument.NotNull(doc, nameof(doc));
			this.docDescription = docDescription;
			this.endOfAnnexes = endOfAnnexes;
		}

		public AnnexSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, IeDoc doc, ZString docDescription) : this(cusEntryHeader, certificateData, doc, docDescription, false) { }

		readonly IeDoc annexDocument;
		readonly ZBool endOfAnnexes;
		readonly ZString docDescription;

		public IAnnexHeader Header => header ?? (header = new AnnexHeaderWrapper(entryHeader, endOfAnnexes));
		AnnexHeaderWrapper header;

		public IAnnexDocCommon Document => document ?? (document = new AnnexDocCommonWrapper(annexDocument, docDescription));
		AnnexDocCommonWrapper document;
	}
}
