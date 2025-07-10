using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class CustomsDocDataObjectProvider : CustomsDocDataObjectProvider<CusEntryHeader>
	{
		protected override IEURCertificateOfOrigin GetEURCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
		{
			return new EURCertificateOfOriginWrapper(entryHeader);
		}

		protected override IDV1Certificate GetDV1CertificateForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new DV1CertificateWrapper(entryHeader);

		protected override IATRCertificateOfOrigin GetATRCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
		{
			return new ATRCertificateOfOriginWrapper(entryHeader);
		}
	}
}
