using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class CustomsDocDataObjectProvider : EU.Business.Documents.DocDataObjects.CustomsDocDataObjectProvider<CusEntryHeader>
	{
		protected override EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new EURCertificateOfOriginWrapper(entryHeader);

		protected override object GetFromEntryHeader(CusEntryHeader entryHeader, string dataContext, IDocDataObjectParameters parameters)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				return base.GetFromEntryHeader(entryHeader, dataContext, parameters);
			}
		}

		protected override EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetDV1CertificateForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => new DV1CertificateWrapper(entryHeader);

		protected override EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin GetATRCertificateOfOriginForEntry(CusEntryHeader entryHeader, IDocDataObjectParameters parameters)
		{
			return new ATRCertificateOfOriginWrapper(entryHeader);
		}
	}
}
